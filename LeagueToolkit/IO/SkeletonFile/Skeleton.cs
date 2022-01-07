using System.Diagnostics;
using System.Text;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.IO.AnimationFile;

namespace LeagueToolkit.IO.SkeletonFile;

public class Skeleton
{
    internal const int FormatToken = 0x22FD4FC3; // FNV hash of the format token string

    public Skeleton(IList<SkeletonJoint> joints, IList<short> influenceMap)
    {
        Joints = joints;
        Influences = influenceMap;
        foreach (var joint in Joints)
            if (joint.ParentId == -1)
                joint.GlobalTransform = joint.LocalTransform;
            else
                joint.GlobalTransform = Joints[joint.ParentId].GlobalTransform * joint.LocalTransform;
    }

    public Skeleton(string filePath) : this(File.OpenRead(filePath))
    {
    }

    public Skeleton(Stream stream, bool leaveOpen = false)
    {
        using var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen);
        br.BaseStream.Seek(4, SeekOrigin.Begin);
        var magic = br.ReadUInt32();
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        if (magic == FormatToken)
        {
            Read(br);
            return;
        }

        ReadLegacy(br);
    }

    public IList<SkeletonJoint> RootJoints { get; } = new List<SkeletonJoint>();
    public IList<SkeletonJoint> Joints { get; } = new List<SkeletonJoint>();
    public IList<short> Influences { get; } = new List<short>();
    public string Name { get; private set; } = string.Empty;
    public string AssetName { get; private set; } = string.Empty;

    private void Read(BinaryReader br)
    {
        var fileSize = br.ReadUInt32();
        var formatToken = br.ReadUInt32();
        var version = br.ReadUInt32();
        if (version != 0) throw new UnsupportedFileVersionException();

        var flags = br.ReadUInt16();

        var jointCount = br.ReadUInt16();
        var influencesCount = br.ReadUInt32();

        var jointsOffset = br.ReadInt32();
        var jointIndicesOffset = br.ReadInt32();
        var influencesOffset = br.ReadInt32();
        var nameOffset = br.ReadInt32();
        var assetNameOffset = br.ReadInt32();
        var boneNamesOffset = br.ReadInt32();
        var reservedOffset1 = br.ReadInt32();
        var reservedOffset2 = br.ReadInt32();
        var reservedOffset3 = br.ReadInt32();
        var reservedOffset4 = br.ReadInt32();
        var reservedOffset5 = br.ReadInt32();

        // Joints
        if (jointsOffset > 0)
        {
            br.BaseStream.Seek(jointsOffset, SeekOrigin.Begin);
            for (var i = 0; i < jointCount; i++)
            {
                var joint = new SkeletonJoint(br);
                Joints.Add(joint);
                if (joint.ParentId == -1)
                    RootJoints.Add(joint);
                else
                    Joints[joint.ParentId].Children.Add(joint);
            }
        }

        // Joint indices
        if (jointIndicesOffset > 0)
        {
            br.BaseStream.Seek(jointIndicesOffset, SeekOrigin.Begin);
            for (var i = 0; i < jointCount; i++)
            {
                var index = br.ReadInt16();
                var padding = br.ReadInt16(); // Might not be padding: not always 0.
                var hash = br.ReadUInt32();

                Debug.Assert(Joints[index].Hash == hash && Cryptography.ElfHash(Joints[index].Name) == hash);
            }
        }
        else
        {
            for (var i = 0; i < jointCount; i++) Joints[i].Hash = Cryptography.ElfHash(Joints[i].Name);
        }

        // Influences
        if (influencesOffset > 0)
        {
            br.BaseStream.Seek(influencesOffset, SeekOrigin.Begin);
            for (var i = 0; i < influencesCount; i++) Influences.Add(br.ReadInt16());
        }

        // Name
        if (nameOffset > 0)
        {
            br.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
            Name = br.ReadZeroTerminatedString();
        }

        // Asset name
        if (assetNameOffset > 0)
        {
            br.BaseStream.Seek(assetNameOffset, SeekOrigin.Begin);
            AssetName = br.ReadZeroTerminatedString();
        }

        Debug.Assert(reservedOffset1 == -1 && reservedOffset2 == -1 && reservedOffset3 == -1 && reservedOffset4 == -1 &&
            reservedOffset5 == -1 || reservedOffset1 == 0 && reservedOffset2 == 0 && reservedOffset3 == 0 &&
            reservedOffset4 == 0 && reservedOffset5 == 0);
    }

    private void ReadLegacy(BinaryReader br)
    {
        var magic = new string(br.ReadChars(8));
        if (magic != "r3d2sklt") throw new InvalidFileSignatureException();

        var version = br.ReadUInt32();
        if (version != 1 && version != 2) throw new UnsupportedFileVersionException();

        var skeletonId = br.ReadUInt32();

        var jointCount = br.ReadUInt32();
        for (var i = 0; i < jointCount; i++)
        {
            var joint = new SkeletonJoint(br, (short) i);
            Joints.Add(joint);
            if (joint.ParentId == -1)
            {
                RootJoints.Add(joint);
                joint.LocalTransform = joint.GlobalTransform;
            }
            else
            {
                var parent = Joints[joint.ParentId];
                parent.Children.Add(joint);
                joint.LocalTransform = joint.GlobalTransform * parent.InverseBindTransform;
            }
        }

        switch (version)
        {
            case 2:
                var influencesCount = br.ReadUInt32();
                for (var i = 0; i < influencesCount; i++) Influences.Add((short) br.ReadUInt32());
                break;
            case 1:
                for (var i = 0; i < Joints.Count; i++) Influences.Add((short) i);
                break;
        }
    }

    public IDictionary<AnimationTrack, SkeletonJoint> MapTracksToJoints(Animation animation)
    {
        var result = new Dictionary<AnimationTrack, SkeletonJoint>();

        var roots = RootJoints.ToList();
        var remainingTracks = animation.Tracks.ToList();

        while (roots.Count != 0)
        {
            MapAllTracksInOrder(result, remainingTracks, roots);
            var count = roots.Count;
            for (var i = 0; i < count; i++)
            {
                roots.AddRange(roots[0].Children);
                roots.RemoveAt(0);
            }
        }

        Debug.Assert(remainingTracks.All(t => Joints.All(j => t.JointNameHash != j.Hash)));

        return result;
    }

    private static void MapAllTracksInOrder(IDictionary<AnimationTrack, SkeletonJoint> result,
        IList<AnimationTrack> remainingTracks, List<SkeletonJoint> roots)
    {
        var change = true;
        while (change)
        {
            change = false;
            for (var i = 0; i < remainingTracks.Count; i++)
            {
                var track = remainingTracks[i];

                SkeletonJoint applicableJoint = null;
                for (var j = 0; j < roots.Count; j++)
                {
                    var joint = roots[j];
                    if (joint.Hash != track.JointNameHash) continue;

                    applicableJoint = joint;
                    roots.RemoveAt(j);
                    roots.AddRange(joint.Children);
                    break;
                }

                if (applicableJoint == null) continue;

                change = true;
                result[track] = applicableJoint;
                remainingTracks.RemoveAt(i);
                i--;
            }
        }
    }

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream)
    {
        using var bw = new BinaryWriter(stream);
        bw.Seek(4, SeekOrigin.Current); // File size
        bw.Write(FormatToken);
        bw.Write(0); // Version
        bw.Write((ushort) 0); // Flags
        bw.Write((ushort) Joints.Count);
        bw.Write(Influences.Count);

        var jointsSectionSize = Joints.Count *
                                (2 * sizeof(ushort) + 2 * sizeof(short) + sizeof(uint) + 21 * sizeof(float) +
                                 sizeof(int));
        var jointIndicesSectionSize = Joints.Count * (2 * sizeof(short) + sizeof(uint));
        var influencesSectionSize = Influences.Count * sizeof(short);
        const int jointsOffset = 64;
        var jointIndicesOffset = jointsOffset + jointsSectionSize;
        var influencesOffset = jointIndicesOffset + jointIndicesSectionSize;

        bw.Write(jointsOffset); // Joints offset
        bw.Write(jointIndicesOffset);
        bw.Write(influencesOffset);

        var nameOffsetOffset = bw.BaseStream.Position;
        bw.Seek(4, SeekOrigin.Current); // Name offset

        var assetNameOffsetOffset = bw.BaseStream.Position;
        bw.Seek(4, SeekOrigin.Current); // Asset name offset

        var jointNamesOffsetOffset = bw.BaseStream.Position;
        bw.Seek(4, SeekOrigin.Current); // Joint names offset
        bw.Write(0xFFFFFFFF); // Write reserved offset field
        bw.Write(0xFFFFFFFF); // Write reserved offset field
        bw.Write(0xFFFFFFFF); // Write reserved offset field
        bw.Write(0xFFFFFFFF); // Write reserved offset field
        bw.Write(0xFFFFFFFF); // Write reserved offset field

        // Name
        bw.Seek(influencesOffset + influencesSectionSize, SeekOrigin.Begin);
        var nameOffset = bw.WriteZeroTerminatedString(Name);

        // Asset name
        var assetNameOffset = bw.WriteZeroTerminatedString(AssetName);

        // Joint names
        var jointNameOffsets = new long[Joints.Count];
        var jointNamesOffset = bw.BaseStream.Position;
        for (var i = 0; i < Joints.Count; i++) jointNameOffsets[i] = bw.WriteZeroTerminatedString(Joints[i].Name);

        bw.Seek(jointsOffset, SeekOrigin.Begin);
        for (var i = 0; i < Joints.Count; i++) Joints[i].Write(bw, jointNameOffsets[i]);

        bw.Seek(influencesOffset, SeekOrigin.Begin);
        foreach (var influence in Influences) bw.Write(influence);

        bw.Seek(jointIndicesOffset, SeekOrigin.Begin);
        foreach (var joint in Joints)
        {
            bw.Write(joint.Id);
            bw.Write((ushort) 0);
            bw.Write(joint.Hash);
        }

        // Write name offset to header
        bw.BaseStream.Seek(nameOffsetOffset, SeekOrigin.Begin);
        bw.Write((int) nameOffset);

        // Write asset name offset to header
        bw.BaseStream.Seek(assetNameOffsetOffset, SeekOrigin.Begin);
        bw.Write((int) assetNameOffset);

        // Write joint names offset to header
        bw.BaseStream.Seek(jointNamesOffsetOffset, SeekOrigin.Begin);
        bw.Write((int) jointNamesOffset);

        // Write file size to header
        var fileSize = (uint) bw.BaseStream.Length;
        bw.BaseStream.Seek(0, SeekOrigin.Begin);
        bw.Write(fileSize);
    }
}
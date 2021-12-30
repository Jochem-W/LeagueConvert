using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.SkeletonFile;

public class Skeleton
{
    internal const int FORMAT_TOKEN = 0x22FD4FC3; // FNV hash of the format token string

    public Skeleton(List<SkeletonJoint> joints, List<short> influenceMap)
    {
        Joints = joints;
        Influences = influenceMap;

        foreach (var joint in Joints)
            if (joint.ParentID == -1)
                joint.GlobalTransform = joint.LocalTransform;
            else
                joint.GlobalTransform = Joints[joint.ParentID].GlobalTransform * joint.LocalTransform;
    }

    public Skeleton(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public Skeleton(Stream stream, bool leaveOpen = false)
    {
        using (var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen))
        {
            br.BaseStream.Seek(4, SeekOrigin.Begin);
            var magic = br.ReadUInt32();
            br.BaseStream.Seek(0, SeekOrigin.Begin);

            if (magic == 0x22FD4FC3)
            {
                IsLegacy = false;
                ReadNew(br);
            }
            else
            {
                IsLegacy = true;
                ReadLegacy(br);
            }
        }
    }

    public bool IsLegacy { get; }

    public List<SkeletonJoint> Joints { get; } = new();
    public List<short> Influences { get; } = new();
    public string Name { get; private set; } = string.Empty;
    public string AssetName { get; private set; } = string.Empty;

    private void ReadNew(BinaryReader br)
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

        if (jointsOffset > 0 && jointCount != 0) //wesmart
        {
            br.BaseStream.Seek(jointsOffset, SeekOrigin.Begin);

            for (var i = 0; i < jointCount; i++) Joints.Add(new SkeletonJoint(br, false));
        }

        if (influencesOffset > 0 && influencesCount != 0)
        {
            br.BaseStream.Seek(influencesOffset, SeekOrigin.Begin);

            for (var i = 0; i < influencesCount; i++) Influences.Add(br.ReadInt16());
        }

        if (jointIndicesOffset > 0 && jointCount != 0)
        {
            br.BaseStream.Seek(jointIndicesOffset, SeekOrigin.Begin);

            for (var i = 0; i < jointCount; i++)
            {
                var index = br.ReadInt16();
                br.ReadInt16(); //pad
                var hash = br.ReadUInt32();
            }
        }

        if (nameOffset > 0)
        {
            br.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
            Name = br.ReadZeroTerminatedString();
        }

        if (assetNameOffset > 0)
        {
            br.BaseStream.Seek(assetNameOffset, SeekOrigin.Begin);
            AssetName = br.ReadZeroTerminatedString();
        }
    }

    private void ReadLegacy(BinaryReader br)
    {
        var magic = Encoding.ASCII.GetString(br.ReadBytes(8));
        if (magic != "r3d2sklt") throw new InvalidFileSignatureException();

        var version = br.ReadUInt32();
        if (version != 1 && version != 2) throw new UnsupportedFileVersionException();

        var skeletonID = br.ReadUInt32();

        var jointCount = br.ReadUInt32();
        for (var i = 0; i < jointCount; i++) Joints.Add(new SkeletonJoint(br, true, (short) i));

        if (version == 2)
        {
            var influencesCount = br.ReadUInt32();
            for (var i = 0; i < influencesCount; i++) Influences.Add((short) br.ReadUInt32());
        }
        else if (version == 1)
        {
            for (var i = 0; i < Joints.Count; i++) Influences.Add((short) i);
        }

        // Derive Local transformations
        foreach (var joint in Joints)
            if (joint.ParentID == -1)
            {
                joint.LocalTransform = joint.GlobalTransform;
            }
            else
            {
                var parent = Joints[joint.ParentID];

                joint.LocalTransform = joint.GlobalTransform * parent.InverseBindTransform;
            }
    }

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream)
    {
        using (var bw = new BinaryWriter(stream))
        {
            bw.Write(0); //File Size, will Seek to start and write it at the end
            bw.Write(FORMAT_TOKEN);
            bw.Write(0); //Version
            bw.Write((ushort) 0); //Flags
            bw.Write((ushort) Joints.Count);
            bw.Write(Influences.Count);

            var jointsSectionSize = Joints.Count * 100;
            var jointIndicesSectionSize = Joints.Count * 8;
            var influencesSectionSize = Influences.Count * 2;
            var jointsOffset = 64;
            var jointIndicesOffset = jointsOffset + jointsSectionSize;
            var influencesOffset = jointIndicesOffset + jointIndicesSectionSize;
            var jointNamesOffset = influencesOffset + influencesSectionSize;

            bw.Write(jointsOffset); //Joints Offset
            bw.Write(jointIndicesOffset);
            bw.Write(influencesOffset);

            var nameOffsetOffset = bw.BaseStream.Position;
            bw.Seek(4, SeekOrigin.Current); //Name offset

            var assetNameOffsetOffset = bw.BaseStream.Position;
            bw.Seek(4, SeekOrigin.Current); //Asset Name offset

            bw.Write(jointNamesOffset);
            bw.Write(0xFFFFFFFF); //Write reserved offset field
            bw.Write(0xFFFFFFFF); //Write reserved offset field
            bw.Write(0xFFFFFFFF); //Write reserved offset field
            bw.Write(0xFFFFFFFF); //Write reserved offset field
            bw.Write(0xFFFFFFFF); //Write reserved offset field

            var jointNameOffsets = new Dictionary<int, int>();
            bw.Seek(jointNamesOffset, SeekOrigin.Begin);
            for (var i = 0; i < Joints.Count; i++)
            {
                jointNameOffsets.Add(i, (int) bw.BaseStream.Position);

                bw.Write(Encoding.ASCII.GetBytes(Joints[i].Name));
                bw.Write((byte) 0);
            }

            bw.Seek(jointsOffset, SeekOrigin.Begin);
            for (var i = 0; i < Joints.Count; i++) Joints[i].Write(bw, jointNameOffsets[i]);

            bw.Seek(influencesOffset, SeekOrigin.Begin);
            foreach (var influence in Influences) bw.Write(influence);

            bw.Seek(jointIndicesOffset, SeekOrigin.Begin);
            foreach (var joint in Joints)
            {
                bw.Write(Cryptography.ElfHash(joint.Name));
                bw.Write((ushort) 0);
                bw.Write(joint.ID);
            }

            bw.BaseStream.Seek(0, SeekOrigin.End);
            // Write Name
            var nameOffset = bw.BaseStream.Position;
            bw.Write(Encoding.ASCII.GetBytes(Name));
            bw.Write((byte) 0);

            // Write Asset Name
            var assetNameOffset = bw.BaseStream.Position;
            bw.Write(Encoding.ASCII.GetBytes(AssetName));
            bw.Write((byte) 0);

            // Write Name offset to header
            bw.BaseStream.Seek(nameOffsetOffset, SeekOrigin.Begin);
            bw.Write((int) nameOffset);

            // Write Asset Name offset to header
            bw.BaseStream.Seek(assetNameOffsetOffset, SeekOrigin.Begin);
            bw.Write((int) assetNameOffset);

            var fileSize = (uint) bw.BaseStream.Length;
            bw.BaseStream.Seek(0, SeekOrigin.Begin);
            bw.Write(fileSize);
        }
    }
}
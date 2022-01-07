using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.SkeletonFile;

public class SkeletonJoint
{
    private Matrix4x4 _globalTransform;
    private Matrix4x4 _inverseBindTransform;

    internal SkeletonJoint(short id, short parentId, string name, Vector3 localTranslation, Vector3 localScale,
        Quaternion localRotation)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        LocalTransform = Compose(localTranslation, localScale, Quaternion.Normalize(localRotation));
    }

    internal SkeletonJoint(BinaryReader br)
    {
        Read(br);
    }

    internal SkeletonJoint(BinaryReader br, short id)
    {
        ReadLegacy(br, id);
    }

    public ushort Flags { get; private set; }
    public short Id { get; private set; }
    public short ParentId { get; private set; }
    public float Radius { get; private set; } = 2.1f;
    public string Name { get; private set; }
    public uint Hash { get; internal set; }
    public Matrix4x4 LocalTransform { get; internal set; }

    public Matrix4x4 GlobalTransform
    {
        get => _globalTransform;
        internal set
        {
            _globalTransform = value;
            _inverseBindTransform =
                Matrix4x4.Invert(_globalTransform, out var inverted) ? inverted : Matrix4x4.Identity;
        }
    }

    public Matrix4x4 InverseBindTransform
    {
        get => _inverseBindTransform;
        private set
        {
            _inverseBindTransform = value;
            _globalTransform = Matrix4x4.Invert(_inverseBindTransform, out var inverted)
                ? inverted
                : Matrix4x4.Identity;
        }
    }

    private void ReadLegacy(BinaryReader br, short id)
    {
        Id = id;
        Name = br.ReadPaddedString(32);
        ParentId = (short) br.ReadInt32();
        var scale = br.ReadSingle();

        var transform = new float[4, 4];
        transform[0, 3] = 0;
        transform[1, 3] = 0;
        transform[2, 3] = 0;
        transform[3, 3] = 1;
        for (var i = 0; i < 3; i++)
        for (var j = 0; j < 4; j++)
            transform[j, i] = br.ReadSingle();
        
        GlobalTransform = new Matrix4x4
        {
            M11 = transform[0, 0],
            M12 = transform[0, 1],
            M13 = transform[0, 2],
            M14 = transform[0, 3],

            M21 = transform[1, 0],
            M22 = transform[1, 1],
            M23 = transform[1, 2],
            M24 = transform[1, 3],

            M31 = transform[2, 0],
            M32 = transform[2, 1],
            M33 = transform[2, 2],
            M34 = transform[2, 3],

            M41 = transform[3, 0],
            M42 = transform[3, 1],
            M43 = transform[3, 2],
            M44 = transform[3, 3]
        };
    }

    private void Read(BinaryReader br)
    {
        Flags = br.ReadUInt16();
        Id = br.ReadInt16();
        ParentId = br.ReadInt16();

        var padding = br.ReadUInt16(); // Might not be padding: not always 0.
        Hash = br.ReadUInt32();
        Radius = br.ReadSingle();

        var localTranslation = br.ReadVector3();
        var localScale = br.ReadVector3();
        var localRotation = Quaternion.Normalize(br.ReadQuaternion());
        var localTransform = Compose(localTranslation, localScale, localRotation);
        localTransform *= 1 / localTransform.M44; // Is this necessary?
        LocalTransform = localTransform;

        var inverseGlobalTranslation = br.ReadVector3();
        var inverseGlobalScale = br.ReadVector3();
        var inverseGlobalRotation = Quaternion.Normalize(br.ReadQuaternion());
        var inverseBindTransform = Compose(inverseGlobalTranslation, inverseGlobalScale, inverseGlobalRotation);
        inverseBindTransform *= 1 / inverseBindTransform.M44; // Is this necessary?
        InverseBindTransform = inverseBindTransform;

        var nameOffset = br.ReadInt32();
        var position = br.BaseStream.Position;

        br.BaseStream.Seek(nameOffset - 4, SeekOrigin.Current);
        Name = br.ReadZeroTerminatedString();
        br.BaseStream.Seek(position, SeekOrigin.Begin);
    }

    private static Matrix4x4 Compose(Vector3 translation, Vector3 scale, Quaternion rotation)
    {
        var translationMatrix = Matrix4x4.CreateTranslation(translation);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);
        var scaleMatrix = Matrix4x4.CreateScale(scale);
        
        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    internal void Write(BinaryWriter bw, long nameOffset)
    {
        bw.Write(Flags);
        bw.Write(Id);
        bw.Write(ParentId);
        bw.Write((ushort) 0); // Padding
        bw.Write(Hash);
        bw.Write(Radius);
        WriteLocalTransform(bw);
        WriteInverseGlobalTransform(bw);
        bw.Write((int) (nameOffset - bw.BaseStream.Position));
    }

    private void WriteLocalTransform(BinaryWriter bw)
    {
        bw.WriteVector3(LocalTransform.Translation);
        bw.WriteVector3(LocalTransform.GetScale());
        bw.WriteQuaternion(Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(LocalTransform)));
    }

    private void WriteInverseGlobalTransform(BinaryWriter bw)
    {
        bw.WriteVector3(InverseBindTransform.Translation);
        bw.WriteVector3(InverseBindTransform.GetScale());
        bw.WriteQuaternion(Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(InverseBindTransform)));
    }
}
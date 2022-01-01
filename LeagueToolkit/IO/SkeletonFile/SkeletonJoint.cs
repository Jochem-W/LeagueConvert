using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Cryptography;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.IO.SkeletonFile;

public class SkeletonJoint
{
    internal SkeletonJoint(short id, short parentId, string name, Vector3 localPosition, Vector3 localScale,
        Quaternion localRotation)
    {
        ID = id;
        ParentID = parentId;
        Name = name;
        LocalTransform = ComposeLocal(localPosition, localScale, localRotation);
    }

    internal SkeletonJoint(BinaryReader br, bool isLegacy, short id = 0)
    {
        IsLegacy = isLegacy;

        if (isLegacy)
            ReadLegacy(br, id);
        else
            ReadNew(br);
    }

    public bool IsLegacy { get; }

    public ushort Flags { get; private set; }
    public short ID { get; private set; }
    public short ParentID { get; private set; }
    public float Radius { get; private set; } = 2.1f;
    public string Name { get; private set; }
    public Matrix4x4 LocalTransform { get; internal set; }
    public Matrix4x4 GlobalTransform { get; internal set; }

    public Matrix4x4 InverseBindTransform
    {
        get
        {
            Matrix4x4.Invert(GlobalTransform, out var inverted);
            return inverted;
        }
    }

    private void ReadLegacy(BinaryReader br, short id = 0)
    {
        ID = id;
        Name = br.ReadPaddedString(32);
        ParentID = (short) br.ReadInt32();
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

    private void ReadNew(BinaryReader br)
    {
        Flags = br.ReadUInt16();
        ID = br.ReadInt16();
        ParentID = br.ReadInt16();
        br.ReadInt16(); //padding
        var nameHash = br.ReadUInt32();
        Radius = br.ReadSingle();

        var localTranslation = br.ReadVector3();
        var localScale = br.ReadVector3();
        var localRotation = br.ReadQuaternion();
        LocalTransform = ComposeLocal(localTranslation, localScale, localRotation);

        var inverseGlobalTranslation = br.ReadVector3();
        var inverseGlobalScale = br.ReadVector3();
        var inverseGlobalRotation = br.ReadQuaternion();
        GlobalTransform = ComposeGlobal(inverseGlobalTranslation, inverseGlobalScale, inverseGlobalRotation);

        var nameOffset = br.ReadInt32();
        var returnOffset = br.BaseStream.Position;

        br.BaseStream.Seek(returnOffset - 4 + nameOffset, SeekOrigin.Begin);
        Name = br.ReadZeroTerminatedString();
        br.BaseStream.Seek(returnOffset, SeekOrigin.Begin);
    }

    private Matrix4x4 ComposeLocal(Vector3 translation, Vector3 scale, Quaternion rotation)
    {
        var translationMatrix = Matrix4x4.CreateTranslation(translation);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);
        var scaleMatrix = Matrix4x4.CreateScale(scale);

        return scaleMatrix * rotationMatrix * translationMatrix;
    }

    private Matrix4x4 ComposeGlobal(Vector3 translation, Vector3 scale, Quaternion rotation)
    {
        var translationMatrix = Matrix4x4.CreateTranslation(translation);
        var rotationMatrix = Matrix4x4.CreateFromQuaternion(rotation);
        var scaleMatrix = Matrix4x4.CreateScale(scale);

        Matrix4x4.Invert(scaleMatrix * rotationMatrix * translationMatrix, out var global);

        return global;
    }

    internal void Write(BinaryWriter bw, int nameOffset)
    {
        bw.Write(Flags);
        bw.Write(ID);
        bw.Write(ParentID);
        bw.Write((ushort) 0); // pad
        bw.Write(Cryptography.ElfHash(Name));
        bw.Write(Radius);
        WriteLocalTransform(bw);
        WriteInverseGlobalTransform(bw);
        bw.Write(nameOffset - (int) bw.BaseStream.Position);
    }

    private void WriteLocalTransform(BinaryWriter bw)
    {
        bw.WriteVector3(LocalTransform.Translation);
        bw.WriteVector3(LocalTransform.GetScale());
        bw.WriteQuaternion(Quaternion.CreateFromRotationMatrix(LocalTransform));
    }

    private void WriteInverseGlobalTransform(BinaryWriter bw)
    {
        var inverse = InverseBindTransform;

        bw.WriteVector3(inverse.Translation);
        bw.WriteVector3(inverse.GetScale());
        bw.WriteQuaternion(Quaternion.CreateFromRotationMatrix(inverse));
    }
}
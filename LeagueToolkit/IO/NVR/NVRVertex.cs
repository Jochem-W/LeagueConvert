using System.IO;
using System.Numerics;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.NVR;

public class NVRVertex
{
    public const int Size = 12;
    public const NVRVertexType Type = NVRVertexType.NVRVERTEX;

    public NVRVertex(BinaryReader br)
    {
        Position = br.ReadVector3();
    }

    public NVRVertex(Vector3 position)
    {
        Position = position;
    }

    public Vector3 Position { get; }

    public virtual void Write(BinaryWriter bw)
    {
        bw.WriteVector3(Position);
    }

    public virtual NVRVertexType GetVertexType()
    {
        return Type;
    }

    public virtual int GetSize()
    {
        return Size;
    }

    private static bool ContainsGroundKeyword(string texture)
    {
        return texture.Contains("_floor") || texture.Contains("_dirt") || texture.Contains("grass") ||
               texture.Contains("RiverBed") || texture.Contains("_project") || texture.Contains("tile_");
    }

    public static bool IsGroundType(NVRMaterial mat)
    {
        return mat.Flags.HasFlag(NVRMaterialFlags.GroundVertex) && ContainsGroundKeyword(mat.Channels[0].Name);
    }

    public static NVRVertexType GetVertexTypeFromMaterial(NVRMaterial mat)
    {
        if (mat.Type == NVRMaterialType.MATERIAL_TYPE_FOUR_BLEND)
            return NVRVertexType.NVRVERTEX_12;
        if (mat.Type == NVRMaterialType.MATERIAL_TYPE_DEFAULT && mat.Flags.HasFlag(NVRMaterialFlags.ColoredVertex))
            return NVRVertexType.NVRVERTEX_8;
        return NVRVertexType.NVRVERTEX_4;
    }
}

public class NVRVertex4 : NVRVertex
{
    public new const int Size = 36;
    public new const NVRVertexType Type = NVRVertexType.NVRVERTEX_4;

    public NVRVertex4(BinaryReader br) : base(br)
    {
        Normal = br.ReadVector3();
        UV = br.ReadVector2();
        DiffuseColor = br.ReadColor(ColorFormat.BgraU8);
    }

    public NVRVertex4(Vector3 position) : base(position)
    {
    }

    public NVRVertex4(Vector3 position, Vector3 normal, Vector2 UV, Color diffuseColor) : base(position)
    {
        Normal = normal;
        this.UV = UV;
        DiffuseColor = diffuseColor;
    }

    public Vector3 Normal { get; set; }
    public Vector2 UV { get; set; }
    public Color DiffuseColor { get; set; }

    public override void Write(BinaryWriter bw)
    {
        bw.WriteVector3(Position);
        bw.WriteVector3(Normal);
        bw.WriteVector2(UV);

        bw.WriteColor(DiffuseColor, ColorFormat.BgraU8);
    }

    public override NVRVertexType GetVertexType()
    {
        return Type;
    }

    public override int GetSize()
    {
        return Size;
    }
}

public class NVRVertex8 : NVRVertex
{
    public new const int Size = 40;
    public new const NVRVertexType Type = NVRVertexType.NVRVERTEX_8;

    public NVRVertex8(BinaryReader br) : base(br)
    {
        Normal = br.ReadVector3();
        UV = br.ReadVector2();
        DiffuseColor = br.ReadColor(ColorFormat.BgraU8);
        EmissiveColor = br.ReadColor(ColorFormat.BgraU8);
    }

    public NVRVertex8(Vector3 position) : base(position)
    {
    }

    public NVRVertex8(Vector3 position, Vector3 normal, Vector2 UV, Color diffuseColor, Color emissiveColor) :
        base(position)
    {
        Normal = normal;
        this.UV = UV;
        DiffuseColor = diffuseColor;
        EmissiveColor = emissiveColor;
    }

    public Vector3 Normal { get; set; }
    public Vector2 UV { get; set; }
    public Color DiffuseColor { get; set; }
    public Color EmissiveColor { get; set; }

    public override void Write(BinaryWriter bw)
    {
        bw.WriteVector3(Position);
        bw.WriteVector3(Normal);
        bw.WriteVector2(UV);
        bw.WriteColor(DiffuseColor, ColorFormat.BgraU8);
        bw.WriteColor(EmissiveColor, ColorFormat.BgraU8);
    }

    public override NVRVertexType GetVertexType()
    {
        return Type;
    }

    public override int GetSize()
    {
        return Size;
    }
}

public class NVRVertex12 : NVRVertex
{
    public new const int Size = 44;
    public new const NVRVertexType Type = NVRVertexType.NVRVERTEX_12;

    public NVRVertex12(BinaryReader br) : base(br)
    {
        Normal = br.ReadVector3();
        Unknown = br.ReadVector2();
        UV = br.ReadVector2();
        DiffuseColor = br.ReadColor(ColorFormat.BgraU8);
    }

    public NVRVertex12(Vector3 position, Vector3 normal, Vector2 unknown, Vector2 UV, Color diffuseColor) :
        base(position)
    {
        Normal = normal;
        Unknown = unknown;
        this.UV = UV;
        DiffuseColor = diffuseColor;
    }

    public Vector3 Normal { get; set; }
    public Vector2 Unknown { get; set; }
    public Vector2 UV { get; set; }
    public Color DiffuseColor { get; set; }

    public override void Write(BinaryWriter bw)
    {
        bw.WriteVector3(Position);
        bw.WriteVector3(Normal);
        bw.WriteVector2(Unknown);
        bw.WriteVector2(UV);
        bw.WriteColor(DiffuseColor, ColorFormat.BgraU8);
    }

    public override NVRVertexType GetVertexType()
    {
        return Type;
    }

    public override int GetSize()
    {
        return Size;
    }
}

public enum NVRVertexType
{
    NVRVERTEX,
    NVRVERTEX_4,
    NVRVERTEX_8,
    NVRVERTEX_12
}
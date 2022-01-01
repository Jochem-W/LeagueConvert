using System;
using System.IO;

namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometryVertexElement : IEquatable<MapGeometryVertexElement>
{
    public MapGeometryVertexElement(MapGeometryVertexElementName name, MapGeometryVertexElementFormat format)
    {
        Name = name;
        Format = format;
    }

    public MapGeometryVertexElement(BinaryReader br)
    {
        Name = (MapGeometryVertexElementName) br.ReadUInt32();
        Format = (MapGeometryVertexElementFormat) br.ReadUInt32();
    }

    public MapGeometryVertexElementName Name { get; set; }
    public MapGeometryVertexElementFormat Format { get; set; }

    public bool Equals(MapGeometryVertexElement other)
    {
        return Name == other.Name && Format == other.Format;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write((uint) Name);
        bw.Write((uint) Format);
    }

    public int GetElementSize()
    {
        var size = 0;

        if (Format == MapGeometryVertexElementFormat.XYZ_Float32)
            size = 12;
        else if (Format == MapGeometryVertexElementFormat.XY_Float32)
            size = 8;
        else if (Format == MapGeometryVertexElementFormat.BGRA_Packed8888) size = 4;

        return size;
    }
}

public enum MapGeometryVertexElementName : uint
{
    Position,
    BlendWeight,
    Normal,
    PrimaryColor,
    SecondaryColor,
    FogCoordinate,
    BlendIndex,
    DiffuseUV,
    Texcoord1,
    Texcoord2,
    Texcoord3,
    Texcoord4,
    Texcoord5,
    Texcoord6,
    LightmapUV,
    StreamIndexCount
}

public enum MapGeometryVertexElementFormat : uint
{
    X_Float32,
    XY_Float32,
    XYZ_Float32,
    XYZW_Float32,
    BGRA_Packed8888,
    ZYXW_Packed8888,
    RGBA_Packed8888,
    XYZW_Packed8888
}
using System.Numerics;
using LeagueToolkit.Helpers;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometryVertex
{
    public MapGeometryVertex()
    {
    }

    public MapGeometryVertex(Vector3 position, Vector3 normal, Vector2 diffuseUV)
    {
        Position = position;
        Normal = normal;
        DiffuseUV = diffuseUV;
    }

    public MapGeometryVertex(Vector3 position, Vector3 normal, Vector2 diffuseUV, Vector2 lightmapUV) : this(position,
        normal, diffuseUV)
    {
        LightmapUV = lightmapUV;
    }

    public MapGeometryVertex(BinaryReader br, List<MapGeometryVertexElement> elements)
    {
        foreach (var element in elements)
        {
            if (element.Name == MapGeometryVertexElementName.Position)
            {
                Position = br.ReadVector3();
            }
            else if (element.Name == MapGeometryVertexElementName.Normal)
            {
                Normal = br.ReadVector3();
            }
            else if (element.Name == MapGeometryVertexElementName.DiffuseUV)
            {
                DiffuseUV = br.ReadVector2();
            }
            else if (element.Name == MapGeometryVertexElementName.LightmapUV)
            {
                LightmapUV = br.ReadVector2();
            }
            else if (element.Name == MapGeometryVertexElementName.SecondaryColor)
            {
                SecondaryColor = br.ReadColor(ColorFormat.BgraU8);
            }
            else
            {
                throw new Exception("Unknown Element Type: " + element.Name);
            }
        }
    }

    public Vector3? Position { get; set; }
    public Vector3? Normal { get; set; }
    public Vector2? DiffuseUV { get; set; }
    public Vector2? LightmapUV { get; set; }
    public Color? SecondaryColor { get; set; }

    internal byte[] ToArray(int vertexSize)
    {
        var array = new byte[vertexSize];
        var currentPosition = 0;

        if (Position != null)
        {
            Memory.CopyStructureToBuffer(array, currentPosition, Position.Value);
            currentPosition += Position.Value.RawSize();
        }

        if (Normal != null)
        {
            Memory.CopyStructureToBuffer(array, currentPosition, Normal.Value);
            currentPosition += Normal.Value.RawSize();
        }

        if (DiffuseUV != null)
        {
            Memory.CopyStructureToBuffer(array, currentPosition, DiffuseUV.Value);
            currentPosition += DiffuseUV.Value.RawSize();
        }

        if (LightmapUV != null)
        {
            Memory.CopyStructureToBuffer(array, currentPosition, LightmapUV.Value);
            currentPosition += LightmapUV.Value.RawSize();
        }

        if (SecondaryColor != null)
        {
            var colorBuffer = SecondaryColor.Value.GetBytes(ColorFormat.BgraU8);
            Buffer.BlockCopy(colorBuffer, 0, array, currentPosition, colorBuffer.Length);
            currentPosition += colorBuffer.Length;
        }

        return array;
    }

    internal int Size()
    {
        var size = 0;

        if (Position != null)
        {
            size += Position.Value.RawSize();
        }

        if (Normal != null)
        {
            size += Normal.Value.RawSize();
        }

        if (DiffuseUV != null)
        {
            size += DiffuseUV.Value.RawSize();
        }

        if (LightmapUV != null)
        {
            size += LightmapUV.Value.RawSize();
        }

        if (SecondaryColor != null)
        {
            size += Color.FormatSize(ColorFormat.BgraU8);
        }

        return size;
    }

    public static MapGeometryVertex Combine(MapGeometryVertex a, MapGeometryVertex b)
    {
        return new MapGeometryVertex
        {
            Position = a.Position == null && b.Position != null ? b.Position : a.Position,
            Normal = a.Normal == null && b.Normal != null ? b.Normal : a.Normal,
            DiffuseUV = a.DiffuseUV == null && b.DiffuseUV != null ? b.DiffuseUV : a.DiffuseUV,
            LightmapUV = a.LightmapUV == null && b.LightmapUV != null ? b.LightmapUV : a.LightmapUV,
            SecondaryColor = a.SecondaryColor == null && b.SecondaryColor != null ? b.SecondaryColor : a.SecondaryColor
        };
    }

    public void Write(BinaryWriter bw)
    {
        if (Position is Vector3 position)
        {
            bw.WriteVector3(position);
        }

        if (Normal is Vector3 normal)
        {
            bw.WriteVector3(normal);
        }

        if (DiffuseUV is Vector2 diffuseUv)
        {
            bw.WriteVector2(diffuseUv);
        }

        if (LightmapUV is Vector2 lightmapUv)
        {
            bw.WriteVector2(lightmapUv);
        }

        if (SecondaryColor is Color color)
        {
            bw.WriteColor(color, ColorFormat.BgraU8);
        }
    }
}
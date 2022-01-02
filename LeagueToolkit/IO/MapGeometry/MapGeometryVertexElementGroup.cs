namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometryVertexElementGroup : IEquatable<MapGeometryVertexElementGroup>
{
    public MapGeometryVertexElementGroup(BinaryReader br)
    {
        Usage = (MapGeometryVertexElementGroupUsage) br.ReadUInt32();

        var vertexElementCount = br.ReadUInt32();
        for (var i = 0; i < vertexElementCount; i++) VertexElements.Add(new MapGeometryVertexElement(br));

        br.BaseStream.Seek(8 * (15 - vertexElementCount), SeekOrigin.Current);
    }

    public MapGeometryVertexElementGroup(MapGeometryVertex vertex)
    {
        Usage = MapGeometryVertexElementGroupUsage.Static;

        if (vertex.Position != null)
            VertexElements.Add(new MapGeometryVertexElement(MapGeometryVertexElementName.Position,
                MapGeometryVertexElementFormat.XYZ_Float32));
        if (vertex.Normal != null)
            VertexElements.Add(new MapGeometryVertexElement(MapGeometryVertexElementName.Normal,
                MapGeometryVertexElementFormat.XYZ_Float32));
        if (vertex.DiffuseUV != null)
            VertexElements.Add(new MapGeometryVertexElement(MapGeometryVertexElementName.DiffuseUV,
                MapGeometryVertexElementFormat.XY_Float32));
        if (vertex.LightmapUV != null)
            VertexElements.Add(new MapGeometryVertexElement(MapGeometryVertexElementName.LightmapUV,
                MapGeometryVertexElementFormat.XY_Float32));
        if (vertex.SecondaryColor != null)
            VertexElements.Add(new MapGeometryVertexElement(MapGeometryVertexElementName.SecondaryColor,
                MapGeometryVertexElementFormat.BGRA_Packed8888));
    }

    public MapGeometryVertexElementGroupUsage Usage { get; }
    public List<MapGeometryVertexElement> VertexElements { get; } = new();

    public bool Equals(MapGeometryVertexElementGroup other)
    {
        var result = false;

        if (Usage != other.Usage) return false;

        if (VertexElements.Count == other.VertexElements.Count)
            for (var i = 0; i < VertexElements.Count; i++)
                result = VertexElements[i].Equals(other.VertexElements[i]);
        else
            return false;


        return result;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write((uint) Usage);
        bw.Write(VertexElements.Count);

        foreach (var vertexElement in VertexElements) vertexElement.Write(bw);

        for (var i = 0; i < 15 - VertexElements.Count; i++)
            new MapGeometryVertexElement(MapGeometryVertexElementName.Position,
                MapGeometryVertexElementFormat.XYZW_Float32).Write(bw);
    }

    public int GetVertexSize()
    {
        var size = 0;

        foreach (var vertexElement in VertexElements) size += vertexElement.GetElementSize();

        return size;
    }
}

public enum MapGeometryVertexElementGroupUsage : uint
{
    /// <summary>
    ///     Static Vertex Data
    /// </summary>
    Static,

    /// <summary>
    ///     Dynamic Vertex Data (can be changed frequently)
    /// </summary>
    Dynamic,

    /// <summary>
    ///     Streaming Vertex Data (changed every frame)
    /// </summary>
    Stream
}
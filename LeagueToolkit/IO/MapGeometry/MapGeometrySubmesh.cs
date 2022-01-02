using System.Text;

namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometrySubmesh
{
    public MapGeometrySubmesh(string material, uint startIndex, uint indexCount, uint startVertex, uint vertexCount)
    {
        Material = material;
        StartIndex = startIndex;
        IndexCount = indexCount;
        StartVertex = startVertex;
        VertexCount = vertexCount;
    }

    public MapGeometrySubmesh(BinaryReader br, MapGeometryModel parent)
    {
        Parent = parent;
        Hash = br.ReadUInt32();
        Material = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
        StartIndex = br.ReadUInt32();
        IndexCount = br.ReadUInt32();
        StartVertex = br.ReadUInt32(); //MinVertex
        VertexCount = br.ReadUInt32() + 1; //MaxVertex

        if (StartVertex != 0) StartVertex--;
    }

    public MapGeometryModel Parent { get; internal set; }
    public uint Hash { get; internal set; }
    public string Material { get; set; }
    public uint StartIndex { get; }
    public uint IndexCount { get; }
    public uint StartVertex { get; }
    public uint VertexCount { get; }

    public (List<ushort>, List<MapGeometryVertex>) GetData(bool normalize = true)
    {
        return (GetIndices(normalize), GetVertices());
    }

    public List<ushort> GetIndices(bool normalize = true)
    {
        var indices = Parent.Indices.GetRange((int) StartIndex, (int) IndexCount);

        if (normalize)
        {
            var minIndex = indices.Min();

            return indices.Select(x => x -= minIndex).ToList();
        }

        return indices;
    }

    public List<MapGeometryVertex> GetVertices()
    {
        return Parent.Vertices.GetRange((int) StartVertex, (int) (VertexCount - StartVertex));
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(Hash);
        bw.Write(Material.Length);
        bw.Write(Encoding.ASCII.GetBytes(Material));
        bw.Write(StartIndex);
        bw.Write(IndexCount);
        bw.Write(StartVertex == 0 ? StartVertex : StartVertex + 1); //edit later
        bw.Write(VertexCount - 1); //edit later
    }
}
using System.Text;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkinSubmesh
{
    internal uint _indexCount;
    internal uint _startIndex;

    internal uint _startVertex;
    internal uint _vertexCount;

    public SimpleSkinSubmesh(string name, List<ushort> indices, List<SimpleSkinVertex> vertices)
    {
        Name = name;
        Indices = indices;
        Vertices = vertices;
    }

    public SimpleSkinSubmesh(BinaryReader br)
    {
        Name = Encoding.ASCII.GetString(br.ReadBytes(64)).Replace("\0", "");
        _startVertex = br.ReadUInt32();
        _vertexCount = br.ReadUInt32();
        _startIndex = br.ReadUInt32();
        _indexCount = br.ReadUInt32();
    }

    public string Name { get; set; }
    public List<SimpleSkinVertex> Vertices { get; set; }
    public List<ushort> Indices { get; set; }

    public void Write(BinaryWriter bw, uint startVertex, uint startIndex)
    {
        bw.Write(Encoding.ASCII.GetBytes(Name.PadRight(64, '\u0000')));
        bw.Write(startVertex);
        bw.Write(Vertices.Count);
        bw.Write(startIndex);
        bw.Write(Indices.Count);
    }
}
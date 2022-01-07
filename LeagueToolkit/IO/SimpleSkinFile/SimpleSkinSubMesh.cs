using System.Text;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkinSubMesh
{
    internal readonly uint IndexCount;
    internal readonly uint StartIndex;
    internal readonly uint StartVertex;
    internal readonly uint VertexCount;

    public SimpleSkinSubMesh(string name, IList<ushort> indices, IList<SimpleSkinVertex> vertices)
    {
        Name = name;
        Indices = indices;
        Vertices = vertices;
    }

    public SimpleSkinSubMesh(BinaryReader br)
    {
        Name = Encoding.ASCII.GetString(br.ReadBytes(64)).Replace("\0", "");
        StartVertex = br.ReadUInt32();
        VertexCount = br.ReadUInt32();
        StartIndex = br.ReadUInt32();
        IndexCount = br.ReadUInt32();
    }

    public string Name { get; set; }
    public IList<SimpleSkinVertex> Vertices { get; set; }
    public IList<ushort> Indices { get; set; }

    public void Write(BinaryWriter bw, uint startVertex, uint startIndex)
    {
        var position = bw.BaseStream.Position;
        bw.Write(Encoding.ASCII.GetBytes(Name));
        var nameLength = bw.BaseStream.Position - position;
        if (nameLength < 64) bw.Seek((int) (64 - nameLength), SeekOrigin.Current);
        bw.Write(startVertex);
        bw.Write(Vertices.Count);
        bw.Write(startIndex);
        bw.Write(Indices.Count);
    }
}
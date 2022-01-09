using System.Text;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkinPrimitive
{
    public SimpleSkinPrimitive(string name, uint indexOffset, uint indexCount, uint vertexOffset, uint vertexCount)
    {
        Name = name;
        IndexOffset = indexOffset;
        IndexCount = indexCount;
        VertexOffset = vertexOffset;
        VertexCount = vertexCount;
    }

    public SimpleSkinPrimitive(BinaryReader br)
    {
        Name = Encoding.ASCII.GetString(br.ReadBytes(64)).Replace("\0", "");
        VertexOffset = br.ReadUInt32();
        VertexCount = br.ReadUInt32();
        IndexOffset = br.ReadUInt32();
        IndexCount = br.ReadUInt32();
    }

    public string Name { get; set; }
    public uint IndexCount { get; }
    public uint IndexOffset { get; }
    public uint VertexCount { get; }
    public uint VertexOffset { get; }

    public void Write(BinaryWriter bw)
    {
        var position = bw.BaseStream.Position;
        bw.Write(Encoding.ASCII.GetBytes(Name));
        var nameLength = bw.BaseStream.Position - position;
        if (nameLength < 64) bw.Seek((int) (64 - nameLength), SeekOrigin.Current);
        bw.Write(VertexOffset);
        bw.Write(VertexCount);
        bw.Write(IndexOffset);
        bw.Write(IndexCount);
    }
}
namespace LeagueToolkit.IO.NVR;

public class NVRVertexBuffer
{
    public NVRVertexBuffer(BinaryReader br)
    {
        Length = br.ReadInt32();
        Offset = br.BaseStream.Position;
        br.BaseStream.Seek(Length, SeekOrigin.Current);
    }

    public NVRVertexBuffer(NVRVertexType type)
    {
        Type = type;
    }

    public int Length { get; }
    public long Offset { get; }

    //Used for write stuff
    public NVRVertexType Type { get; }
    public List<NVRVertex> Vertices { get; } = new();

    public void Write(BinaryWriter bw)
    {
        var bufferLength = Vertices[0].GetSize() * Vertices.Count;
        bw.Write(bufferLength);
        foreach (var vertex in Vertices)
        {
            vertex.Write(bw);
        }
    }
}
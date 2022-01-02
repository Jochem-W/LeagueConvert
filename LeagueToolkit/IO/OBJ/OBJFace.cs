namespace LeagueToolkit.IO.OBJ;

public class OBJFace
{
    public OBJFace(uint[] vertexIndices)
    {
        VertexIndices = vertexIndices;
    }

    public OBJFace(uint[] vertexIndices, uint[] uvIndices)
    {
        VertexIndices = vertexIndices;
        UVIndices = uvIndices;
    }

    public OBJFace(uint[] vertexIndices, uint[] uvIndices, uint[] normalIndices)
    {
        VertexIndices = vertexIndices;
        UVIndices = uvIndices;
        NormalIndices = normalIndices;
    }

    public uint[] VertexIndices { get; set; }
    public uint[] UVIndices { get; set; }
    public uint[] NormalIndices { get; set; }

    public void Write(StreamWriter sw)
    {
        if (UVIndices != null && NormalIndices == null)
            sw.WriteLine("f {0}/{1} {2}/{3} {4}/{5}", VertexIndices[0] + 1, UVIndices[0] + 1, VertexIndices[1] + 1,
                UVIndices[1] + 1, VertexIndices[2] + 1, UVIndices[2] + 1);
        else if (UVIndices != null && NormalIndices != null)
            sw.WriteLine("f {0}/{1}/{2} {3}/{4}/{5} {6}/{7}/{8}", VertexIndices[0] + 1, UVIndices[0] + 1,
                NormalIndices[0] + 1, VertexIndices[1] + 1, UVIndices[1] + 1, NormalIndices[1] + 1,
                VertexIndices[2] + 1, UVIndices[2] + 1, NormalIndices[2] + 1);
        else
            sw.WriteLine("f {0} {1} {2}", VertexIndices[0] + 1, VertexIndices[1] + 1, VertexIndices[2] + 1);
    }
}
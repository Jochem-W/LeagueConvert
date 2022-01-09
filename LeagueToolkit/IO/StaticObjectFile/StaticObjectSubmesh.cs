using System.Numerics;

namespace LeagueToolkit.IO.StaticObjectFile;

public class StaticObjectSubmesh
{
    public StaticObjectSubmesh(string name, List<StaticObjectVertex> vertices, List<uint> indices)
    {
        Name = name;
        Vertices = vertices;
        Indices = indices;
    }

    public string Name { get; set; }
    public List<StaticObjectVertex> Vertices { get; }
    public List<uint> Indices { get; }

    internal List<StaticObjectFace> GetFaces()
    {
        var faces = new List<StaticObjectFace>();

        for (var i = 0; i < Indices.Count; i += 3)
        {
            uint[] indices = {Indices[i], Indices[i + 1], Indices[i + 2]};
            Vector2[] uvs =
            {
                Vertices[(int) indices[0]].UV,
                Vertices[(int) indices[1]].UV,
                Vertices[(int) indices[2]].UV
            };

            faces.Add(new StaticObjectFace(indices, Name, uvs));
        }

        return faces;
    }
}
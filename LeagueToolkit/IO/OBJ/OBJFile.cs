using System.Globalization;
using System.Numerics;
using System.Text;

namespace LeagueToolkit.IO.OBJ;

public class OBJFile
{
    public OBJFile(List<Vector3> vertices, List<uint> indices)
    {
        Vertices = vertices;
        for (var i = 0; i < indices.Count; i += 3)
        {
            uint[] faceIndices = {indices[i], indices[i + 1], indices[i + 2]};
            Faces.Add(new OBJFace(faceIndices));
        }
    }

    public OBJFile(List<Vector3> vertices, List<uint> indices, List<Vector2> uvs)
    {
        Vertices = vertices;
        UVs = uvs;
        for (var i = 0; i < indices.Count; i += 3)
        {
            uint[] faceIndices = {indices[i], indices[i + 1], indices[i + 2]};
            Faces.Add(new OBJFace(faceIndices, faceIndices));
        }
    }

    public OBJFile(List<Vector3> vertices, List<uint> indices, List<Vector2> uvs, List<Vector3> normals)
    {
        Vertices = vertices;
        UVs = uvs;
        Normals = normals;
        for (var i = 0; i < indices.Count; i += 3)
        {
            uint[] faceIndices = {indices[i], indices[i + 1], indices[i + 2]};
            Faces.Add(new OBJFace(faceIndices, faceIndices, faceIndices));
        }
    }


    public OBJFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public OBJFile(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            while (!sr.EndOfStream) ReadLine(sr);
        }
    }

    public List<string> Comments { get; set; } = new();
    public List<Vector3> Vertices { get; set; } = new();
    public List<Vector2> UVs { get; set; } = new();
    public List<Vector3> Normals { get; set; } = new();
    public List<OBJFace> Faces { get; set; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            foreach (var comment in Comments) sw.WriteLine("#" + comment);
            foreach (var vertex in Vertices) sw.WriteLine("v {0} {1} {2}", vertex.X, vertex.Y, vertex.Z);
            foreach (var uv in UVs) sw.WriteLine("vt {0} {1}", uv.X, 1 - uv.Y);
            foreach (var normal in Normals) sw.WriteLine("vn {0} {1} {2}", normal.X, normal.Y, normal.Z);
            foreach (var face in Faces) face.Write(sw);
        }
    }

    private void ReadLine(StreamReader sr)
    {
        var input = sr.ReadLine().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        if (input.Length == 0) return;
        if (input[0] == "#")
        {
            Comments.Add(string.Join(" ", input).Remove(0, 1));
        }
        else if (input[0] == "v")
        {
            Vertices.Add(new Vector3(float.Parse(input[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(input[3], CultureInfo.InvariantCulture.NumberFormat)));
        }
        else if (input[0] == "vt")
        {
            UVs.Add(new Vector2(float.Parse(input[1], CultureInfo.InvariantCulture.NumberFormat),
                1 - float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat)));
        }
        else if (input[0] == "vn")
        {
            Normals.Add(new Vector3(float.Parse(input[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(input[2], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(input[3], CultureInfo.InvariantCulture.NumberFormat)));
        }
        else if (input[0] == "f")
        {
            var vertex1 = input[1].Split('/');
            var vertex2 = input[2].Split('/');
            var vertex3 = input[3].Split('/');

            if (vertex1.Length == 1)
                Faces.Add(new OBJFace(
                    new[]
                    {
                        uint.Parse(vertex1[0]) - 1,
                        uint.Parse(vertex2[0]) - 1,
                        uint.Parse(vertex3[0]) - 1
                    }));
            else if (vertex1.Length == 2)
                Faces.Add(new OBJFace(
                    new[]
                    {
                        uint.Parse(vertex1[0]) - 1,
                        uint.Parse(vertex2[0]) - 1,
                        uint.Parse(vertex3[0]) - 1
                    },
                    new[]
                    {
                        uint.Parse(vertex1[1]) - 1,
                        uint.Parse(vertex2[1]) - 1,
                        uint.Parse(vertex3[1]) - 1
                    }));
            else if (vertex1.Length == 3)
                Faces.Add(new OBJFace(
                    new[]
                    {
                        uint.Parse(vertex1[0]) - 1,
                        uint.Parse(vertex2[0]) - 1,
                        uint.Parse(vertex3[0]) - 1
                    },
                    new[]
                    {
                        uint.Parse(vertex1[1]) - 1,
                        uint.Parse(vertex2[1]) - 1,
                        uint.Parse(vertex3[1]) - 1
                    },
                    new[]
                    {
                        uint.Parse(vertex1[2]) - 1,
                        uint.Parse(vertex2[2]) - 1,
                        uint.Parse(vertex3[2]) - 1
                    }));
        }
    }
}
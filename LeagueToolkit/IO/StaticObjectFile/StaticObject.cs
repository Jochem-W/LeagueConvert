using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.StaticObjectFile;

public class StaticObject
{
    public StaticObject(List<StaticObjectSubmesh> submeshes) : this(string.Empty, submeshes)
    {
    }

    public StaticObject(string name, List<StaticObjectSubmesh> submeshes)
    {
        Name = name;
        Submeshes = submeshes;
    }

    public StaticObject(string name, List<StaticObjectSubmesh> submeshes, Vector3 pivotPoint) : this(name, submeshes)
    {
        PivotPoint = pivotPoint;
    }

    public string Name { get; set; }
    public Vector3 PivotPoint { get; set; }
    public List<StaticObjectSubmesh> Submeshes { get; }

    public static StaticObject ReadSCB(string fileLocation)
    {
        return ReadSCB(File.OpenRead(fileLocation));
    }

    public static StaticObject ReadSCB(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if (magic != "r3d2Mesh") throw new InvalidFileSignatureException();

            var major = br.ReadUInt16();
            var minor = br.ReadUInt16();
            if (major != 3 && major != 2 && minor != 1) //There are versions [2][1] and [1][1] aswell
                throw new UnsupportedFileVersionException();

            var name = Encoding.ASCII.GetString(br.ReadBytes(128)).Replace("\0", "");
            var vertexCount = br.ReadUInt32();
            var faceCount = br.ReadUInt32();
            var flags = (StaticObjectFlags) br.ReadUInt32();
            var boundingBox = new R3DBox(br);

            var hasVertexColors = false;
            if (major == 3 && minor == 2) hasVertexColors = br.ReadUInt32() == 1;

            var vertices = new List<Vector3>((int) vertexCount);
            var vertexColors = new List<Color>((int) vertexCount);
            for (var i = 0; i < vertexCount; i++) vertices.Add(br.ReadVector3());

            if (hasVertexColors)
                for (var i = 0; i < vertexCount; i++)
                    vertexColors.Add(br.ReadColor(ColorFormat.RgbaU8));

            var centralPoint = br.ReadVector3();

            var faces = new List<StaticObjectFace>((int) faceCount);
            for (var i = 0; i < faceCount; i++) faces.Add(new StaticObjectFace(br));

            return new StaticObject(name, CreateSubmeshes(vertices, vertexColors, faces), centralPoint);
        }
    }

    public static StaticObject ReadSCO(string fileLocation)
    {
        return ReadSCO(File.OpenRead(fileLocation));
    }

    public static StaticObject ReadSCO(Stream stream)
    {
        using (var sr = new StreamReader(stream))
        {
            char[] splittingArray = {' '};
            string[] input = null;

            if (sr.ReadLine() != "[ObjectBegin]") throw new InvalidFileSignatureException();

            input = sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries);
            var name = input.Length != 1 ? input[1] : string.Empty;

            input = sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries);
            var centralPoint = new Vector3(
                float.Parse(input[1], CultureInfo.InvariantCulture),
                float.Parse(input[2], CultureInfo.InvariantCulture),
                float.Parse(input[3], CultureInfo.InvariantCulture));
            var pivotPoint = centralPoint;

            var hasVertexColors = false;

            input = sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries);
            if (input[0] == "PivotPoint=")
                pivotPoint = new Vector3(
                    float.Parse(input[1], CultureInfo.InvariantCulture),
                    float.Parse(input[2], CultureInfo.InvariantCulture),
                    float.Parse(input[3], CultureInfo.InvariantCulture));
            else if (input[0] == "VertexColors=") hasVertexColors = uint.Parse(input[1]) != 0;

            var vertexCount = 0;
            if (input[0] == "Verts=")
                vertexCount = int.Parse(input[1]);
            else
                vertexCount = int.Parse(sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries)[1]);

            var vertices = new List<Vector3>(vertexCount);
            var vertexColors = new List<Color>(vertexCount);
            for (var i = 0; i < vertexCount; i++)
            {
                input = sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries);

                vertices.Add(new Vector3(
                    float.Parse(input[0], CultureInfo.InvariantCulture),
                    float.Parse(input[1], CultureInfo.InvariantCulture),
                    float.Parse(input[2], CultureInfo.InvariantCulture)));
            }

            if (hasVertexColors)
                for (var i = 0; i < vertexCount; i++)
                {
                    var colorComponents = sr.ReadLine().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (colorComponents.Length != 4)
                        throw new Exception("Invalid number of vertex color components: " + colorComponents.Length);

                    var r = byte.Parse(colorComponents[0]);
                    var g = byte.Parse(colorComponents[1]);
                    var b = byte.Parse(colorComponents[2]);
                    var a = byte.Parse(colorComponents[3]);

                    vertexColors.Add(new Color(r, g, b, a));
                }

            var faceCount = int.Parse(sr.ReadLine().Split(splittingArray, StringSplitOptions.RemoveEmptyEntries)[1]);
            var faces = new List<StaticObjectFace>(faceCount);
            for (var i = 0; i < faceCount; i++) faces.Add(new StaticObjectFace(sr));

            return new StaticObject(name, CreateSubmeshes(vertices, vertexColors, faces), pivotPoint);
        }
    }

    private static List<StaticObjectSubmesh> CreateSubmeshes(List<Vector3> vertices, List<Color> vertexColors,
        List<StaticObjectFace> faces)
    {
        var hasVertexColors = vertexColors.Count != 0;
        var submeshMap = CreateSubmeshMap(faces);
        var submeshes = new List<StaticObjectSubmesh>();

        foreach (var mappedSubmesh in submeshMap)
        {
            //Collect all indices and build UV map
            var indices = new List<uint>(mappedSubmesh.Value.Count * 3);
            var uvMap = new Dictionary<uint, Vector2>(mappedSubmesh.Value.Count * 3);
            foreach (var face in mappedSubmesh.Value)
                for (var i = 0; i < 3; i++)
                {
                    var index = face.Indices[i];

                    indices.Add(index);

                    if (!uvMap.ContainsKey(index)) uvMap.Add(index, face.UVs[i]);
                }

            //Get Vertex range from indices
            var minVertex = indices.Min();
            var maxVertex = indices.Max();

            //Build vertex list
            var vertexCount = maxVertex - minVertex;
            var submeshVertices = new List<StaticObjectVertex>((int) vertexCount);
            for (var i = minVertex; i < maxVertex + 1; i++)
            {
                var uv = uvMap[i];

                if (hasVertexColors)
                    submeshVertices.Add(new StaticObjectVertex(vertices[(int) i], uv, vertexColors[(int) i]));
                else
                    submeshVertices.Add(new StaticObjectVertex(vertices[(int) i], uv));
            }

            //Normalize indices
            for (var i = 0; i < indices.Count; i++) indices[i] -= minVertex;

            submeshes.Add(new StaticObjectSubmesh(mappedSubmesh.Key, submeshVertices, indices));
        }

        return submeshes;
    }

    private static Dictionary<string, List<StaticObjectFace>> CreateSubmeshMap(List<StaticObjectFace> faces)
    {
        var submeshMap = new Dictionary<string, List<StaticObjectFace>>();

        foreach (var face in faces)
        {
            if (!submeshMap.ContainsKey(face.Material)) submeshMap.Add(face.Material, new List<StaticObjectFace>());

            submeshMap[face.Material].Add(face);
        }

        return submeshMap;
    }

    public void WriteSCB(string fileLocation)
    {
        WriteSCB(File.Create(fileLocation));
    }

    public void WriteSCB(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            var vertices = GetVertices();
            var faces = new List<StaticObjectFace>();
            var hasVertexColors = false;
            StaticObjectFlags flags = 0;

            foreach (var submesh in Submeshes) faces.AddRange(submesh.GetFaces());

            foreach (var vertex in vertices)
                if (vertex.Color != null)
                {
                    hasVertexColors = true;
                    break;
                }

            if (hasVertexColors) flags |= StaticObjectFlags.VERTEX_COLORS;

            bw.Write(Encoding.ASCII.GetBytes("r3d2Mesh"));
            bw.Write((ushort) 3);
            bw.Write((ushort) 2);
            bw.Write(Encoding.ASCII.GetBytes(Name.PadRight(128, '\u0000')));
            bw.Write(vertices.Count);
            bw.Write(faces.Count);
            bw.Write((uint) flags);
            GetBoundingBox().Write(bw);
            bw.Write((uint) (flags & StaticObjectFlags.VERTEX_COLORS));

            vertices.ForEach(vertex => bw.WriteVector3(vertex.Position));

            if (hasVertexColors)
                foreach (var vertex in vertices)
                    if (vertex.Color.HasValue)
                        bw.WriteColor(vertex.Color.Value, ColorFormat.RgbaU8);
                    else
                        bw.WriteColor(new Color(0, 0, 0, 255), ColorFormat.RgbaU8);


            bw.WriteVector3(GetCentralPoint());
            faces.ForEach(face => face.Write(bw));
        }
    }

    public void WriteSCO(string fileLocation)
    {
        WriteSCO(File.Create(fileLocation));
    }

    public void WriteSCO(Stream stream, bool leaveOpen = false)
    {
        using (var sw = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen))
        {
            var vertices = GetVertices();
            var faces = new List<StaticObjectFace>();
            var hasVertexColors = false;

            foreach (var submesh in Submeshes) faces.AddRange(submesh.GetFaces());

            foreach (var vertex in vertices)
                if (vertex.Color != null)
                {
                    hasVertexColors = true;
                    break;
                }

            sw.WriteLine("[ObjectBegin]");
            sw.WriteLine("Name= " + Name);
            sw.WriteLine("CentralPoint= " + GetCentralPoint());

            if (PivotPoint != Vector3.Zero) sw.WriteLine("PivotPoint= " + PivotPoint);
            if (hasVertexColors) sw.WriteLine("VertexColors= 1");

            sw.WriteLine("Verts= " + vertices.Count);
            vertices.ForEach(vertex =>
            {
                sw.WriteLine("{0} {1} {2}", vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
            });

            if (hasVertexColors)
                foreach (var vertex in vertices)
                {
                    if (vertex.Color.HasValue)
                        sw.WriteColor(vertex.Color.Value, ColorFormat.RgbaU8);
                    else
                        sw.WriteColor(new Color(0, 0, 0, 255), ColorFormat.RgbaU8);

                    sw.Write('\n');
                }


            sw.WriteLine("Faces= " + faces.Count);
            faces.ForEach(face => face.Write(sw));

            sw.WriteLine("[ObjectEnd]");
        }
    }

    public List<StaticObjectVertex> GetVertices()
    {
        var vertices = new List<StaticObjectVertex>();

        foreach (var submesh in Submeshes) vertices.AddRange(submesh.Vertices);

        return vertices;
    }

    public List<uint> GetIndices()
    {
        var indices = new List<uint>();

        uint startIndex = 0;
        foreach (var submesh in Submeshes)
        {
            indices.AddRange(submesh.Indices.Select(x => x += startIndex));

            startIndex += submesh.Indices.Max();
        }

        return indices;
    }

    public R3DBox GetBoundingBox()
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var submesh in Submeshes)
        foreach (var vertex in submesh.Vertices)
        {
            if (min.X > vertex.Position.X) min.X = vertex.Position.X;
            if (min.Y > vertex.Position.Y) min.Y = vertex.Position.Y;
            if (min.Z > vertex.Position.Z) min.Z = vertex.Position.Z;
            if (max.X < vertex.Position.X) max.X = vertex.Position.X;
            if (max.Y < vertex.Position.Y) max.Y = vertex.Position.Y;
            if (max.Z < vertex.Position.Z) max.Z = vertex.Position.Z;
        }

        return new R3DBox(min, max);
    }

    public Vector3 GetCentralPoint()
    {
        return GetBoundingBox().GetCentralPoint();
    }
}

[Flags]
public enum StaticObjectFlags : uint
{
    VERTEX_COLORS = 1,
    LOCAL_ORIGIN_LOCATOR_AND_PIVOT = 2
}
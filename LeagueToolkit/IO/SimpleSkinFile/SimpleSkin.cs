using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Structures;
using LeagueToolkit.IO.StaticObjectFile;
using LeagueToolkit.IO.WGT;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkin
{
    public SimpleSkin(List<SimpleSkinSubmesh> submeshes)
    {
        Submeshes = submeshes;
    }

    public SimpleSkin(StaticObject staticObject, WGTFile weightFile)
    {
        var staticObjectIndices = staticObject.GetIndices();
        var staticObjectVertices = staticObject.GetVertices();

        var currentVertexOffset = 0;
        foreach (var submesh in staticObject.Submeshes)
        {
            // Build vertices
            List<SimpleSkinVertex> vertices = new(staticObjectVertices.Count);
            for (var i = 0; i < submesh.Vertices.Count; i++)
            {
                var vertex = submesh.Vertices[i];
                var weightData = weightFile.Weights[i + currentVertexOffset];

                vertices.Add(new SimpleSkinVertex(vertex.Position, weightData.BoneIndices, weightData.Weights,
                    Vector3.Zero, vertex.UV));
            }

            Submeshes.Add(new SimpleSkinSubmesh(submesh.Name, submesh.Indices.Select(x => (ushort) x).ToList(),
                vertices));

            currentVertexOffset += submesh.Vertices.Count;
        }
    }

    public SimpleSkin(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public SimpleSkin(Stream stream, bool leaveOpen = false)
    {
        using (var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen))
        {
            var magic = br.ReadUInt32();
            if (magic != 0x00112233) throw new InvalidFileSignatureException();

            var major = br.ReadUInt16();
            var minor = br.ReadUInt16();
            if (major != 0 && major != 2 && major != 4 && minor != 1) throw new UnsupportedFileVersionException();

            uint indexCount = 0;
            uint vertexCount = 0;
            var vertexType = SimpleSkinVertexType.Basic;
            if (major == 0)
            {
                indexCount = br.ReadUInt32();
                vertexCount = br.ReadUInt32();
            }
            else
            {
                var submeshCount = br.ReadUInt32();

                for (var i = 0; i < submeshCount; i++) Submeshes.Add(new SimpleSkinSubmesh(br));
                if (major == 4)
                {
                    var flags = br.ReadUInt32();
                }

                indexCount = br.ReadUInt32();
                vertexCount = br.ReadUInt32();

                var vertexSize = major == 4 ? br.ReadUInt32() : 52;
                vertexType = major == 4 ? (SimpleSkinVertexType) br.ReadUInt32() : SimpleSkinVertexType.Basic;
                var boundingBox = major == 4 ? new R3DBox(br) : new R3DBox(Vector3.Zero, Vector3.Zero);
                var boundingSphere = major == 4 ? new R3DSphere(br) : R3DSphere.Infinite;
            }

            var indices = new List<ushort>();
            var vertices = new List<SimpleSkinVertex>();
            for (var i = 0; i < indexCount; i++) indices.Add(br.ReadUInt16());
            for (var i = 0; i < vertexCount; i++) vertices.Add(new SimpleSkinVertex(br, vertexType));

            if (major == 0)
                Submeshes.Add(new SimpleSkinSubmesh("Base", indices, vertices));
            else
                foreach (var submesh in Submeshes)
                {
                    var submeshIndices = indices.GetRange((int) submesh._startIndex, (int) submesh._indexCount);
                    var minIndex = submeshIndices.Min();

                    submesh.Indices = submeshIndices.Select(x => x -= minIndex).ToList();
                    submesh.Vertices = vertices.GetRange((int) submesh._startVertex, (int) submesh._vertexCount);
                }
        }
    }

    public List<SimpleSkinSubmesh> Submeshes { get; } = new();

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream)
    {
        using (var bw = new BinaryWriter(stream))
        {
            bw.Write(0x00112233);
            bw.Write((ushort) 4);
            bw.Write((ushort) 1);
            bw.Write(Submeshes.Count);

            var hasVertexColors = false;
            uint indexCount = 0;
            uint vertexCount = 0;
            foreach (var submesh in Submeshes)
            {
                if (!hasVertexColors)
                    foreach (var vertex in submesh.Vertices)
                        if (vertex.Color != null)
                        {
                            hasVertexColors = true;
                            break;
                        }

                submesh.Write(bw, vertexCount, indexCount);

                indexCount += (uint) submesh.Indices.Count;
                vertexCount += (uint) submesh.Vertices.Count;
            }

            bw.Write((uint) 0); //Flags
            bw.Write(indexCount);
            bw.Write(vertexCount);
            if (hasVertexColors)
            {
                bw.Write((uint) 56);
                bw.Write((uint) SimpleSkinVertexType.Color);
            }
            else
            {
                bw.Write((uint) 52);
                bw.Write((uint) SimpleSkinVertexType.Basic);
            }

            var box = GetBoundingBox();
            box.Write(bw);
            box.GetBoundingSphere().Write(bw);

            ushort indexOffset = 0;
            foreach (var submesh in Submeshes)
            {
                foreach (var index in submesh.Indices.Select(x => x += indexOffset)) bw.Write(index);

                indexOffset += submesh.Indices.Max();
            }

            foreach (var submesh in Submeshes)
            foreach (var vertex in submesh.Vertices)
                vertex.Write(bw, hasVertexColors ? SimpleSkinVertexType.Color : SimpleSkinVertexType.Basic);

            bw.Write(new byte[12]); //End tab
        }
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
}
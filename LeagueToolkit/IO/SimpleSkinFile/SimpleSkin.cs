using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;
using LeagueToolkit.IO.StaticObjectFile;
using LeagueToolkit.IO.WGT;

namespace LeagueToolkit.IO.SimpleSkinFile;

public class SimpleSkin
{
    public const int Magic = 0x00112233;

    public SimpleSkin(IList<SimpleSkinPrimitive> primitives, SimpleSkinVertexType vertexType)
    {
        Primitives = primitives;
        VertexType = vertexType;
    }

    public SimpleSkin(StaticObject staticObject, WGTFile weightFile)
    {
        // Not tested
        var indexOffset = 0;
        foreach (var subMesh in staticObject.Submeshes)
        {
            var vertices = new SimpleSkinVertex[subMesh.Vertices.Count];
            var indices = new ushort[subMesh.Indices.Count];

            for (var i = 0; i < subMesh.Vertices.Count; i++)
            {
                var vertex = subMesh.Vertices[i];
                var weightData = weightFile.Weights[i + indexOffset];
                vertices[i] = new SimpleSkinVertex(vertex.Position, weightData.BoneIndices, weightData.Weights,
                    Vector3.Zero, vertex.UV);
            }

            Primitives.Add(new SimpleSkinPrimitive(subMesh.Name, indices, vertices));

            indexOffset += subMesh.Vertices.Count;
        }
    }

    public SimpleSkin(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public SimpleSkin(Stream stream, bool leaveOpen = false)
    {
        using var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen);
        var magic = br.ReadUInt32();
        if (magic != Magic) throw new InvalidFileSignatureException();

        var major = br.ReadUInt16();
        var minor = br.ReadUInt16();
        switch (major)
        {
            case 1 or 2 or 4 when minor == 1:
                Read(br, major);
                break;
            case 0 when minor == 1:
                ReadLegacy(br);
                break;
            default:
                throw new UnsupportedFileVersionException();
        }
    }

    public IList<SimpleSkinPrimitive> Primitives { get; } = new List<SimpleSkinPrimitive>();
    public SimpleSkinVertexType VertexType { get; private set; } = SimpleSkinVertexType.Basic;

    private void Read(BinaryReader br, int major)
    {
        var primitiveCount = br.ReadUInt32();
        for (var i = 0; i < primitiveCount; i++) Primitives.Add(new SimpleSkinPrimitive(br));

        if (major == 4)
        {
            var flags = br.ReadUInt32();
        }

        var indexCount = br.ReadUInt32();
        var vertexCount = br.ReadUInt32();

        if (major == 4)
        {
            var vertexSize = br.ReadUInt32();
            VertexType = (SimpleSkinVertexType) br.ReadUInt32();
            if (!VertexType.IsDefinedFast())
                throw new InvalidDataException($"Vertex type {VertexType} is not supported");
            var boundingBox = new R3DBox(br);
            var boundingSphere = new R3DSphere(br);
        }

        var indices = new List<ushort>();
        var vertices = new List<SimpleSkinVertex>();
        for (var i = 0; i < indexCount; i++) indices.Add(br.ReadUInt16());
        for (var i = 0; i < vertexCount; i++) vertices.Add(new SimpleSkinVertex(br, VertexType));

        foreach (var primitive in Primitives)
        {
            primitive.Indices = indices
                .GetRange((int) primitive.StartIndex, (int) primitive.IndexCount)
                .Select(i => (ushort) (i - primitive.StartVertex))
                .ToList();
            primitive.Vertices = vertices.GetRange((int) primitive.StartVertex, (int) primitive.VertexCount);
        }
    }

    private void ReadLegacy(BinaryReader br)
    {
        var indexCount = br.ReadUInt32();
        var vertexCount = br.ReadUInt32();

        var indices = new List<ushort>();
        var vertices = new List<SimpleSkinVertex>();

        for (var i = 0; i < indexCount; i++) indices.Add(br.ReadUInt16());
        for (var i = 0; i < vertexCount; i++) vertices.Add(new SimpleSkinVertex(br, VertexType));

        Primitives.Add(new SimpleSkinPrimitive("Base", indices, vertices));
    }

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using var bw = new BinaryWriter(stream, Encoding.ASCII, leaveOpen);
        bw.Write(Magic);

        bw.Write((ushort) 4); // Major version
        bw.Write((ushort) 1); // Minor version

        bw.Write(Primitives.Count);

        uint vertexCount = 0;
        uint indexCount = 0;
        foreach (var primitive in Primitives)
        {
            primitive.Write(bw, vertexCount, indexCount);
            vertexCount += (uint) primitive.Vertices.Count;
            indexCount += (uint) primitive.Indices.Count;
        }

        bw.Write((uint) 0); // Flags

        bw.Write(indexCount);
        bw.Write(vertexCount);

        switch (VertexType)
        {
            case SimpleSkinVertexType.Basic:
                bw.Write((uint) (12 * sizeof(float) + 4 * sizeof(byte)));
                break;
            case SimpleSkinVertexType.Color:
                bw.Write((uint) (12 * sizeof(float) + 8 * sizeof(byte)));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        bw.Write((uint) VertexType);

        var box = GetBoundingBox();
        box.Write(bw);
        box.GetBoundingSphere().Write(bw);

        uint indexOffset = 0;
        foreach (var primitive in Primitives)
        {
            foreach (var index in primitive.Indices) bw.Write((ushort) (index + indexOffset));
            indexOffset += (uint) primitive.Vertices.Count;
        }

        foreach (var primitive in Primitives)
        foreach (var vertex in primitive.Vertices)
            vertex.Write(bw, VertexType);

        bw.Pad(16, true);
    }

    public R3DBox GetBoundingBox()
    {
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var primitive in Primitives)
        foreach (var vertex in primitive.Vertices)
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
namespace LeagueToolkit.IO.NVR;

public class NVRDrawIndexedPrimitive
{
    public int FirstIndex;
    public int FirstVertex;
    public int IndexBuffer;
    public int IndexCount;
    public NVRMesh Parent;

    //Used for writing only
    public int VertexBuffer;
    public int VertexCount;

    public NVRDrawIndexedPrimitive(BinaryReader br, NVRBuffers buffers, NVRMesh mesh, bool isComplex)
    {
        Parent = mesh;
        // Read vertices
        VertexBuffer = br.ReadInt32();
        FirstVertex = br.ReadInt32();
        VertexCount = br.ReadInt32();
        var currentOffset = br.BaseStream.Position;

        // Find vertex type
        var vertexSize = 12;
        if (isComplex)
        {
            VertexType = NVRVertex.GetVertexTypeFromMaterial(mesh.Material);
            switch (VertexType)
            {
                case NVRVertexType.NVRVERTEX_4:
                    vertexSize = NVRVertex4.Size;
                    break;
                case NVRVertexType.NVRVERTEX_8:
                    vertexSize = NVRVertex8.Size;
                    break;
                case NVRVertexType.NVRVERTEX_12:
                    vertexSize = NVRVertex12.Size;
                    break;
            }
        }

        //Parse vertices
        br.BaseStream.Seek(buffers.VertexBuffers[VertexBuffer].Offset + FirstVertex * vertexSize, SeekOrigin.Begin);
        for (var i = 0; i < VertexCount; i++)
        {
            NVRVertex newVertex;
            switch (VertexType)
            {
                case NVRVertexType.NVRVERTEX_4:
                    newVertex = new NVRVertex4(br);
                    break;
                case NVRVertexType.NVRVERTEX_8:
                    newVertex = new NVRVertex8(br);
                    break;
                case NVRVertexType.NVRVERTEX_12:
                    newVertex = new NVRVertex12(br);
                    break;
                default:
                    newVertex = new NVRVertex(br);
                    break;
            }

            Vertices.Add(newVertex);
        }

        // Store indices
        br.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
        IndexBuffer = br.ReadInt32();
        FirstIndex = br.ReadInt32();
        IndexCount = br.ReadInt32();

        for (var i = FirstIndex; i < FirstIndex + IndexCount; i++)
        {
            Indices.Add(buffers.IndexBuffers[IndexBuffer].Indices[i]);
        }

        // Fix indices
        var indicesMin = FindMin(Indices);
        for (var i = 0; i < Indices.Count; i++)
        {
            Indices[i] -= indicesMin;
        }
    }

    public NVRDrawIndexedPrimitive(NVRMesh mesh, List<NVRVertex> vertices, List<int> indices, bool complex)
    {
        Parent = mesh;
        Indices.AddRange(indices);
        if (complex)
        {
            Vertices.AddRange(vertices);
            if (vertices.Count > 0)
            {
                VertexType = vertices[0].GetVertexType();
                var expectedType = NVRVertex.GetVertexTypeFromMaterial(mesh.Material);
                if (expectedType != VertexType)
                {
                    throw new InvalidVertexTypeException(mesh.Material.Type, expectedType);
                }
            }
        }
        else
        {
            VertexType = NVRVertexType.NVRVERTEX;
            // Conversion to simple vertex
            foreach (var vertex in vertices)
            {
                Vertices.Add(new NVRVertex(vertex.Position));
            }
        }
    }

    public NVRVertexType VertexType { get; }
    public List<NVRVertex> Vertices { get; } = new();
    public List<int> Indices { get; } = new();

    private static int FindMin(List<int> list)
    {
        var min = list[0];
        for (var i = 1; i < list.Count; i++)
        {
            if (list[i] < min)
            {
                min = list[i];
            }
        }

        return min;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(VertexBuffer);
        bw.Write(FirstVertex);
        bw.Write(VertexCount);
        bw.Write(IndexBuffer);
        bw.Write(FirstIndex);
        bw.Write(IndexCount);
    }
}

public class InvalidVertexTypeException : Exception
{
    public InvalidVertexTypeException(NVRMaterialType matType, NVRVertexType expected) : base(
        string.Format("Invalid vertex type for the specified material ({0}), expected type: {1}.", matType, expected))
    {
    }
}
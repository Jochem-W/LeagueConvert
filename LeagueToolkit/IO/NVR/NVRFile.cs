using System.Collections.Generic;
using System.IO;
using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.NVR;

public class NVRFile
{
    public NVRFile(string fileLocation)
        : this(File.OpenRead(fileLocation))
    {
    }

    public NVRFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            //Reading magic and version
            var magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (magic != "NVR\0") throw new InvalidFileSignatureException();
            MajorVersion = br.ReadInt16();
            MinorVersion = br.ReadInt16();

            //Reading the counts
            var materialsCount = br.ReadInt32();
            var vertexBufferCount = br.ReadInt32();
            var indexBufferCount = br.ReadInt32();
            var meshesCount = br.ReadInt32();
            var nodesCount = br.ReadInt32();

            //Parse content
            var buffers = new NVRBuffers();
            for (var i = 0; i < materialsCount; i++)
                buffers.Materials.Add(new NVRMaterial(br, MajorVersion == 8 && MinorVersion == 1 ? true : false));
            for (var i = 0; i < vertexBufferCount; i++) buffers.VertexBuffers.Add(new NVRVertexBuffer(br));
            for (var i = 0; i < indexBufferCount; i++) buffers.IndexBuffers.Add(new NVRIndexBuffer(br));
            for (var i = 0; i < meshesCount; i++)
                buffers.Meshes.Add(new NVRMesh(br, buffers, MajorVersion == 8 && MinorVersion == 1 ? true : false));
            // Unused
            for (var i = 0; i < nodesCount; i++) buffers.Nodes.Add(new NVRNode(br, buffers));

            Meshes = buffers.Meshes;
        }
    }

    public short MajorVersion { get; }
    public short MinorVersion { get; }
    public List<NVRMesh> Meshes { get; }

    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            var buffers = GenerateBuffers();
            bw.Write(Encoding.ASCII.GetBytes("NVR\0"));
            bw.Write(MajorVersion);
            bw.Write(MinorVersion);
            bw.Write(buffers.Materials.Count);
            bw.Write(buffers.VertexBuffers.Count);
            bw.Write(buffers.IndexBuffers.Count);
            bw.Write(buffers.Meshes.Count);
            bw.Write(buffers.Nodes.Count);
            foreach (var material in buffers.Materials) material.Write(bw);
            foreach (var vertBuffer in buffers.VertexBuffers) vertBuffer.Write(bw);
            foreach (var indBuffer in buffers.IndexBuffers) indBuffer.Write(bw);
            foreach (var mesh in buffers.Meshes) mesh.Write(bw);
            foreach (var node in buffers.Nodes) node.Write(bw);
        }
    }

    public NVRMesh AddMesh(NVRMeshQuality meshQualityLevel, NVRMaterial material, List<NVRVertex> vertices,
        List<int> indices)
    {
        var newMesh = new NVRMesh(meshQualityLevel, 0, material, vertices, indices);
        Meshes.Add(newMesh);
        return newMesh;
    }

    // Generate buffers for writing
    private NVRBuffers GenerateBuffers()
    {
        var buffers = new NVRBuffers();
        // Material buffer
        foreach (var mesh in Meshes)
            if (!buffers.Materials.Contains(mesh.Material))
                buffers.Materials.Add(mesh.Material);

        // Creating complex buffers first
        foreach (var mesh in Meshes)
        {
            var complexMesh = mesh.IndexedPrimitives[0];
            var type = complexMesh.Vertices[0].GetVertexType();
            var vertBuffer = buffers.GetVertexBuffer(complexMesh.Vertices.Count, type);
            var bufferIndex = buffers.VertexBuffers.IndexOf(vertBuffer);
            var indBuffer = buffers.GetIndexBuffer(bufferIndex);

            complexMesh.IndexBuffer = bufferIndex;
            complexMesh.VertexBuffer = bufferIndex;
            complexMesh.FirstVertex = vertBuffer.Vertices.Count;
            complexMesh.FirstIndex = indBuffer.Indices.Count;
            complexMesh.IndexCount = complexMesh.Indices.Count;
            complexMesh.VertexCount = complexMesh.Vertices.Count;

            vertBuffer.Vertices.AddRange(complexMesh.Vertices);
            var indBufferMax = indBuffer.CurrentMax + 1;
            foreach (var index in complexMesh.Indices) indBuffer.AddIndex(index + indBufferMax);
        }

        // Then do simple ones
        foreach (var mesh in Meshes)
        {
            var simpleMesh = mesh.IndexedPrimitives[1];
            var type = simpleMesh.Vertices[0].GetVertexType();
            var vertBuffer = buffers.GetVertexBuffer(simpleMesh.Vertices.Count, type);
            var bufferIndex = buffers.VertexBuffers.IndexOf(vertBuffer);
            var indBuffer = buffers.GetIndexBuffer(bufferIndex);

            simpleMesh.IndexBuffer = bufferIndex;
            simpleMesh.VertexBuffer = bufferIndex;
            simpleMesh.FirstVertex = vertBuffer.Vertices.Count;
            simpleMesh.FirstIndex = indBuffer.Indices.Count;
            simpleMesh.IndexCount = simpleMesh.Indices.Count;
            simpleMesh.VertexCount = simpleMesh.Vertices.Count;

            vertBuffer.Vertices.AddRange(simpleMesh.Vertices);
            var indBufferMax = indBuffer.CurrentMax + 1;
            foreach (var index in simpleMesh.Indices) indBuffer.AddIndex(index + indBufferMax);
        }

        var parentNode = CreateRootNode();
        // Making mesh buffer
        buffers.GenerateMeshBuffer(parentNode);
        foreach (var mesh in buffers.Meshes) mesh.MaterialIndex = buffers.Materials.IndexOf(mesh.Material);

        // Making node buffer
        buffers.Nodes.Add(parentNode);
        buffers.GenerateNodeBuffer(parentNode);
        foreach (var node in buffers.Nodes)
            if (node.Children.Count > 0)
                node.FirstChildNode = buffers.Nodes.IndexOf(node.Children[0]);
            else
                node.FirstChildNode = -1;
        return buffers;
    }

    private NVRNode CreateRootNode()
    {
        // Calculate the bounding box of the entire map + bounding box of the central points of the meshes (used to split nodes).
        var rootNode = new NVRNode(Meshes);
        // Create children for root node and all of its children and children and children
        if (Meshes.Count > 1)
            rootNode.Split();
        return rootNode;
    }
}

public class NVRBuffers
{
    public List<NVRMaterial> Materials { get; } = new();
    public List<NVRVertexBuffer> VertexBuffers { get; } = new();
    public List<NVRIndexBuffer> IndexBuffers { get; } = new();
    public List<NVRMesh> Meshes { get; } = new();
    public List<NVRNode> Nodes { get; } = new();

    // Find index buffer with its position (for a given model, it has to be the same as its vertex buffer position)
    public NVRIndexBuffer GetIndexBuffer(int position)
    {
        if (IndexBuffers.Count > position)
        {
            return IndexBuffers[position];
        }

        var newBuffer = new NVRIndexBuffer(D3DFORMAT.D3DFMT_INDEX16);
        IndexBuffers.Add(newBuffer);
        return newBuffer;
    }

    // Find apropriate vertex buffer and create it if doesn't exist
    public NVRVertexBuffer GetVertexBuffer(int vertexToAddCount, NVRVertexType type)
    {
        foreach (var buffer in VertexBuffers)
            if (buffer.Type == type && buffer.Vertices.Count < ushort.MaxValue - vertexToAddCount)
                return buffer;
        var created = new NVRVertexBuffer(type);
        VertexBuffers.Add(created);
        return created;
    }

    // Generate a mesh buffer
    public void GenerateMeshBuffer(NVRNode node)
    {
        node.FirstMesh = Meshes.Count;
        node.MeshCount = node.Meshes.Count;
        if (node.Children.Count == 0)
            Meshes.AddRange(node.Meshes);
        else
            foreach (var child in node.Children)
                GenerateMeshBuffer(child);
    }

    // Generate a node buffer
    public void GenerateNodeBuffer(NVRNode node)
    {
        node.ChildNodeCount = node.Children.Count;
        Nodes.InsertRange(0, node.Children);
        for (var i = node.Children.Count - 1; i >= 0; i--) GenerateNodeBuffer(node.Children[i]);
    }
}

public enum D3DFORMAT
{
    D3DFMT_UNKNOWN = 0x0,
    D3DFMT_R8G8B8 = 0x14,
    D3DFMT_A8R8G8B8 = 0x15,
    D3DFMT_X8R8G8B8 = 0x16,
    D3DFMT_R5G6B5 = 0x17,
    D3DFMT_X1R5G5B5 = 0x18,
    D3DFMT_A1R5G5B5 = 0x19,
    D3DFMT_A4R4G4B4 = 0x1A,
    D3DFMT_R3G3B2 = 0x1B,
    D3DFMT_A8 = 0x1C,
    D3DFMT_A8R3G3B2 = 0x1D,
    D3DFMT_X4R4G4B4 = 0x1E,
    D3DFMT_A2B10G10R10 = 0x1F,
    D3DFMT_A8B8G8R8 = 0x20,
    D3DFMT_X8B8G8R8 = 0x21,
    D3DFMT_G16R16 = 0x22,
    D3DFMT_A2R10G10B10 = 0x23,
    D3DFMT_A16B16G16R16 = 0x24,
    D3DFMT_A8P8 = 0x28,
    D3DFMT_P8 = 0x29,
    D3DFMT_L8 = 0x32,
    D3DFMT_A8L8 = 0x33,
    D3DFMT_A4L4 = 0x34,
    D3DFMT_V8U8 = 0x3C,
    D3DFMT_L6V5U5 = 0x3D,
    D3DFMT_X8L8V8U8 = 0x3E,
    D3DFMT_Q8W8V8U8 = 0x3F,
    D3DFMT_V16U16 = 0x40,
    D3DFMT_A2W10V10U10 = 0x43,
    D3DFMT_UYVY = 0x59565955,
    D3DFMT_R8G8_B8G8 = 0x47424752,
    D3DFMT_YUY2 = 0x32595559,
    D3DFMT_G8R8_G8B8 = 0x42475247,
    D3DFMT_DXT1 = 0x31545844,
    D3DFMT_DXT2 = 0x32545844,
    D3DFMT_DXT3 = 0x33545844,
    D3DFMT_DXT4 = 0x34545844,
    D3DFMT_DXT5 = 0x35545844,
    D3DFMT_D16_LOCKABLE = 0x46,
    D3DFMT_D32 = 0x47,
    D3DFMT_D15S1 = 0x49,
    D3DFMT_D24S8 = 0x4B,
    D3DFMT_D24X8 = 0x4D,
    D3DFMT_D24X4S4 = 0x4F,
    D3DFMT_D16 = 0x50,
    D3DFMT_D32F_LOCKABLE = 0x52,
    D3DFMT_D24FS8 = 0x53,
    D3DFMT_D32_LOCKABLE = 0x54,
    D3DFMT_S8_LOCKABLE = 0x55,
    D3DFMT_L16 = 0x51,
    D3DFMT_VERTEXDATA = 0x64,
    D3DFMT_INDEX16 = 0x65,
    D3DFMT_INDEX32 = 0x66,
    D3DFMT_Q16W16V16U16 = 0x6E,
    D3DFMT_MULTI2_ARGB8 = 0x3154454D,
    D3DFMT_R16F = 0x6F,
    D3DFMT_G16R16F = 0x70,
    D3DFMT_A16B16G16R16F = 0x71,
    D3DFMT_R32F = 0x72,
    D3DFMT_G32R32F = 0x73,
    D3DFMT_A32B32G32R32F = 0x74,
    D3DFMT_CxV8U8 = 0x75,
    D3DFMT_A1 = 0x76,
    D3DFMT_A2B10G10R10_XR_BIAS = 0x77,
    D3DFMT_BINARYBUFFER = 0xC7,
    D3DFMT_FORCE_DWORD = 0x7FFFFFFF
}
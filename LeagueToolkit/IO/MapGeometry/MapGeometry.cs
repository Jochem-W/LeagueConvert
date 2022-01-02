using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Structures.BucketGrid;

namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometry
{
    public MapGeometry(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public MapGeometry(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (magic != "OEGM") throw new InvalidFileSignatureException();

            var version = br.ReadUInt32();
            if (version != 5 && version != 6 && version != 7 && version != 9 && version != 11)
                throw new UnsupportedFileVersionException();

            var useSeparatePointLights = false;
            if (version < 7) useSeparatePointLights = br.ReadBoolean();

            if (version >= 9)
            {
                UnknownString1 = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));

                if (version >= 11) UnknownString2 = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            }

            List<MapGeometryVertexElementGroup> vertexElementGroups = new();
            var vertexElementGroupCount = br.ReadUInt32();
            for (var i = 0; i < vertexElementGroupCount; i++)
                vertexElementGroups.Add(new MapGeometryVertexElementGroup(br));

            var vertexBufferCount = br.ReadUInt32();
            List<long> vertexBufferOffsets = new();
            for (var i = 0; i < vertexBufferCount; i++)
            {
                var bufferSize = br.ReadUInt32();

                vertexBufferOffsets.Add(br.BaseStream.Position);
                br.BaseStream.Seek(bufferSize, SeekOrigin.Current);
            }

            var indexBufferCount = br.ReadUInt32();
            List<ushort[]> indexBuffers = new();
            for (var i = 0; i < indexBufferCount; i++)
            {
                var bufferSize = br.ReadUInt32();
                var indexBuffer = new ushort[bufferSize / 2];

                for (var j = 0; j < bufferSize / 2; j++) indexBuffer[j] = br.ReadUInt16();

                indexBuffers.Add(indexBuffer);
            }

            var modelCount = br.ReadUInt32();
            for (var i = 0; i < modelCount; i++)
                Models.Add(new MapGeometryModel(br, vertexElementGroups, vertexBufferOffsets, indexBuffers,
                    useSeparatePointLights, version));

            BucketGrid = new BucketGrid(br);
        }
    }

    public string UnknownString1 { get; set; } = string.Empty;
    public string UnknownString2 { get; set; } = string.Empty;
    public List<MapGeometryModel> Models { get; set; } = new();
    public BucketGrid BucketGrid { get; set; }

    public void Write(string fileLocation, uint version)
    {
        Write(File.Create(fileLocation), version);
    }

    public void Write(Stream stream, uint version, bool leaveOpen = false)
    {
        if (version != 5 && version != 6 && version != 7 && version != 9 && version != 11)
            throw new Exception("Unsupported version");

        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("OEGM"));
            bw.Write(version);

            var usesSeparatePointLights = false;
            if (version < 7)
            {
                usesSeparatePointLights = UsesSeparatePointLights();
                bw.Write(usesSeparatePointLights);
            }

            if (version >= 9)
            {
                bw.Write(UnknownString1.Length);
                bw.Write(Encoding.ASCII.GetBytes(UnknownString1));

                if (version >= 11)
                {
                    bw.Write(UnknownString2.Length);
                    bw.Write(Encoding.ASCII.GetBytes(UnknownString2));
                }
            }

            var vertexElementGroups = GenerateVertexElementGroups();
            bw.Write(vertexElementGroups.Count);
            foreach (var vertexElementGroup in vertexElementGroups) vertexElementGroup.Write(bw);

            var vertexBuffers = GenerateVertexBuffers(vertexElementGroups);
            bw.Write(vertexBuffers.Count);
            foreach (var vertexBuffer in vertexBuffers)
            {
                bw.Write(vertexBuffer.Length);
                bw.Write(vertexBuffer);
            }

            var indexBuffers = GenerateIndexBuffers();
            bw.Write(indexBuffers.Count);
            foreach (var indexBuffer in indexBuffers)
            {
                bw.Write(indexBuffer.Length * 2);

                for (var i = 0; i < indexBuffer.Length; i++) bw.Write(indexBuffer[i]);
            }

            bw.Write(Models.Count);
            foreach (var model in Models) model.Write(bw, usesSeparatePointLights, version);

            BucketGrid.Write(bw);
        }
    }

    public void AddModel(MapGeometryModel model)
    {
        Models.Add(model);
    }

    private bool UsesSeparatePointLights()
    {
        foreach (var model in Models)
            if (model.SeparatePointLight != null)
                return true;

        return false;
    }

    private List<MapGeometryVertexElementGroup> GenerateVertexElementGroups()
    {
        var vertexElementGroups = new List<MapGeometryVertexElementGroup>();

        foreach (var model in Models)
        {
            var vertexElementGroup = new MapGeometryVertexElementGroup(model.Vertices[0]);

            if (!vertexElementGroups.Contains(vertexElementGroup)) vertexElementGroups.Add(vertexElementGroup);

            model._vertexElementGroupID = vertexElementGroups.IndexOf(vertexElementGroup);
        }

        return vertexElementGroups;
    }

    private List<byte[]> GenerateVertexBuffers(List<MapGeometryVertexElementGroup> vertexElementGroups)
    {
        var vertexBuffers = new List<byte[]>();
        var vertexBufferID = 0;

        foreach (var model in Models)
        {
            var vertexSize = vertexElementGroups[model._vertexElementGroupID].GetVertexSize();
            var vertexBuffer = new byte[vertexSize * model.Vertices.Count];

            for (var i = 0; i < model.Vertices.Count; i++)
            {
                var vertexElementsBuffer = model.Vertices[i].ToArray(vertexSize);
                Buffer.BlockCopy(vertexElementsBuffer, 0, vertexBuffer, i * vertexSize, vertexElementsBuffer.Length);
            }

            vertexBuffers.Add(vertexBuffer);
            model._vertexBufferID = vertexBufferID;
            vertexBufferID++;
        }

        return vertexBuffers;
    }

    private List<ushort[]> GenerateIndexBuffers()
    {
        var indexBuffers = new List<ushort[]>();
        var indexBufferID = 0;

        foreach (var model in Models)
        {
            indexBuffers.Add(model.Indices.ToArray());

            model._indexBufferID = indexBufferID;
            indexBufferID++;
        }

        return indexBuffers;
    }
}
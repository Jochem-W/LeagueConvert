using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Extensions;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.MapGeometry;

public class MapGeometryModel
{
    internal int _indexBufferID;
    internal int _vertexBufferID;

    internal int _vertexElementGroupID;

    public MapGeometryModel()
    {
    }

    public MapGeometryModel(string name, List<MapGeometryVertex> vertices, List<ushort> indices,
        List<MapGeometrySubmesh> submeshes)
    {
        Name = name;
        Vertices = vertices;
        Indices = indices;
        Submeshes = submeshes;

        foreach (var submesh in submeshes) submesh.Parent = this;

        BoundingBox = GetBoundingBox();
    }

    public MapGeometryModel(string name, List<MapGeometryVertex> vertices, List<ushort> indices,
        List<MapGeometrySubmesh> submeshes, MapGeometryLayer layer)
        : this(name, vertices, indices, submeshes)
    {
        Layer = layer;
    }

    public MapGeometryModel(string name, List<MapGeometryVertex> vertices, List<ushort> indices,
        List<MapGeometrySubmesh> submeshes, R3DMatrix44 transformation)
        : this(name, vertices, indices, submeshes)
    {
        Transformation = transformation;
    }

    public MapGeometryModel(string name, List<MapGeometryVertex> vertices, List<ushort> indices,
        List<MapGeometrySubmesh> submeshes, MapGeometryLayer layer, R3DMatrix44 transformation)
        : this(name, vertices, indices, submeshes)
    {
        Layer = layer;
        Transformation = transformation;
    }

    public MapGeometryModel(string name, List<MapGeometryVertex> vertices, List<ushort> indices,
        List<MapGeometrySubmesh> submeshes, string lightmap, Color color)
        : this(name, vertices, indices, submeshes)
    {
        Lightmap = lightmap;
        Color = color;
    }

    public MapGeometryModel(BinaryReader br, List<MapGeometryVertexElementGroup> vertexElementGroups,
        List<long> vertexBufferOffsets, List<ushort[]> indexBuffers, bool useSeparatePointLights, uint version)
    {
        Name = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
        var vertexCount = br.ReadUInt32();
        var vertexBufferCount = br.ReadUInt32();
        var vertexElementGroup = br.ReadInt32();

        for (var i = 0; i < vertexCount; i++) Vertices.Add(new MapGeometryVertex());

        for (int i = 0, currentVertexElementGroup = vertexElementGroup;
             i < vertexBufferCount;
             i++, currentVertexElementGroup++)
        {
            var vertexBufferID = br.ReadInt32();
            var returnPosition = br.BaseStream.Position;
            br.BaseStream.Seek(vertexBufferOffsets[vertexBufferID], SeekOrigin.Begin);

            for (var j = 0; j < vertexCount; j++)
                Vertices[j] = MapGeometryVertex.Combine(Vertices[j],
                    new MapGeometryVertex(br, vertexElementGroups[currentVertexElementGroup].VertexElements));

            br.BaseStream.Seek(returnPosition, SeekOrigin.Begin);
        }

        var indexCount = br.ReadUInt32();
        var indexBuffer = br.ReadInt32();
        Indices.AddRange(indexBuffers[indexBuffer]);

        var submeshCount = br.ReadUInt32();
        for (var i = 0; i < submeshCount; i++) Submeshes.Add(new MapGeometrySubmesh(br, this));

        if (version != 5) FlipNormals = br.ReadBoolean();

        BoundingBox = new R3DBox(br);
        Transformation = new R3DMatrix44(br);
        Flags = (MapGeometryModelFlags) br.ReadByte();

        if (version >= 7)
        {
            Layer = (MapGeometryLayer) br.ReadByte();

            if (version >= 11) UnknownByte = br.ReadByte();
        }

        if (useSeparatePointLights && version < 7) SeparatePointLight = br.ReadVector3();

        if (version < 9)
        {
            for (var i = 0; i < 9; i++) UnknownFloats.Add(br.ReadVector3());

            Lightmap = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            Color = br.ReadColor(ColorFormat.RgbaF32);
        }
        else if (version >= 9)
        {
            Lightmap = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            Color = br.ReadColor(ColorFormat.RgbaF32);

            BakedPaintTexture = Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            BakedPaintColor = br.ReadColor(ColorFormat.RgbaF32);
        }
    }

    public string Name { get; set; }
    public List<MapGeometryVertex> Vertices { get; set; } = new();
    public List<ushort> Indices { get; set; } = new();
    public List<MapGeometrySubmesh> Submeshes { get; set; } = new();
    public bool FlipNormals { get; set; }
    public R3DBox BoundingBox { get; set; }
    public R3DMatrix44 Transformation { get; set; } = R3DMatrix44.IdentityR3DMatrix44();
    public MapGeometryModelFlags Flags { get; set; } = MapGeometryModelFlags.GenericObject;
    public MapGeometryLayer Layer { get; set; } = MapGeometryLayer.AllLayers;
    public byte UnknownByte { get; set; }
    public Vector3? SeparatePointLight { get; set; }
    public List<Vector3> UnknownFloats { get; set; } = new();
    public string Lightmap { get; set; } = string.Empty;
    public string BakedPaintTexture { get; set; } = string.Empty;
    public Color Color { get; set; } = new(0, 0, 0, 1);
    public Color BakedPaintColor { get; set; } = new(0, 0, 0, 1);

    public void Write(BinaryWriter bw, bool useSeparatePointLights, uint version)
    {
        bw.Write(Name.Length);
        bw.Write(Encoding.ASCII.GetBytes(Name));

        bw.Write(Vertices.Count);
        bw.Write((uint) 1);
        bw.Write(_vertexElementGroupID);
        bw.Write(_vertexBufferID); //we only have one vertex buffer

        bw.Write(Indices.Count);
        bw.Write(_indexBufferID);

        bw.Write(Submeshes.Count);
        foreach (var submesh in Submeshes) submesh.Write(bw);

        if (version != 5) bw.Write(FlipNormals);

        BoundingBox.Write(bw);
        Transformation.Write(bw);
        bw.Write((byte) Flags);

        if (version >= 7)
        {
            bw.Write((byte) Layer);

            if (version >= 11) bw.Write(UnknownByte);
        }

        if (version < 9)
        {
            if (useSeparatePointLights)
            {
                if (SeparatePointLight is Vector3 separatePointLight)
                    bw.WriteVector3(separatePointLight);
                else
                    bw.WriteVector3(Vector3.Zero);
            }

            foreach (var pointLight in UnknownFloats) bw.WriteVector3(pointLight);
            for (var i = 0; i < 9 - UnknownFloats.Count; i++) bw.WriteVector3(Vector3.Zero);

            bw.Write(Lightmap.Length);
            bw.Write(Encoding.ASCII.GetBytes(Lightmap));
            bw.WriteColor(Color, ColorFormat.RgbaF32);
        }
        else if (version >= 9)
        {
            bw.Write(Lightmap.Length);
            if (Lightmap.Length != 0) bw.Write(Encoding.ASCII.GetBytes(Lightmap));
            bw.WriteColor(Color, ColorFormat.RgbaF32);

            bw.Write(BakedPaintTexture.Length);
            if (BakedPaintTexture.Length != 0) bw.Write(Encoding.ASCII.GetBytes(BakedPaintTexture));
            bw.WriteColor(BakedPaintColor, ColorFormat.RgbaF32);
        }
    }

    public void AssignLightmap(string lightmap, Color color)
    {
        Lightmap = lightmap;
        Color = color;
    }

    public R3DBox GetBoundingBox()
    {
        if (Vertices == null || Vertices.Count == 0) return new R3DBox(new Vector3(0, 0, 0), new Vector3(0, 0, 0));

        var min = Vertices[0].Position.Value;
        var max = Vertices[0].Position.Value;

        foreach (var vertex in Vertices)
        {
            if (min.X > vertex.Position.Value.X) min.X = vertex.Position.Value.X;
            if (min.Y > vertex.Position.Value.Y) min.Y = vertex.Position.Value.Y;
            if (min.Z > vertex.Position.Value.Z) min.Z = vertex.Position.Value.Z;
            if (max.X < vertex.Position.Value.X) max.X = vertex.Position.Value.X;
            if (max.Y < vertex.Position.Value.Y) max.Y = vertex.Position.Value.Y;
            if (max.Z < vertex.Position.Value.Z) max.Z = vertex.Position.Value.Z;
        }

        return new R3DBox(min, max);
    }
}

[Flags]
public enum MapGeometryLayer : byte
{
    NoLayer = 0,
    Layer1 = 1,
    Layer2 = 2,
    Layer3 = 4,
    Layer4 = 8,
    Layer5 = 16,
    Layer6 = 32,
    Layer7 = 64,
    Layer8 = 128,
    AllLayers = 255
}

[Flags]
public enum MapGeometryModelFlags : byte
{
    UnknownTransparency = 1,
    UnknownLightning = 2,
    Unknown3 = 4,
    Unknown4 = 8,
    UnknownConst1 = 16,

    GenericObject = UnknownTransparency | UnknownLightning | Unknown3 | Unknown4 | UnknownConst1
}
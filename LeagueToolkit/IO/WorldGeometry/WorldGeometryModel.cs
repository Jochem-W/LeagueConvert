using System.Numerics;
using System.Text;
using LeagueToolkit.Helpers.Structures;

namespace LeagueToolkit.IO.WorldGeometry;

/// <summary>
///     Represents a <see cref="WorldGeometryModel" /> inside of a <see cref="WorldGeometry" />
/// </summary>
public class WorldGeometryModel
{
    /// <summary>
    ///     Initializes an empty <see cref="WorldGeometryModel" />
    /// </summary>
    public WorldGeometryModel()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldGeometryModel" />
    /// </summary>
    /// <param name="texture">Texture Path of this <see cref="WorldGeometryModel" /></param>
    /// <param name="material">Material of this <see cref="WorldGeometryModel" /></param>
    /// <param name="vertices">Vertices of this <see cref="WorldGeometryModel" /></param>
    /// <param name="indices">Indices of this <see cref="WorldGeometryModel" /></param>
    public WorldGeometryModel(string texture, string material, List<WorldGeometryVertex> vertices, List<uint> indices)
    {
        Texture = texture;
        Material = material;
        Vertices = vertices;
        Indices = indices;
        BoundingBox = CalculateBoundingBox();
        Sphere = CalculateSphere();
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldGeometryModel" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public WorldGeometryModel(BinaryReader br)
    {
        Texture = Encoding.ASCII.GetString(br.ReadBytes(260)).Replace("\0", "");
        Material = Encoding.ASCII.GetString(br.ReadBytes(64));
        Material = Material.Remove(Material.IndexOf("\0", StringComparison.Ordinal));
        Sphere = new R3DSphere(br);
        BoundingBox = new R3DBox(br);

        var vertexCount = br.ReadUInt32();
        var indexCount = br.ReadUInt32();
        for (var i = 0; i < vertexCount; i++)
        {
            Vertices.Add(new WorldGeometryVertex(br));
        }

        if (indexCount <= 65536)
        {
            for (var i = 0; i < indexCount; i++)
            {
                Indices.Add(br.ReadUInt16());
            }
        }
        else
        {
            for (var i = 0; i < indexCount; i++)
            {
                Indices.Add(br.ReadUInt32());
            }
        }
    }

    /// <summary>
    ///     Texture Path of this <see cref="WorldGeometryModel" />
    /// </summary>
    public string Texture { get; set; }

    /// <summary>
    ///     Material of this <see cref="WorldGeometryModel" />
    /// </summary>
    public string Material { get; set; }

    /// <summary>
    ///     Bounding Sphere of this <see cref="WorldGeometryModel" />
    /// </summary>
    public R3DSphere Sphere { get; }

    /// <summary>
    ///     Axis Aligned Bounding Box of this <see cref="WorldGeometryModel" />
    /// </summary>
    public R3DBox BoundingBox { get; }

    /// <summary>
    ///     Vertices of this <see cref="WorldGeometryModel" />
    /// </summary>
    public List<WorldGeometryVertex> Vertices { get; set; } = new();

    /// <summary>
    ///     Indices of this <see cref="WorldGeometryModel" />
    /// </summary>
    public List<uint> Indices { get; set; } = new();

    /// <summary>
    ///     Writes this <see cref="WorldGeometryModel" /> into the specified <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        bw.Write(Encoding.ASCII.GetBytes(Texture.PadRight(260, '\u0000')));
        bw.Write(Encoding.ASCII.GetBytes(Material.PadRight(64, '\u0000')));

        var boundingGeometry = CalculateBoundingGeometry();
        boundingGeometry.Item1.Write(bw);
        boundingGeometry.Item2.Write(bw);

        bw.Write(Vertices.Count);
        bw.Write(Indices.Count);
        foreach (var vertex in Vertices)
        {
            vertex.Write(bw);
        }

        if (Indices.Count <= 65536)
        {
            foreach (ushort index in Indices)
            {
                bw.Write(index);
            }
        }
        else
        {
            foreach (var index in Indices)
            {
                bw.Write(index);
            }
        }
    }

    public Tuple<R3DSphere, R3DBox> CalculateBoundingGeometry()
    {
        var box = CalculateBoundingBox();
        var sphere = CalculateSphere(box);
        return new Tuple<R3DSphere, R3DBox>(sphere, box);
    }

    /// <summary>
    ///     Calculates the Axis Aligned Bounding Box of this <see cref="WorldGeometryModel" />
    /// </summary>
    public R3DBox CalculateBoundingBox()
    {
        if (Vertices == null || Vertices.Count == 0)
        {
            return new R3DBox(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }

        var min = Vertices[0].Position;
        var max = Vertices[0].Position;

        foreach (var vertex in Vertices)
        {
            if (min.X > vertex.Position.X)
            {
                min.X = vertex.Position.X;
            }

            if (min.Y > vertex.Position.Y)
            {
                min.Y = vertex.Position.Y;
            }

            if (min.Z > vertex.Position.Z)
            {
                min.Z = vertex.Position.Z;
            }

            if (max.X < vertex.Position.X)
            {
                max.X = vertex.Position.X;
            }

            if (max.Y < vertex.Position.Y)
            {
                max.Y = vertex.Position.Y;
            }

            if (max.Z < vertex.Position.Z)
            {
                max.Z = vertex.Position.Z;
            }
        }

        return new R3DBox(min, max);
    }

    /// <summary>
    ///     Calculates the Bounding Sphere of this <see cref="WorldGeometryModel" />
    /// </summary>
    public R3DSphere CalculateSphere()
    {
        var box = CalculateBoundingBox();
        var centralPoint = new Vector3(0.5f * (BoundingBox.Max.X + BoundingBox.Min.X),
            0.5f * (BoundingBox.Max.Y + BoundingBox.Min.Y),
            0.5f * (BoundingBox.Max.Z + BoundingBox.Min.Z));

        return new R3DSphere(centralPoint, Vector3.Distance(centralPoint, box.Max));
    }

    /// <summary>
    ///     Calculates the Bounding Sphere of this <see cref="WorldGeometryModel" /> from the specified <see cref="R3DBox" />
    /// </summary>
    /// <param name="box"><see cref="R3DBox" /> to use for calculation</param>
    public R3DSphere CalculateSphere(R3DBox box)
    {
        var centralPoint = new Vector3(0.5f * (BoundingBox.Max.X + BoundingBox.Min.X),
            0.5f * (BoundingBox.Max.Y + BoundingBox.Min.Y),
            0.5f * (BoundingBox.Max.Z + BoundingBox.Min.Z));

        return new R3DSphere(centralPoint, Vector3.Distance(centralPoint, box.Max));
    }
}
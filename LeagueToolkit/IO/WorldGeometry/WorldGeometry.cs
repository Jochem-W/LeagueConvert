using System.Text;
using LeagueToolkit.Helpers.Exceptions;
using LeagueToolkit.Helpers.Structures.BucketGrid;

namespace LeagueToolkit.IO.WorldGeometry;

/// <summary>
///     Represents a World Geometry (WGEO) file
/// </summary>
public class WorldGeometry
{
    /// <summary>
    ///     Initializes a new empty <see cref="WorldGeometry" />
    /// </summary>
    public WorldGeometry()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldGeometry" />
    /// </summary>
    /// <param name="models">Models of this <see cref="WorldGeometry" /></param>
    /// <param name="bucketGrid"><see cref="BucketGrid" /> of this <see cref="WorldGeometry" /></param>
    public WorldGeometry(List<WorldGeometryModel> models, BucketGrid bucketGrid)
    {
        Models = models;
        BucketGrid = bucketGrid;
    }

    /// <summary>
    ///     Initalizes a new <see cref="WorldGeometry" /> from the specified location
    /// </summary>
    /// <param name="fileLocation">Location to read from</param>
    public WorldGeometry(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WorldGeometry" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public WorldGeometry(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (magic != "WGEO") throw new InvalidFileSignatureException();

            var version = br.ReadUInt32();
            if (version != 5 && version != 4) throw new UnsupportedFileVersionException();

            var modelCount = br.ReadUInt32();
            var faceCount = br.ReadUInt32();

            for (var i = 0; i < modelCount; i++) Models.Add(new WorldGeometryModel(br));

            if (version == 5) BucketGrid = new BucketGrid(br);
        }
    }

    /// <summary>
    ///     Models of this <see cref="WorldGeometry" />
    /// </summary>
    public List<WorldGeometryModel> Models { get; set; } = new();

    /// <summary>
    ///     <see cref="WGEOBucketGeometry" /> of this <see cref="WorldGeometry" />
    /// </summary>
    public BucketGrid BucketGrid { get; set; }

    /// <summary>
    ///     Writes this <see cref="WorldGeometry" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">Location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="WorldGeometry" /> into the specified stream
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            uint faceCount = 0;
            bw.Write(Encoding.ASCII.GetBytes("WGEO"));
            bw.Write(BucketGrid == null ? 4 : 5);
            bw.Write(Models.Count);
            foreach (var model in Models) faceCount += (uint) model.Indices.Count / 3;
            bw.Write(faceCount);

            foreach (var model in Models) model.Write(bw);

            BucketGrid?.Write(bw);
        }
    }
}
using System.Text;
using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.WGT;

/// <summary>
///     Represents a Weight File which is used in old League of Legends versions together with SCO
/// </summary>
public class WGTFile
{
    /// <summary>
    ///     Initializes a blank <see cref="WGTFile" />
    /// </summary>
    public WGTFile()
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WGTFile" />
    /// </summary>
    /// <param name="weights">Weights of this <see cref="WGTFile" /></param>
    public WGTFile(List<WGTWeight> weights)
    {
        Weights = weights;
    }

    /// <summary>
    ///     Initializes a new <see cref="WGTFile" />
    /// </summary>
    /// <param name="weights">Weight data of this <see cref="WGTFile" /></param>
    /// <param name="boneIndices">Bone Index data of this <see cref="WGTFile" /></param>
    public WGTFile(List<float[]> weights, List<byte[]> boneIndices)
    {
        if (weights.Count != boneIndices.Count)
        {
            throw new Exception("Weights and Bone Indices have to be synchronized");
        }

        for (var i = 0; i < weights.Count; i++)
        {
            Weights.Add(new WGTWeight(weights[i], boneIndices[i]));
        }
    }

    /// <summary>
    ///     Initializes a new <see cref="WGTFile" /> from the spcified location
    /// </summary>
    /// <param name="fileLocation">Location to read from</param>
    public WGTFile(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    /// <summary>
    ///     Initializes a new <see cref="WGTFile" /> from a <see cref="Stream" />
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to read from</param>
    public WGTFile(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var magic = Encoding.ASCII.GetString(br.ReadBytes(8));
            if (magic != "r3d2wght")
            {
                throw new InvalidFileSignatureException();
            }

            var version = br.ReadUInt32();
            if (version != 1)
            {
                throw new UnsupportedFileVersionException();
            }

            var skeletonId = br.ReadUInt32();
            var weightCount = br.ReadUInt32();

            for (var i = 0; i < weightCount; i++)
            {
                Weights.Add(new WGTWeight(br));
            }
        }
    }

    /// <summary>
    ///     Weights of this <see cref="WGTFile" />
    /// </summary>
    public List<WGTWeight> Weights { get; } = new();

    /// <summary>
    ///     Writes this <see cref="WGTFile" /> to the specified location
    /// </summary>
    /// <param name="fileLocation">Location to write to</param>
    public void Write(string fileLocation)
    {
        Write(File.Create(fileLocation));
    }

    /// <summary>
    ///     Writes this <see cref="WGTFile" /> into a <see cref="Stream" />
    /// </summary>
    /// <param name="stream"><see cref="Stream" /> to write to</param>
    public void Write(Stream stream, bool leaveOpen = false)
    {
        using (var bw = new BinaryWriter(stream, Encoding.UTF8, leaveOpen))
        {
            bw.Write(Encoding.ASCII.GetBytes("r3d2wght"));
            bw.Write(1);
            bw.Write(0);
            bw.Write(Weights.Count);

            foreach (var weight in Weights)
            {
                weight.Write(bw);
            }
        }
    }
}
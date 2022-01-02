namespace LeagueToolkit.IO.WGT;

/// <summary>
///     Represents a Weight Entry inside of a <see cref="WGTFile" />
/// </summary>
public class WGTWeight
{
    /// <summary>
    ///     Initializes a new <see cref="WGTWeight" />
    /// </summary>
    /// <param name="weights">Weights of this <see cref="WGTWeight" /></param>
    /// <param name="boneIndices">Bone Indices of this <see cref="WGTWeight" /></param>
    public WGTWeight(float[] weights, byte[] boneIndices)
    {
        Weights = weights;
        BoneIndices = boneIndices;
    }

    /// <summary>
    ///     Initializes a new <see cref="WGTWeight" /> from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public WGTWeight(BinaryReader br)
    {
        BoneIndices = new[]
        {
            br.ReadByte(),
            br.ReadByte(),
            br.ReadByte(),
            br.ReadByte()
        };
        Weights = new[]
        {
            br.ReadSingle(),
            br.ReadSingle(),
            br.ReadSingle(),
            br.ReadSingle()
        };
    }

    /// <summary>
    ///     Bone Indices of this <see cref="WGTWeight" />
    /// </summary>
    public byte[] BoneIndices { get; set; }

    /// <summary>
    ///     Weights of this <see cref="WGTWeight" />
    /// </summary>
    public float[] Weights { get; set; }

    /// <summary>
    ///     Writes this <see cref="WGTWeight" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw">The <see cref="BinaryWriter" /> to write to</param>
    public void Write(BinaryWriter bw)
    {
        for (var i = 0; i < 4; i++) bw.Write(BoneIndices[i]);
        for (var i = 0; i < 4; i++) bw.Write(Weights[i]);
    }
}
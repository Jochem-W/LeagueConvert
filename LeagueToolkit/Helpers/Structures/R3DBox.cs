using System.Numerics;
using LeagueToolkit.Helpers.Extensions;

namespace LeagueToolkit.Helpers.Structures;

/// <summary>
///     Represents an Axis-Aligned Bounding Box
/// </summary>
public class R3DBox
{
    /// <summary>
    ///     Initializes a new <see cref="R3DBox" /> instance
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public R3DBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    ///     Initializes a new <see cref="R3DBox" /> instance from a <see cref="BinaryReader" />
    /// </summary>
    /// <param name="br">The <see cref="BinaryReader" /> to read from</param>
    public R3DBox(BinaryReader br)
    {
        Min = br.ReadVector3();
        Max = br.ReadVector3();
    }

    /// <summary>
    ///     Creates a clone of a <see cref="R3DBox" /> object
    /// </summary>
    /// <param name="r3dBox">The <see cref="R3DBox" /> to clone</param>
    public R3DBox(R3DBox r3dBox)
    {
        Min = r3dBox.Min;
        Max = r3dBox.Max;
    }

    public Vector3 Min { get; }
    public Vector3 Max { get; }

    /// <summary>
    ///     Writes this <see cref="R3DBox" /> into a <see cref="BinaryWriter" />
    /// </summary>
    /// <param name="bw"></param>
    public void Write(BinaryWriter bw)
    {
        bw.WriteVector3(Min);
        bw.WriteVector3(Max);
    }

    /// <summary>
    ///     Calculates the proportions of this <see cref="R3DBox" />
    /// </summary>
    public Vector3 GetProportions()
    {
        return Max - Min;
    }

    public Vector3 GetCentralPoint()
    {
        return new Vector3(
            0.5f * (Min.X + Max.X),
            0.5f * (Min.Y + Max.Y),
            0.5f * (Min.Z + Max.Z));
    }

    public R3DSphere GetBoundingSphere()
    {
        var centralPoint = GetCentralPoint();

        return new R3DSphere(centralPoint, Vector3.Distance(centralPoint, Max));
    }

    /// <summary>
    ///     Determines wheter this <see cref="R3DBox" /> contains the <see cref="Vector3" /> <paramref name="point" />
    /// </summary>
    /// <param name="point">The containing point</param>
    /// <returns>Wheter this <see cref="R3DBox" /> contains the <see cref="Vector3" /> <paramref name="point" /></returns>
    public bool ContainsPoint(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X
                                && point.Y >= Min.Y && point.Y <= Max.Y
                                && point.Z >= Min.Z && point.Z <= Max.Z;
    }
}
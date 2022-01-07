using System.Numerics;

namespace LeagueToolkit.IO.AnimationFile;

public class AnimationTrack
{
    internal AnimationTrack(uint jointNameHash)
    {
        JointNameHash = jointNameHash;
    }

    public uint JointNameHash { get; }
    public IDictionary<float, Vector3> Translations { get; } = new Dictionary<float, Vector3>();
    public IDictionary<float, Vector3> Scales { get; } = new Dictionary<float, Vector3>();
    public IDictionary<float, Quaternion> Rotations { get; } = new Dictionary<float, Quaternion>();
}
using System.Numerics;

namespace LeagueToolkit.IO.AnimationFile;

public class AnimationTrack
{
    internal AnimationTrack(uint jointNameHash)
    {
        JointNameHash = jointNameHash;
    }

    public uint JointNameHash { get; }

    public Dictionary<float, Vector3> Translations { get; } = new();
    public Dictionary<float, Vector3> Scales { get; } = new();
    public Dictionary<float, Quaternion> Rotations { get; } = new();
}
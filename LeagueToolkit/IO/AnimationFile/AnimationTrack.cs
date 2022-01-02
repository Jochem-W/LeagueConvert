using System.Numerics;

namespace LeagueToolkit.IO.AnimationFile;

public class AnimationTrack
{
    internal AnimationTrack(uint jointHash)
    {
        JointHash = jointHash;
    }

    public uint JointHash { get; }

    public Dictionary<float, Vector3> Translations { get; internal set; } = new();
    public Dictionary<float, Vector3> Scales { get; internal set; } = new();
    public Dictionary<float, Quaternion> Rotations { get; internal set; } = new();
}
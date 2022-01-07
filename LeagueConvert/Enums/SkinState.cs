namespace LeagueConvert.Enums;

[Flags]
public enum SkinState
{
    MeshLoaded = 1,
    TexturesLoaded = 2,
    SkeletonLoaded = 4,
    AnimationsLoaded = 8
}

public static class SkinStateExtensions
{
    public static bool HasFlagFast(this SkinState value, SkinState flag)
    {
        return (value & flag) != 0;
    }
}
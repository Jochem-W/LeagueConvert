namespace LeagueConvert.Enums;

[Flags]
public enum SkinState
{
    MeshLoaded = 1,
    TexturesLoaded = 2,
    SkeletonLoaded = 4,
    AnimationsLoaded = 8
}
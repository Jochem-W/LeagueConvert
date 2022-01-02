namespace LeagueToolkit.Helpers;

public static class FileVersionProvider
{
    private static readonly Dictionary<LeagueFileType, Version[]> SUPPORTED_VERSIONS = new()
    {
        {
            LeagueFileType.Animation, new[]
            {
                new(3, 0, 0, 0),
                new Version(4, 0, 0, 0),
                new Version(5, 0, 0, 0)
            }
        },
        {
            LeagueFileType.MapGeometry, new[]
            {
                new Version(5, 0, 0, 0),
                new Version(6, 0, 0, 0),
                new Version(7, 0, 0, 0),
                new Version(9, 0, 0, 0),
                new Version(11, 0, 0, 0)
            }
        },
        {
            LeagueFileType.PropertyBin, new[]
            {
                new Version(1, 0, 0, 0),
                new Version(2, 0, 0, 0),
                new Version(3, 0, 0, 0)
            }
        }
    };

    public static Version[] GetSupportedVersions(LeagueFileType fileType)
    {
        return SUPPORTED_VERSIONS[fileType];
    }
}
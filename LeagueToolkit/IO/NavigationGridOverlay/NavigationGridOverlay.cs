using LeagueToolkit.Helpers.Exceptions;

namespace LeagueToolkit.IO.NavigationGridOverlay;

public class NavigationGridOverlay
{
    public NavigationGridOverlay(string fileLocation) : this(File.OpenRead(fileLocation))
    {
    }

    public NavigationGridOverlay(Stream stream)
    {
        using (var br = new BinaryReader(stream))
        {
            var version = br.ReadByte();
            if (version != 1) throw new UnsupportedFileVersionException();

            var regionCount = br.ReadByte();
            for (var i = 0; i < regionCount; i++) Regions.Add(new NavigationGridOverlayRegion(br));
        }
    }

    public List<NavigationGridOverlayRegion> Regions { get; set; } = new();
}
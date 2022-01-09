namespace LeagueToolkit.IO.NavigationGridOverlay;

public class NavigationGridOverlayRegion
{
    public NavigationGridOverlayRegion(BinaryReader br)
    {
        X = br.ReadUInt32();
        Y = br.ReadUInt32();
        Width = br.ReadUInt32();
        Height = br.ReadUInt32();

        CellFlags = new List<List<NavigationGridCellFlags>>((int) Height);
        for (var i = 0; i < Height; i++)
        {
            var line = new List<NavigationGridCellFlags>((int) Width);
            for (var j = 0; j < Width; j++) line.Add((NavigationGridCellFlags) br.ReadUInt16());

            CellFlags.Add(line);
        }
    }

    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
    public List<List<NavigationGridCellFlags>> CellFlags { get; set; }
}

[Flags]
public enum NavigationGridCellFlags : ushort
{
    HAS_GRASS = 0x1,
    NOT_PASSABLE = 0x2,
    BUSY = 0x4,
    TARGETTED = 0x8,
    MARKED = 0x10,
    PATHED_ON = 0x20,
    SEE_THROUGH = 0x40,
    OTHER_DIRECTION_END_TO_START = 0x80,

    HAS_GLOBAL_VISION = 0x100
    // HAS_TRANSPARENT_TERRAIN = 0x42 // (SeeThrough | NotPassable)
}
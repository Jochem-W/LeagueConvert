using LeagueToolkit.IO.WadFile;

namespace LeagueConvert.IO.WadFile;

public class ParentedWadEntry
{
    internal ParentedWadEntry(StringWad wad, WadEntry value)
    {
        Parent = wad;
        Value = value;
    }

    public StringWad Parent { get; }
    public WadEntry Value { get; }

    public Stream GetStream(bool decompress = true)
    {
        return decompress
            ? Value.GetDataHandle().GetDecompressedStream()
            : Value.GetDataHandle().GetCompressedStream();
    }
}
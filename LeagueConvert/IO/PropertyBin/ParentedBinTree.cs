using LeagueConvert.IO.WadFile;
using LeagueToolkit.IO.PropertyBin;

namespace LeagueConvert.IO.PropertyBin;

public class ParentedBinTree : BinTree
{
    internal readonly StringWad Parent;

    private ParentedBinTree(StringWad parent, Stream stream) : base(stream)
    {
        Parent = parent;
    }

    public static async Task<ParentedBinTree> FromWadEntry(ParentedWadEntry entry)
    {
        await using var stream = entry.GetStream();
        var binTree = new ParentedBinTree(entry.Parent, stream);
        return binTree;
    }
}
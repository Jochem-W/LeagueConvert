using System.IO;
using System.Threading.Tasks;
using LeagueConvert.Helpers;
using LeagueConvert.IO.WadFile;
using LeagueToolkit.IO.PropertyBin;

namespace LeagueConvert.IO.PropertyBin
{
    public class ParentedBinTree : BinTree
    {
        internal readonly StringWad Parent;

        private ParentedBinTree(StringWad parent, Stream stream) : base(stream)
        {
            Parent = parent;
        }

        public static async Task<ParentedBinTree> FromWadEntry(ParentedWadEntry entry)
        {
            var stream = await entry.GetStream().Version3ToVersion2();
            var binTree = new ParentedBinTree(entry.Parent, stream);
            await stream.DisposeAsync();
            return binTree;
        }
    }
}
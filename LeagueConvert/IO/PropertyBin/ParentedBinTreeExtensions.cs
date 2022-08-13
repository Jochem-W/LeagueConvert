using LeagueConvert.IO.WadFile;

namespace LeagueConvert.IO.PropertyBin;

public static class ParentedBinTreeExtensions
{
    public static async Task<IList<ParentedBinTree>> GetDependenciesRecurseAsync(this ParentedBinTree binTree,
        StringWad stringWad, bool ignore = false)
    {
        var dependencies = new List<ParentedBinTree>();
        foreach (var dependency in binTree.Dependencies)
        {
            try
            {
                var dependencyBinTree = await ParentedBinTree.FromWadEntry(stringWad.GetEntryByName(dependency));
                dependencies.Add(dependencyBinTree);
                dependencies.AddRange(await dependencyBinTree.GetDependenciesRecurseAsync(stringWad, ignore));
            }
            catch (Exception)
            {
                if (!ignore)
                {
                    throw;
                }
            }
        }

        return dependencies;
    }
}
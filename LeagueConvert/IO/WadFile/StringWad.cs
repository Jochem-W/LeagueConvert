using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueConvert.IO.PropertyBin;
using LeagueToolkit.IO.WadFile;
using Serilog;

namespace LeagueConvert.IO.WadFile
{
    public class StringWad : IDisposable
    {
        private readonly Wad _wad;

        public StringWad(string filePath, bool leaveOpen = false)
        {
            FilePath = filePath;
            _wad = Wad.Mount(FilePath, leaveOpen);
            Entries = _wad.Entries
                .Where(pair => HashTables.HashTables.Game.ContainsKey(pair.Key))
                .Select(pair =>
                    new KeyValuePair<string, ParentedWadEntry>(HashTables.HashTables.Game[pair.Key],
                        new ParentedWadEntry(this, pair.Value)));
        }

        public IEnumerable<KeyValuePair<string, ParentedWadEntry>> Entries { get; }
        public string FilePath { get; }

        public void Dispose()
        {
            _wad.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool EntryExists(string name)
        {
            return Entries.Any(pair => string.Equals(pair.Key, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public ParentedWadEntry GetEntryByName(string name)
        {
            if (name == null)
                throw new NotImplementedException();
            var entry = Entries.FirstOrDefault(pair => pair.Key == name.ToLower()).Value;
            if (entry == null)
                throw new NotImplementedException();
            return entry;
        }

        public async IAsyncEnumerable<Skin.Skin> GetSkins(ILogger logger = null)
        {
            var skinEntries = new Dictionary<KeyValuePair<string, string>, ParentedWadEntry>();
            foreach (var (path, entry) in Entries)
            {
                var split = path.Split('/');
                if (split[0] != "data" ||
                    split[1] != "characters" ||
                    split[3] != "skins")
                    continue;
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrWhiteSpace(fileName) || fileName == "root")
                    continue;
                skinEntries[new KeyValuePair<string, string>(split[2], fileName)] = entry;
            }

            foreach (var ((character, skinName), skinEntry) in skinEntries)
            {
                var binTrees = new List<ParentedBinTree>();
                var skinBinTree = await ParentedBinTree.FromWadEntry(skinEntry);
                binTrees.Add(skinBinTree);
                foreach (var dependency in skinBinTree.Dependencies.Select(d => d))
                {
                    var split = dependency.Split('/');
                    if (split[0] != "DATA" ||
                        split[1] != "Characters" ||
                        split[3] != "Animations")
                        continue;
                    try
                    {
                        binTrees.Add(await ParentedBinTree.FromWadEntry(GetEntryByName(dependency)));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                yield return new Skin.Skin(character, skinName, logger, binTrees.ToArray());
            }
        }
    }
}
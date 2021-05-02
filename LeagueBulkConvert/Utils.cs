using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;
using LeagueToolkit.IO.WadFile;
using Serilog;

namespace LeagueBulkConvert
{
    public static class Utils
    {
        public static async Task Convert(Config config, ILogger logger = null,
            CancellationToken? cancellationToken = null)
        {
            var hashTables = await ReadHashTables(logger);
            foreach (var wad in config.Wads.Where(w => w.Included))
            {
                if (cancellationToken is {IsCancellationRequested: true})
                    return;
                await ConvertWad(wad.FilePath, hashTables, config, logger, cancellationToken);
            }

            logger?.Information("Finished!");
        }

        public static async Task ConvertWad(string wadPath, IDictionary<string, IDictionary<ulong, string>> hashTables,
            Config config,
            ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            logger?.Information("Extracting {FileName}", Path.GetFileName(wadPath));
            IDictionary<string, WadEntry> skinEntries = new Dictionary<string, WadEntry>();
            using var wad = Wad.Mount(wadPath, true);
            foreach (var entry in wad.Entries.Where(e => hashTables["game"].ContainsKey(e.Key)))
            {
                if (cancellationToken is {IsCancellationRequested: true})
                    return;
                var entryPath = hashTables["game"][entry.Key].ToLower();
                if (entryPath.EndsWith(".bin") && entryPath.Contains(@"/skins/") && !entryPath.Contains("root"))
                    skinEntries.Add(entryPath, entry.Value);
                if (!(config.ExtractFormats.Contains(Path.GetExtension(entryPath)) &
                      !(entryPath.EndsWith(".bin") && !entryPath.Contains("animations")))) continue;
                var folderPath = Path.GetDirectoryName(entryPath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                await using var outputFile = File.Create(entryPath);
                await using var stream = entry.Value.GetDataHandle().GetDecompressedStream();
                await stream.CopyToAsync(outputFile);
            }

            foreach (var (entryPath, wadEntry) in skinEntries)
            {
                if (cancellationToken is {IsCancellationRequested: true})
                    return;
                var splitName = entryPath.Split('/');
                var character = splitName[^3];
                logger?.Information("Converting {Name}", string.Join('/', splitName.TakeLast(3)));
                var binTree = new BinTree(wadEntry.GetDataHandle().GetDecompressedStream());
                var skin = new Skin(character, Path.GetFileNameWithoutExtension(entryPath), binTree, config);
                if (!skin.Exists)
                    continue;
                if (config.IncludeAnimations)
                    foreach (var dependencyPath in binTree.Dependencies)
                    {
                        if (cancellationToken is {IsCancellationRequested: true})
                            return;
                        if (!dependencyPath.ToLower().Contains("/animations/") || !File.Exists(dependencyPath))
                            continue;
                        try
                        {
                            await skin.AddAnimations(dependencyPath, hashTables, config, logger);
                        }
                        catch (Exception e)
                        {
                            logger?.Warning(e, "Couldn't add animations");
                            return;
                        }
                    }

                skin.FixTextures();
                try
                {
                    await skin.Save(config, logger);
                }
                catch (Exception e)
                {
                    logger?.Warning(e, "Couldn't save");
                }
            }

            logger?.Information("Cleaning");
            if (Directory.Exists("assets"))
                Directory.Delete("assets", true);
            if (Directory.Exists("data"))
                Directory.Delete("data", true);
            if (Directory.Exists("levels"))
                Directory.Delete("levels", true);
        }

        internal static bool FindTexture(BinTreeObject treeObject, Config config, out string texture)
        {
            texture = string.Empty;
            var samplers = treeObject.Properties.FirstOrDefault(p => p.NameHash == 175050421); //samplerValues
            if (samplers == null)
                return false;
            foreach (BinTreeEmbedded sampler in ((BinTreeContainer) samplers).Properties)
            {
                var samplerNameProperty = sampler.Properties.FirstOrDefault(p => p.NameHash == 48757580); //samplerName
                if (samplerNameProperty == null)
                    continue;
                var textureProperty = sampler.Properties.FirstOrDefault(p => p.NameHash == 3004290287); //textureName
                if (textureProperty == null)
                    continue;
                var samplerName = ((BinTreeString) samplerNameProperty).Value;
                if (!config.SamplerNames.Contains(samplerName))
                    continue;
                texture = ((BinTreeString) textureProperty).Value.ToLower();
                return true;
            }

            return false;
        }

        public static async Task<IDictionary<string, IDictionary<ulong, string>>> ReadHashTables(ILogger logger = null)
        {
            logger?.Information("Reading hash tables");
            IDictionary<string, IDictionary<ulong, string>> hashTables =
                new Dictionary<string, IDictionary<ulong, string>>();
            foreach (var file in Directory.EnumerateFiles("hashes", "*.txt"))
            {
                IDictionary<ulong, string> hashTable = new Dictionary<ulong, string>();
                foreach (var line in (await File.ReadAllLinesAsync(file)).SkipLast(1))
                {
                    var splitLine = line.Split(' ');
                    var ulongHash = ulong.Parse(splitLine[0], NumberStyles.HexNumber);
                    if (!hashTable.ContainsKey(ulongHash))
                        hashTable[ulongHash] = splitLine[1];
                }

                hashTables[Path.GetFileNameWithoutExtension(file).Split('.')[1]] = hashTable;
            }

            return hashTables;
        }
    }
}
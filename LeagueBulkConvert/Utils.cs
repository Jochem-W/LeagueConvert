using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;
using LeagueToolkit.IO.WadFile;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueBulkConvert
{
    public static class Utils
    {
        public static async Task Convert(Config config, ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            var hashTables = await ReadHashTables();
            foreach (var wad in config.Wads.Where(w => w.Included))
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return;
                await ConvertWad(wad.FilePath, hashTables, config, logger, cancellationToken);
            }
                
        }

        public static async Task ConvertWad(string wadPath, IDictionary<string, IDictionary<ulong, string>> hashTables, Config config,
            ILogger logger = null, CancellationToken? cancellationToken = null)
        {
            if (logger != null)
                logger.Information($"Extracting {Path.GetFileName(wadPath)}");
            IDictionary<string, WadEntry> skinEntries = new Dictionary<string, WadEntry>();
            using var wad = Wad.Mount(wadPath, true);
            foreach (var entry in wad.Entries.Where(e => hashTables["game"].ContainsKey(e.Key)))
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return;
                var entryPath = hashTables["game"][entry.Key].ToLower();
                if (entryPath.EndsWith(".bin") && entryPath.Contains(@"/skins/") && !entryPath.Contains("root"))
                    skinEntries.Add(entryPath, entry.Value);
                if (config.ExtractFormats.Contains(Path.GetExtension(entryPath)) & !(entryPath.EndsWith(".bin") && !entryPath.Contains("animations")))
                {
                    var folderPath = Path.GetDirectoryName(entryPath);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    await using var outputFile = File.Create(entryPath);
                    await using var stream = entry.Value.GetDataHandle().GetDecompressedStream();
                    await stream.CopyToAsync(outputFile);
                }
            }
            foreach (var (entryPath, wadEntry) in skinEntries)
            {
                if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                    return;
                var splitName = entryPath.Split('/');
                var character = splitName[^3];
                if (logger != null)
                    logger.Information($"  Converting {string.Join('/', splitName.TakeLast(3))}");
                BinTree binTree;
                if (config.ReadVersion3)
                    binTree = await ReadVersion3(wadEntry.GetDataHandle().GetDecompressedStream());
                else
                    binTree = new BinTree(wadEntry.GetDataHandle().GetDecompressedStream());
                var skin = new Skin(character, Path.GetFileNameWithoutExtension(entryPath), binTree, config);
                if (!skin.Exists)
                    continue;
                if (config.IncludeAnimations)
                    foreach (var dependencyPath in binTree.Dependencies)
                    {
                        if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested)
                            return;
                        if (dependencyPath.ToLower().Contains("/animations/") && File.Exists(dependencyPath))
                            try
                            {
                                await skin.AddAnimations(dependencyPath, hashTables, config, logger);
                            }
                            catch (Exception)
                            {
                                if (logger != null)
                                    logger.Information("    Couldn't add animations");
                                return;
                            }
                    }

                skin.Clean();
                try
                {
                    skin.Save(config, logger);
                }
                catch (Exception)
                {
                    if (logger != null)
                        logger.Information("    Couldn't save");
                }
            }
        }

        internal static bool FindTexture(BinTreeObject treeObject, Config config, out string texture)
        {
            texture = string.Empty;
            var samplers = treeObject.Properties.FirstOrDefault(p => p.NameHash == 175050421); //samplerValues
            if (samplers == null)
                return false;
            foreach (BinTreeEmbedded sampler in ((BinTreeContainer)samplers).Properties)
            {
                var samplerNameProperty = sampler.Properties.FirstOrDefault(p => p.NameHash == 48757580); //samplerName
                if (samplerNameProperty == null)
                    continue;
                var textureProperty = sampler.Properties.FirstOrDefault(p => p.NameHash == 3004290287); //textureName
                if (textureProperty == null)
                    continue;
                var samplerName = ((BinTreeString)samplerNameProperty).Value;
                if (!config.SamplerNames.Contains(samplerName))
                    continue;
                texture = ((BinTreeString)textureProperty).Value.ToLower();
                return true;
            }
            return false;
        }

        public static async Task<IDictionary<string, IDictionary<ulong, string>>> ReadHashTables()
        {
            if (!File.Exists("hashes/hashes.binhashes.txt") || !File.Exists("hashes/hashes.game.txt"))
                return null;
            IDictionary<string, IDictionary<ulong, string>> hashTables = new Dictionary<string, IDictionary<ulong, string>>();
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

        internal static async Task<BinTree> ReadVersion3(string fileName)
        {
            await using var stream = File.OpenRead(fileName);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return await ReadVersion3(memoryStream);
        }

        internal static async Task<BinTree> ReadVersion3(Stream stream)
        {
            var versionBuffer = new byte[1];
            stream.Position = 4;
            await stream.ReadAsync(versionBuffer.AsMemory(0, 1));
            if (versionBuffer[0] == 3)
            {
                stream.Position = 4;
                await stream.WriteAsync((new byte[1] { 2 }).AsMemory(0, 1));
            }
            stream.Position = 0;
            var binTree = new BinTree(stream);
            return binTree;
        }
    }
}
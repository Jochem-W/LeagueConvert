using LeagueBulkConvert.ViewModels;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.PropertyBin.Properties;
using LeagueToolkit.IO.WadFile;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Conversion
{
    static class Utils
    {
        internal static async Task ExtractWad(Wad wad)
        {
            foreach (var entry in wad.Entries)
            {
                if (!Converter.HashTables["game"].ContainsKey(entry.Key))
                    continue;
                var path = Converter.HashTables["game"][entry.Key].ToLower().Replace('/', '\\');
                if (!Converter.Config.ExtractFormats.Contains(Path.GetExtension(path)))
                    continue;
                if (path.EndsWith(".bin") && !path.Contains("animations"))
                    continue;
                var folderPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                var outputFile = File.Create(path);
                await entry.Value.GetDataHandle().GetDecompressedStream().CopyToAsync(outputFile);
                await outputFile.DisposeAsync();
            }
        }

        internal static bool FindTexture(BinTreeObject treeObject, out string texture)
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
                if (!Converter.Config.SamplerNames.Contains(samplerName))
                    continue;
                texture = ((BinTreeString)textureProperty).Value.ToLower().Replace('/', '\\');
                return true;
            }
            return false;
        }

        internal static async Task ReadHashTables(MainWindowViewModel viewModel)
        {
            await UpdateHashes(viewModel);
            foreach (var file in Directory.EnumerateFiles($"{Environment.CurrentDirectory}\\hashes", "*.txt"))
            {
                var lines = await File.ReadAllLinesAsync(file);
                IDictionary<ulong, string> hashTable = new Dictionary<ulong, string>();
                foreach (var line in lines.SkipLast(1))
                {
                    var splitLine = line.Split(' ');
                    var ulongHash = ulong.Parse(splitLine[0], NumberStyles.HexNumber);
                    if (!hashTable.ContainsKey(ulongHash))
                        hashTable[ulongHash] = splitLine[1];
                }
                Converter.HashTables[Path.GetFileNameWithoutExtension(file).Split('.')[1]] = hashTable;
            }
        }

        internal static async Task UpdateHashes(MainWindowViewModel viewModel)
        {
            if (!Directory.Exists("hashes"))
                Directory.CreateDirectory("hashes");
            foreach (var file in (await viewModel.GitHubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox"))
                .Where(f => f.Name == "hashes.binhashes.txt" || f.Name == "hashes.game.txt"))
            {
                var filePath = @$"hashes\{file.Name}";
                var shaFilePath = $"{filePath}.sha";
                if (!File.Exists(filePath) || !File.Exists(shaFilePath) || await File.ReadAllTextAsync(shaFilePath) != file.Sha)
                {
                    var tempFilePath = $"{filePath}.tmp";
                    await File.WriteAllTextAsync(tempFilePath, await viewModel.HttpClient.GetStringAsync(file.DownloadUrl));
                    File.Move(tempFilePath, filePath);
                    await File.WriteAllTextAsync(shaFilePath, file.Sha);
                }
            }
        }

        internal static async Task<BinTree> ReadVersion3(string fileName)
        {
            var stream = File.OpenRead(fileName);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await stream.DisposeAsync();
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
            await stream.DisposeAsync();
            return binTree;
        }
    }
}

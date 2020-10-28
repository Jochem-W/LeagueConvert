using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WadFile;
using Octokit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Conversion
{
    static class Utils
    {
        /* This needs to be updated to accomodate for the new UI
        
        public static void CheckMissing(IList<Json.Champion> champions)
        {
            foreach (var champion in champions)
            {
                var path = $"export\\assets\\{champion.Folder}";
                if (!Directory.Exists(path))
                {
                    Console.WriteLine($"Missing {champion.Folder}");
                    continue;
                }
                foreach (var skin in champion.Skins)
                {
                    if (skin.Chromas is null)
                    {
                        var skinPath = $"{path}\\skin{skin.Key}.glb";
                        if (!File.Exists(skinPath))
                            Console.WriteLine($"Missing {skinPath}");
                        continue;
                    }
                    foreach (var chroma in skin.Chromas)
                    {
                        var chromaPath = $"{path}\\skin{chroma.Key}.glb";
                        if (!File.Exists(chromaPath))
                            Console.WriteLine($"Missing {chromaPath}");
                    }
                }
            }
        }*/

        public static async Task ExtractWad(Wad wad)
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

        public static string FindTexture(BINEntry entry)
        {
            var values = entry.Values.FirstOrDefault(v => v.Property == 175050421); //samplerValues
            if (values is null)
                return string.Empty;
            var samplerValues = (BINContainer)values.Value;
            foreach (var samplerValue in samplerValues.Values)
            {
                var things = ((BINStructure)samplerValue.Value).Values;
                var textureType = (string)things.FirstOrDefault(v => v.Property == 48757580).Value; //samplerName
                if (Converter.Config.SamplerNames.Contains(textureType))
                    return ((string)things.First(v => v.Property == 3004290287).Value).ToLower().Replace('/', '\\'); //textureName
            }
            return string.Empty;
        }

        public static async Task ReadHashTables()
        {
            await UpdateHashes();
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

        public static async Task UpdateHashes()
        {
            if (!Directory.Exists("hashes"))
                Directory.CreateDirectory("hashes");
            IGitHubClient gitHubClient = new GitHubClient(new ProductHeaderValue("Jochem-W"));
            var files = await gitHubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox");
            foreach (var file in files.Where(c => c.Name.Contains("hashes") && c.Name.EndsWith(".txt")))
            {
                var filePath = $"{Environment.CurrentDirectory}\\hashes\\{file.Name}";
                var shaFilePath = $"{filePath}.sha";
                if (!File.Exists(filePath) || !File.Exists(shaFilePath) || await File.ReadAllTextAsync(shaFilePath) != file.Sha)
                {
                    var httpClient = new HttpClient();
                    await File.WriteAllTextAsync(filePath, await httpClient.GetStringAsync(file.DownloadUrl));
                    httpClient.Dispose();
                    await File.WriteAllTextAsync(shaFilePath, file.Sha);
                }
            }
        }
    }
}

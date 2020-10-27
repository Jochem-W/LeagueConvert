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

namespace LeagueBulkConvert.Converter
{
    static class Utils
    {
        /*public static async Task CheckColours()
        {
            var fileStream = File.OpenRead("colours.min.json");
            var coloursIn = await JsonSerializer.DeserializeAsync<IDictionary<string, IList<IList<string>>>>(fileStream);
            await fileStream.DisposeAsync();
            var translatedColours = new List<IList<string>>();
            foreach (var colours in coloursIn.Values)
                foreach (var colour in colours)
                    translatedColours.Add(colour);
            var cDragonFileStream = File.OpenRead("skins.json");
            var cDragonData = await JsonSerializer.DeserializeAsync<Dictionary<string, CommunityDragon.Skin>>(cDragonFileStream, SerializerOptions);
            await cDragonFileStream.DisposeAsync();
            foreach (var skin in cDragonData.Values.Where(s => !(s.Chromas is null)))
                foreach (var chroma in skin.Chromas)
                {
                    if (translatedColours.FindIndex(c => c[0] == chroma.Colours[0] && c[1] == chroma.Colours[1]) != -1)
                        continue;
                    Console.WriteLine($"\"{chroma.Colours[0]}\",\n                    \"{chroma.Colours[1]}");
                    var key = Console.ReadLine();
                    Console.WriteLine();
                    if (coloursIn.ContainsKey(key))
                        coloursIn[key].Add(chroma.Colours);
                    else
                        coloursIn[key] = new List<IList<string>> { chroma.Colours };
                    translatedColours.Add(chroma.Colours);
                }
            fileStream = File.Create("colours.min.json");
            await JsonSerializer.SerializeAsync(fileStream, coloursIn, SerializerOptions);
            await fileStream.DisposeAsync();
        }*/

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
                var ext = Path.GetExtension(path);
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

        public static string SimplifyKey(string key)
        {
            return key.Substring(key.Length - 3).TrimStart('0').PadLeft(1, '0');
        }

        public static string SimplifyKey(int key)
        {
            return SimplifyKey(key.ToString());
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

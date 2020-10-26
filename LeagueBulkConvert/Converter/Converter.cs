using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WadFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Converter
{
    static class Converter
    {
        public static Config Config;

        public static readonly IDictionary<string, IDictionary<ulong, string>> HashTables = new Dictionary<string, IDictionary<ulong, string>>();

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static async Task StartConversion(string installPath, string outPath)
        {
            var fileStream = File.OpenRead("config.json");
            Config = await JsonSerializer.DeserializeAsync<Config>(fileStream, SerializerOptions);
            Config.CalculateScale();
            await fileStream.DisposeAsync();
            var currentDirectory = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(outPath);
            await Utils.ReadHashTables();
            foreach (var path in Directory.EnumerateFiles($"{installPath}\\Game\\DATA\\FINAL\\Champions", "*.wad.client").Where(f => !f.Contains('_')))
            {
                if (Config.IncludeOnly.Count != 0 && Config.IncludeOnly.FirstOrDefault(p => path.Contains(p)) is null)
                    continue;
                var wad = Wad.Mount(path, true);
                await Utils.ExtractWad(wad);
                foreach (var entry in wad.Entries)
                {
                    if (!HashTables["game"].ContainsKey(entry.Key))
                        continue;
                    var name = HashTables["game"][entry.Key].ToLower().Replace('/', '\\');
                    if (!name.EndsWith(".bin") || !name.Contains("\\skins\\") || name.Contains("root"))
                        continue;
                    var splitName = name.Split('\\');
                    Console.WriteLine(string.Join('\\', splitName.TakeLast(3)));
                    var character = splitName.SkipLast(2).Last();
                    if (Config.IgnoreCharacters.Contains(character))
                        continue;
                    var binFile = new BINFile(entry.Value.GetDataHandle().GetDecompressedStream());
                    //await File.WriteAllTextAsync("current.json", Newtonsoft.Json.JsonConvert.SerializeObject(binFile, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }));
                    var skin = new Skin(character, splitName.Last().Split('.')[0], binFile);
                    if (!skin.Exists)
                        continue;
                    skin.Clean();
                    skin.Save();
                }
                wad.Dispose();
            }
            Directory.Delete("assets", true);
            //await CheckColours();
            //await Json.Utils.Export();
            Directory.SetCurrentDirectory(currentDirectory);
        }
    }
}
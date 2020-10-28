using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WadFile;
using LeagueBulkConvert.MVVM.ViewModels;
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

        public static readonly IDictionary<string, IDictionary<ulong, string>> HashTables =
            new Dictionary<string, IDictionary<ulong, string>>();

        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task StartConversion(MainViewModel viewModel, LoggingViewModel loggingViewModel)
        {
            loggingViewModel.AddLine("Reading config.json");
            var fileStream = File.OpenRead("config.json");
            Config = await JsonSerializer.DeserializeAsync<Config>(fileStream, SerializerOptions);
            await fileStream.DisposeAsync();
            Config.CalculateScale();
            if (viewModel.IncludeSkeletons)
                Config.ExtractFormats.Add(".skl");
            if (viewModel.IncludeAnimations)
            {
                Config.ExtractFormats.Add(".anm");
                Config.ExtractFormats.Add(".bin");
            }
            var currentDirectory = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(viewModel.OutPath);
            if (Directory.Exists("assets"))
                Directory.Delete("assets", true);
            if (Directory.Exists("data"))
                Directory.Delete("data", true);
            loggingViewModel.AddLine("Reading hashtables");
            await Utils.ReadHashTables();
            foreach (var path in Directory.EnumerateFiles($"{viewModel.LeaguePath}\\Game\\DATA\\FINAL\\Champions", "*.wad.client")
                                          .Where(f => !f.Contains('_')
                                                      && (Config.IncludeOnly.Count == 0
                                                      || Config.IncludeOnly.Contains(Path.GetFileName(f)))))
            {
                loggingViewModel.AddLine($"Extracting {path}");
                var wad = Wad.Mount(path, true);
                await Utils.ExtractWad(wad);
                foreach (var entry in wad.Entries.Where(e => HashTables["game"].ContainsKey(e.Key)))
                {
                    var name = HashTables["game"][entry.Key].ToLower().Replace('/', '\\');
                    if (!name.EndsWith(".bin") || !name.Contains("\\skins\\") || name.Contains("root"))
                        continue;
                    var splitName = name.Split('\\');
                    var character = splitName[^3];
                    if (Config.IgnoreCharacters.Contains(character))
                        continue;
                    loggingViewModel.AddLine($"Converting {string.Join('\\', splitName.TakeLast(3))}", 1);
                    var binFile = new BINFile(entry.Value.GetDataHandle().GetDecompressedStream());
                    var skin = new Skin(character, Path.GetFileNameWithoutExtension(name), binFile, viewModel, loggingViewModel);
                    if (!skin.Exists)
                        continue;
                    skin.Clean();
                    try
                    {
                        skin.Save(viewModel, loggingViewModel);
                    }
                    catch (Exception)
                    {
                        loggingViewModel.AddLine("Couldn't save", 2);
                    }
                }
                wad.Dispose();
                loggingViewModel.AddLine("Cleaning", 1);
                Directory.Delete("assets", true);
                if (viewModel.IncludeAnimations)
                    Directory.Delete("data", true);
            }
            loggingViewModel.AddLine("Finished!");
            Directory.SetCurrentDirectory(currentDirectory);
        }
    }
}
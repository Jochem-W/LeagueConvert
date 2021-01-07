using LeagueBulkConvert.ViewModels;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.WadFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeagueBulkConvert.Conversion
{
    static class Converter
    {
        internal static Config Config;

        internal static readonly IDictionary<string, IDictionary<ulong, string>> HashTables = new Dictionary<string, IDictionary<ulong, string>>();

        public static async Task StartConversion(MainWindowViewModel viewModel, LoggingWindowViewModel loggingViewModel)
        {
            loggingViewModel.AddLine("Reading config.json");
            var fileStream = File.OpenRead("config.json");
            Config = await JsonSerializer.DeserializeAsync<Config>(fileStream);
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
            loggingViewModel.AddLine("Cleaning");
            if (Directory.Exists("assets"))
                Directory.Delete("assets", true);
            if (Directory.Exists("data"))
                Directory.Delete("data", true);
            loggingViewModel.AddLine("Reading hashtables");
            await Utils.ReadHashTables(viewModel);
            foreach (var path in Directory.EnumerateFiles(@$"{viewModel.LeaguePath}\Game\DATA\FINAL\Champions", "*.wad.client")
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
                    if (!name.EndsWith(".bin") || !name.Contains(@"\skins\") || name.Contains("root"))
                        continue;
                    var splitName = name.Split('\\');
                    var character = splitName[^3];
                    if (Config.IgnoreCharacters.Contains(character))
                        continue;
                    loggingViewModel.AddLine($"Converting {string.Join('\\', splitName.TakeLast(3))}", 1);
                    BinTree binTree;
                    if (viewModel.ReadVersion3)
                    {
                        var binStream = entry.Value.GetDataHandle().GetDecompressedStream();
                        var versionBuffer = new byte[1];
                        binStream.Position = 4;
                        await binStream.ReadAsync(versionBuffer.AsMemory(0, 1));
                        if (versionBuffer[0] == 3)
                        {
                            binStream.Position = 4;
                            await binStream.WriteAsync((new byte[1] { 2 }).AsMemory(0, 1));
                        }
                        binStream.Position = 0;
                        binTree = new BinTree(binStream);
                        await binStream.DisposeAsync();
                    }
                    else
                        binTree = new BinTree(entry.Value.GetDataHandle().GetDecompressedStream());
                    var skin = new Skin(character, Path.GetFileNameWithoutExtension(name), binTree, viewModel, loggingViewModel);
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
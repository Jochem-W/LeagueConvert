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

        public static readonly IDictionary<string, IDictionary<ulong, string>> HashTables = new Dictionary<string, IDictionary<ulong, string>>();

        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static async Task StartConversion(MainViewModel viewModel)
        {
            var fileStream = File.OpenRead("config.json");
            Config = await JsonSerializer.DeserializeAsync<Config>(fileStream, SerializerOptions);
            Config.CalculateScale();
            await fileStream.DisposeAsync();
            if (viewModel.IncludeSkeletons)
                Config.ExtractFormats.Add(".skl");
            if (viewModel.IncludeAnimations)
            {
                Config.ExtractFormats.Add(".anm");
                Config.ExtractFormats.Add(".bin");
            }
            var currentDirectory = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(viewModel.OutPath);
            await Utils.ReadHashTables();
            foreach (var path in Directory.EnumerateFiles($"{viewModel.LeaguePath}\\Game\\DATA\\FINAL\\Champions", "*.wad.client").Where(f => !f.Contains('_')))
            {
                if (Config.IncludeOnly.Count != 0 && !Config.IncludeOnly.Contains(Path.GetFileName(path)))
                    continue;
                var wad = Wad.Mount(path, true);
                await Utils.ExtractWad(wad);
                IList<Task> tasks = new List<Task>(viewModel.ThreadCount);
                foreach (var entry in wad.Entries)
                {
                    while (tasks.Count >= viewModel.ThreadCount)
                    {
                        var wait = true;
                        for (var i = 0; i < tasks.Count; i++)
                        {
                            var task = tasks[i];
                            if (!task.IsCompleted)
                                continue;
                            task.Dispose();
                            tasks.Remove(task);
                            i--;
                            wait = false;
                        }
                        if (wait)
                            await Task.Delay(100);
                    }
                    tasks.Add(Task.Run(() =>
                    {
                        if (!HashTables["game"].ContainsKey(entry.Key))
                            return;
                        var name = HashTables["game"][entry.Key].ToLower().Replace('/', '\\');
                        if (!name.EndsWith(".bin") || !name.Contains("\\skins\\") || name.Contains("root"))
                            return;
                        var splitName = name.Split('\\');
                        //Console.WriteLine(string.Join('\\', splitName.TakeLast(3)));
                        var character = splitName[^3];
                        if (Config.IgnoreCharacters.Contains(character))
                            return;
                        var binFile = new BINFile(entry.Value.GetDataHandle().GetDecompressedStream());
                        //await File.WriteAllTextAsync("current.json", Newtonsoft.Json.JsonConvert.SerializeObject(binFile, new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }));
                        var skin = new Skin(character, Path.GetFileNameWithoutExtension(name), binFile, viewModel.IncludeAnimations, viewModel.IncludeHiddenMeshes, viewModel.ShowErrors);
                        if (!skin.Exists)
                            return;
                        skin.Clean();
                        skin.Save(viewModel.IncludeSkeletons, viewModel.ShowErrors);
                    }));
                }
                await Task.WhenAll(tasks);
                wad.Dispose();
                Directory.Delete("assets", true);
                Directory.Delete("data", true);
            }
            //await CheckColours();
            //await Json.Utils.Export();
            Directory.SetCurrentDirectory(currentDirectory);
        }
    }
}
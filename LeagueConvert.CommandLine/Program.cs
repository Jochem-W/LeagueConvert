using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using LeagueConvert.Enums;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.Skin;
using LeagueConvert.IO.WadFile;
using Serilog;

namespace LeagueConvert.CommandLine
{
    internal static class Program
    {
        private static readonly ILogger Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        
        private static async Task<int> Main(string[] args)
        {
            await TryCheckForUpdates();
            var rootCommand = new RootCommand
            {
                GetConvertWadCommand(),
                GetConvertAllCommand()
            };
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<bool> TryCheckForUpdates()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                var httpClient = new HttpClient();
                var latestVersion = new Version(await httpClient.GetStringAsync(
                    $"https://api.jochemw.workers.dev/products/{assembly.Name?.ToLower()}/version/latest"));
                httpClient.Dispose();
                if (assembly.Version?.CompareTo(latestVersion) < 0)
                    Logger.Information("A new version of {Name} is available: {Version}", assembly.Name, latestVersion);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Update check failed");
                return false;
            }
        }

        private static Command GetConvertWadCommand()
        {
            var command = new Command("convert-wad", "Convert all models found in the specified WAD file")
            {
                new Argument<string>("path", "Path to a WAD file"),
                new Option<string>(new[] {"-o", "--output-directory"}, () => "output", "Path to the output directory"),
                new Option<bool>(new[] {"-s", "--skeletons"}, () => false, "Include skeletons"),
                new Option<bool>(new[] {"-a", "--animations"}, () => false, "Include animations; requires -s"),
                new Option<string>(new[] {"--game-hash-file"}, "Path to 'hashes.game.txt'"),
                new Option<string>(new[] {"--binhashes-hash-file"}, "Path to 'hashes.binhashes.txt'")
            };
            command.Handler = CommandHandler.Create<string, string, bool, bool, string, string>(
                async (path, outputDirectory, skeletons, animations,
                    gameHashFile, binHashesHashFile) =>
                {
                    if (!await TryLoadHashes(gameHashFile, binHashesHashFile))
                        return;
                    if (!TryCreateOutputDirectory(outputDirectory))
                        return;
                    if (!TryGetSkinMode(skeletons, animations, out var mode) || !mode.HasValue)
                        return;
                    if (!TryLoadWad(path, out var wad))
                        return;
                    await TryConvertWad(wad, mode.Value, outputDirectory);
                    wad.Dispose();
                });
            return command;
        }

        private static Command GetConvertAllCommand()
        {
            var command = new Command("convert-all", "Convert all models in all WAD files in a specified directory")
            {
                new Argument<string>("path", "Path to a folder containing WAD files"),
                new Option<string>(new[] {"-o", "--output-directory"}, () => "output", "Path to the output directory"),
                new Option<bool>(new[] {"-s", "--skeletons"}, () => false, "Include skeletons"),
                new Option<bool>(new[] {"-a", "--animations"}, () => false, "Include animations; requires -s"),
                new Option<bool>(new [] {"-r", "--recursive"}, () => false, "Search for WAD files recursively"),
                new Option<string>(new[] {"--game-hash-file"}, "Path to 'hashes.game.txt'"),
                new Option<string>(new[] {"--binhashes-hash-file"}, "Path to 'hashes.binhashes.txt'")
            };
            command.Handler = CommandHandler.Create<string, string, bool, bool, bool, string, string>(
                async (path, outputDirectory, skeletons, animations, recursive, gameHashFile, binHashesHashFile) => 
            {
                if (!await TryLoadHashes(gameHashFile, binHashesHashFile))
                    return;
                if (!TryCreateOutputDirectory(outputDirectory))
                    return;
                if (!TryGetSkinMode(skeletons, animations, out var mode) || !mode.HasValue)
                    return;
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var filePath in Directory.EnumerateFiles(path, "*.wad.client", searchOption)
                    .Where(filePath => Path.GetFileName(filePath).Count(character => character == '.') == 2))
                {
                    if (!TryLoadWad(filePath, out var wad))
                        continue;
                    await TryConvertWad(wad, mode.Value, outputDirectory);
                    wad.Dispose();
                }
            });
            return command;
        }

        private static async Task<bool> TryLoadHashes(string gameHashFile, string binHashesHashFile)
        {
            var latestHashes = await HashTables.TryLoadLatest(Logger);
            var userHashes = await HashTables.TryLoadFile(gameHashFile, HashTable.Game, Logger) && await HashTables.TryLoadFile(binHashesHashFile, HashTable.BinHashes, Logger);
            if (latestHashes || userHashes)
                return true;
            Logger.Fatal("No hash tables were loaded");
            return false;
        }

        private static bool TryCreateOutputDirectory(string outputDirectory)
        {
            try
            {
                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);
                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Output directory couldn't be created");
                return false;
            }
        }

        private static bool TryGetSkinMode(bool skeletons, bool animations, out SkinMode? mode)
        {
            try
            {
                mode = skeletons switch
                {
                    true when animations => SkinMode.WithAnimations,
                    false when animations => throw new ArgumentException("Animations can't be included without skeletons", nameof(skeletons)),
                    true => SkinMode.WithSkeleton,
                    _ => SkinMode.MeshAndTextures
                };
                return true;
            }
            catch (Exception e)
            {
                mode = null;
                Logger.Fatal(e, "Couldn't get SkinMode");
                return false;
            }
        }

        private static bool TryLoadWad(string filePath, out StringWad wad)
        {
            try
            {
                wad = new StringWad(filePath);
                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Couldn't open '{Path}'", filePath);
                wad = null;
                return false;
            }
        }

        private static async Task<bool> TryConvertWad(StringWad wad, SkinMode mode, string outputDirectory)
        {
            try
            {
                await foreach (var skin in wad.GetSkins(Logger))
                    await TryConvertSkin(skin, mode, outputDirectory);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't convert {Path}", wad.FilePath);
                return false;
            }
        }
        
        private static async Task<bool> TryConvertSkin(Skin skin, SkinMode mode, string outputDirectory)
        {
            try
            {
                Logger.Information("Converting {Character} skin{Id}", skin.Character, skin.Id);
                await skin.Load(mode, Logger);
                var skinDirectory = Path.Combine(outputDirectory, skin.Character);
                if (!Directory.Exists(skinDirectory))
                    Directory.CreateDirectory(skinDirectory);
                skin.Save(Path.Combine(skinDirectory, $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"),
                    Logger);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't convert {Character} skin{Id}", skin.Character, skin.Id);
                return false;
            }
        }

        /*private static async Task SamplerStuff(IDictionary<ulong, string> gameHashes)
        {
            var samplers = new Dictionary<string, IList<string>>();
            await foreach (var (sampler, texture) in Samplers.EnumerateSamplerNamesAsync(
                @"C:\Riot Games\League of Legends",
                gameHashes))
            {
                if (!samplers.ContainsKey(sampler))
                {
                    samplers[sampler] = new List<string> {texture};
                    continue;
                }

                if (!samplers[sampler].Contains(texture))
                    samplers[sampler].Add(texture);
            }

            var fileStream = File.Create(@"D:\out.json");
            await JsonSerializer.SerializeAsync(fileStream, samplers);
            await fileStream.DisposeAsync();
        }*/
    }
}
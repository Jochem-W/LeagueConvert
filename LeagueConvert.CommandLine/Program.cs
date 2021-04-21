using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using LeagueConvert.Enums;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.WadFile;
using Serilog;

namespace LeagueConvert.CommandLine
{
    internal static class Program
    {
        private static readonly ILogger Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();


        private static async Task<int> Main(string[] args)
        {
            try
            {
                await CheckForUpdates();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Update check failed");
            }

            var rootCommand = new RootCommand
            {
                GetConvertWadCommand()
            };
            var result = await rootCommand.InvokeAsync(args);
            Logger.Information("Finished");
            return result;
        }

        private static async Task CheckForUpdates()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var httpClient = new HttpClient();
            var latestVersion = new Version(await httpClient.GetStringAsync(
                $"https://api.jochemw.workers.dev/products/{assembly.Name?.ToLower()}/version/latest"));
            httpClient.Dispose();
            if (assembly.Version?.CompareTo(latestVersion) < 0)
                Logger.Information("A new version of {Name} is available: {Version}", assembly.Name, latestVersion);
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
                    if (!await HashTables.TryLoadLatest(Logger) ||
                        await HashTables.TryLoadFile(gameHashFile, HashTable.Game, Logger) &&
                        await HashTables.TryLoadFile(binHashesHashFile, HashTable.BinHashes, Logger))
                    {
                        Logger.Fatal("Couldn't load hash tables");
                        return;
                    }

                    StringWad wad;
                    try
                    {
                        wad = new StringWad(path);
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e, "Couldn't open '{Path}'", path);
                        return;
                    }

                    try
                    {
                        if (!Directory.Exists(outputDirectory))
                            Directory.CreateDirectory(outputDirectory);
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e, "Output directory couldn't be created");
                        return;
                    }

                    SkinMode mode;
                    try
                    {
                        mode = GetMode(skeletons, animations);
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e, "Can't include animations without skeletons");
                        return;
                    }

                    try
                    {
                        await foreach (var skin in wad.GetSkins(Logger))
                        {
                            Logger.Information("Converting {Character} skin{Id}", skin.Character, skin.Id);
                            await skin.Load(mode, Logger);
                            var skinDirectory = Path.Combine(outputDirectory, skin.Character);
                            if (!Directory.Exists(skinDirectory))
                                Directory.CreateDirectory(skinDirectory);
                            skin.Save(Path.Combine(skinDirectory, $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"),
                                Logger);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal(e, "Unexpected error occurred");
                    }
                });
            return command;
        }

        private static SkinMode GetMode(bool skeletons, bool animations)
        {
            return skeletons switch
            {
                true when animations => SkinMode.WithAnimations,
                false when animations => throw new ArgumentException("Animations can't be included without skeletons",
                    nameof(skeletons)),
                true => SkinMode.WithSkeleton,
                _ => SkinMode.MeshAndTextures
            };
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
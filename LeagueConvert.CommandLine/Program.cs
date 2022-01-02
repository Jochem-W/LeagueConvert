using System.CommandLine;
using System.Reflection;
using LeagueConvert.Enums;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.Skin;
using LeagueConvert.IO.Skin.Extensions;
using LeagueConvert.IO.WadFile;
using Serilog;
using SimpleGltf.Json.Extensions;

namespace LeagueConvert.CommandLine;

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
        var wadsArgument = new Argument<string[]>("wads", "Paths to WAD files")
        {
            Arity = ArgumentArity.OneOrMore
        };
        var outputOption = new Option<string>("-o", () => "output", "Path to an output directory");
        var skeletonsOption = new Option<bool>("-s", () => false, "Include skeletons");
        var animationsOption = new Option<bool>("-a", () => false, "Include animations");
        var gameHashOption = new Option<string>("-g", "Path to 'hashes.game.txt'");
        var binHashesHashOption = new Option<string>("-b", "Path to 'hashes.binhashes.txt'");

        var command = new Command("convert-wad", "Convert all models in specified WAD files")
        {
            wadsArgument,
            outputOption,
            skeletonsOption,
            animationsOption,
            gameHashOption,
            binHashesHashOption
        };

        command.SetHandler(async (string[] wads, string outputDirectory, bool skeletons, bool animations,
            string gameHashFile, string binHashesHashFile) =>
        {
            if (!await TryLoadHashes(gameHashFile, binHashesHashFile))
                return;
            if (!TryCreateOutputDirectory(outputDirectory))
                return;
            if (!TryGetSkinMode(skeletons, animations, out var mode) || !mode.HasValue)
                return;
            foreach (var path in wads)
            {
                Logger.Information("Converting {File}", Path.GetFileName(path));
                if (!TryLoadWad(path, out var wad))
                    return;
                await TryConvertWad(wad, mode.Value, outputDirectory);
                wad.Dispose();
            }

            Logger.Information("Finished!");
        }, wadsArgument, outputOption, skeletonsOption, animationsOption, gameHashOption, binHashesHashOption);

        return command;
    }

    private static Command GetConvertAllCommand()
    {
        var pathArgument = new Argument<string>("path", "Path to a folder containing WAD files");
        var outputOption = new Option<string>("-o", () => "output", "Path to an output directory");
        var skeletonsOption = new Option<bool>("-s", () => false, "Include skeletons");
        var animationsOption = new Option<bool>("-a", () => false, "Include animations");
        var recurseOption = new Option<bool>("-r", () => false, "Search for WAD files recursively");
        var gameHashOption = new Option<string>("-g", "Path to 'hashes.game.txt'");
        var binHashesHashOption = new Option<string>("-b", "Path to 'hashes.binhashes.txt'");

        var command = new Command("convert-all", "Convert all models in WAD files in a specified directory")
        {
            pathArgument,
            outputOption,
            skeletonsOption,
            animationsOption,
            recurseOption,
            gameHashOption,
            binHashesHashOption
        };

        command.SetHandler(async (string path, string outputDirectory, bool skeletons, bool animations, bool recurse,
                string gameHashFile, string binHashesHashFile) =>
            {
                if (!await TryLoadHashes(gameHashFile, binHashesHashFile))
                    return;
                if (!TryCreateOutputDirectory(outputDirectory))
                    return;
                if (!TryGetSkinMode(skeletons, animations, out var mode) || !mode.HasValue)
                    return;
                var searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var filePath in Directory.EnumerateFiles(path, "*.wad.client", searchOption)
                             .Where(filePath => Path.GetFileName(filePath).Count(character => character == '.') == 2))
                {
                    if (!TryLoadWad(filePath, out var wad))
                        continue;
                    await TryConvertWad(wad, mode.Value, outputDirectory);
                    wad.Dispose();
                }

                Logger.Information("Finished!");
            }, pathArgument, outputOption, skeletonsOption, animationsOption, recurseOption, gameHashOption,
            binHashesHashOption);

        return command;
    }

    private static async Task<bool> TryLoadHashes(string gameHashFile, string binHashesHashFile)
    {
        var latestHashes = await HashTables.TryLoadLatest(Logger);
        var userHashes = await HashTables.TryLoadFile(gameHashFile, HashTable.Game, Logger) &&
                         await HashTables.TryLoadFile(binHashesHashFile, HashTable.BinHashes, Logger);
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
                false when animations => throw new ArgumentException(
                    "Animations can't be included without skeletons", nameof(skeletons)),
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

            await using var gltfAsset = await skin.GetGltfAsset(Logger);
            await gltfAsset.Save(Path.Combine(skinDirectory, $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"));
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Couldn't convert {Character} skin{Id}", skin.Character, skin.Id);
            return false;
        }
    }
}
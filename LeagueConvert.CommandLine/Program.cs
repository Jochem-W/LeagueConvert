using System.CommandLine;
using System.Reflection;
using LeagueConvert.Enums;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.Skin;
using LeagueConvert.IO.Skin.Extensions;
using LeagueConvert.IO.WadFile;
using Octokit;
using Octokit.Extensions.Models;
using Serilog;
using SimpleGltf.Json.Extensions;

namespace LeagueConvert.CommandLine;

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
            Logger.Error(e, "An error occurred while checking for updates");
        }

        var rootCommand = new RootCommand
        {
            GetConvertWadCommand(),
            GetConvertAllCommand()
        };
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task CheckForUpdates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (informationalVersion == null)
        {
            Logger.Error("Update check failed: could not determine current version");
            return;
        }

        var split = informationalVersion.InformationalVersion.Split('+');
        if (split.Length == 1) return;

        if (split.Length != 5)
        {
            Logger.Error("Update check failed: invalid version format {Version}",
                informationalVersion.InformationalVersion);
            return;
        }

        var currentVersion = new Version(split[0]);
        var repository = split[1].Split('/');
        var owner = repository[0];
        var name = repository[1];
        var eventName = split[2];
        var reference = split[3];
        var commitSha = split[4];

        var gitHubClient = new GitHubClient(new ProductHeaderValue(name, split[0]));

        Uri uri;
        switch (eventName)
        {
            case "push":
                uri = await CheckForNewRelease(gitHubClient, owner, name, currentVersion);
                if (uri == null) uri = await CheckForNewBuild(gitHubClient, owner, name, reference, commitSha);
                break;
            case "release":
                uri = await CheckForNewRelease(gitHubClient, owner, name, currentVersion);
                break;
            default:
                uri = null;
                break;
        }

        if (uri != null) Logger.Information("Download link: {Uri}", uri);
    }

    private static async Task<Uri> CheckForNewRelease(IGitHubClient gitHubClient, string owner, string name,
        Version currentVersion)
    {
        var latestRelease = await gitHubClient.Repository.Release.GetLatest(owner, name);
        var latestVersion = new Version(latestRelease.TagName.Remove(0, 1));
        if (latestVersion <= currentVersion) return null;

        Logger.Information("A new version is available: {Version}", latestVersion);
        return new Uri(latestRelease.HtmlUrl);
    }

    private static async Task<Uri> CheckForNewBuild(IGitHubClient gitHubClient, string owner, string name,
        string branch, string commitSha)
    {
        var apiResponse = await gitHubClient.Connection.Get<ActionRunsResponse>(
            new Uri(
                $"https://api.github.com/repos/{owner}/{name}/actions/runs?branch={branch}&event=push&status=success&per_page=1"),
            TimeSpan.FromSeconds(5));

        if (apiResponse?.Body == null || apiResponse.Body.TotalCount == 0)
        {
            Logger.Error("Error while checking for updates: no builds found");
            return null;
        }

        var run = apiResponse.Body.WorkflowRuns[0];
        if (run.HeadSha == commitSha) return null;

        Logger.Information("A new build is available: {Commit}", run.HeadSha);
        return new Uri(run.HtmlUrl);
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
        var forceScaleOption = new Option<bool>("--force-scale", () => false,
            "Flip the X-axis even if it's not allowed by the glTF specification");
        var keepHiddenSubMeshesOption = new Option<bool>("-k", () => false, "Keep hidden sub meshes");


        var command = new Command("convert-wad", "Convert all models in specified WAD files")
        {
            wadsArgument,
            outputOption,
            skeletonsOption,
            animationsOption,
            gameHashOption,
            binHashesHashOption,
            forceScaleOption,
            keepHiddenSubMeshesOption
        };

        command.SetHandler(async (string[] wads, string outputDirectory, bool skeletons, bool animations,
                string gameHashFile, string binHashesHashFile, bool forceScale, bool keepHiddenSubMeshes) =>
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
                    await TryConvertWad(wad, mode.Value, outputDirectory, forceScale, keepHiddenSubMeshes);
                    wad.Dispose();
                }

                Logger.Information("Finished!");
            }, wadsArgument, outputOption, skeletonsOption, animationsOption, gameHashOption, binHashesHashOption,
            forceScaleOption, keepHiddenSubMeshesOption);

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
        var forceScaleOption = new Option<bool>("--force-scale", () => false,
            "Flip the X-axis even if it's not allowed by the glTF specification");
        var keepHiddenSubMeshesOption = new Option<bool>("-k", () => false, "Keep hidden sub meshes");

        var command = new Command("convert-all", "Convert all models in WAD files in a specified directory")
        {
            pathArgument,
            outputOption,
            skeletonsOption,
            animationsOption,
            recurseOption,
            gameHashOption,
            binHashesHashOption,
            forceScaleOption,
            keepHiddenSubMeshesOption
        };

        command.SetHandler(async context =>
        {
            if (!await TryLoadHashes(context.ParseResult.GetValueForOption(gameHashOption),
                    context.ParseResult.GetValueForOption(binHashesHashOption)))
                return;
            if (!TryCreateOutputDirectory(context.ParseResult.GetValueForOption(outputOption)))
                return;
            if (!TryGetSkinMode(context.ParseResult.GetValueForOption(skeletonsOption),
                    context.ParseResult.GetValueForOption(animationsOption), out var mode) || !mode.HasValue)
                return;
            var searchOption = context.ParseResult.GetValueForOption(recurseOption)
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;
            foreach (var filePath in Directory
                         .EnumerateFiles(context.ParseResult.GetValueForArgument(pathArgument), "*.wad.client",
                             searchOption).Where(filePath =>
                             Path.GetFileName(filePath).Count(character => character == '.') == 2))
            {
                if (!TryLoadWad(filePath, out var wad))
                    continue;
                await TryConvertWad(wad, mode.Value, context.ParseResult.GetValueForOption(outputOption),
                    context.ParseResult.GetValueForOption(forceScaleOption),
                    context.ParseResult.GetValueForOption(keepHiddenSubMeshesOption));
                wad.Dispose();
            }

            Logger.Information("Finished!");
        });

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

    private static async Task<bool> TryConvertWad(StringWad wad, SkinMode mode, string outputDirectory, bool forceScale,
        bool keepHiddenSubMeshes)
    {
        try
        {
            await foreach (var skin in wad.GetSkins(Logger))
                await TryConvertSkin(skin, mode, outputDirectory, forceScale, keepHiddenSubMeshes);
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Couldn't convert {Path}", wad.FilePath);
            return false;
        }
    }

    private static async Task<bool> TryConvertSkin(Skin skin, SkinMode mode, string outputDirectory, bool forceScale,
        bool keepHiddenSubMeshes)
    {
        try
        {
            Logger.Information("Converting {Character} skin{Id}", skin.Character, skin.Id);
            await skin.Load(mode, Logger);
            var skinDirectory = Path.Combine(outputDirectory, skin.Character);
            if (!Directory.Exists(skinDirectory))
                Directory.CreateDirectory(skinDirectory);
            await using var gltfAsset = await skin.GetGltfAsset(forceScale, keepHiddenSubMeshes, Logger);
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
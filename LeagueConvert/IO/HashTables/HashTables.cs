using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LeagueConvert.Enums;
using Octokit;
using Serilog;

namespace LeagueConvert.IO.HashTables
{
    public static class HashTables
    {
        private static GitHubClient _gitHubClient;
        private static HttpClient _httpClient;

        public static IDictionary<ulong, string> Game { get; private set; }

        public static IDictionary<uint, string> BinHashes { get; private set; }

        public static async Task<bool> TryLoadFile(string filePath, HashTable hashTable, ILogger logger = null)
        {
            if (!File.Exists(filePath))
                return false;
            try
            {
                switch (hashTable)
                {
                    case HashTable.Game:
                        logger?.Information("Loading {Path}", filePath);
                        LoadGame(GetUlongHashPairs(await File.ReadAllTextAsync(filePath)), logger);
                        break;
                    case HashTable.BinHashes:
                        logger?.Information("Loading {Path}", filePath);
                        LoadBinHashes(GetUintHashPairs(await File.ReadAllTextAsync(filePath)), logger);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(hashTable), hashTable, "Invalid hash table");
                }
                
                return true;
            }
            catch (Exception e)
            {
                logger?.Warning(e, "Couldn't load {Path}", filePath);
                return false;
            }
        }

        public static async Task<bool> TryLoadLatest(ILogger logger = null)
        {
            logger?.Information("Loading latest hash tables");
            var path = Path.Combine(Path.GetTempPath(), "LeagueConvert");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (await TryLoadFromGitHub(path, logger))
                return true;
            logger?.Warning("Trying to load (possibly outdated) existing hash tables");
            return await TryLoadExisting(path, logger);
        }

        private static async Task<bool> TryLoadFromGitHub(string path, ILogger logger = null)
        {
            _gitHubClient ??= new GitHubClient(new ProductHeaderValue("LeagueBulkConvert"));
            _httpClient ??= new HttpClient();
            IReadOnlyList<RepositoryContent> contents;
            try
            {
                contents = await _gitHubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB",
                    "cdragontoolbox");
            }
            catch (Exception e)
            {
                logger?.Error(e, "Couldn't load hash tables from GitHub");
                return false;
            }

            foreach (var repositoryContent in contents)
                switch (repositoryContent.Name)
                {
                    case "hashes.game.txt":
                        var content = await Download(repositoryContent, path, logger);
                        logger?.Information("Loading downloaded {FileName}", repositoryContent.Name);
                        LoadGame(GetUlongHashPairs(content), logger);
                        break;
                    case "hashes.binhashes.txt":
                        content = await Download(repositoryContent, path, logger);
                        logger?.Information("Loading downloaded {FileName}", repositoryContent.Name);
                        LoadBinHashes(GetUintHashPairs(content), logger);
                        break;
                }

            return true;
        }

        private static async Task<bool> TryLoadExisting(string path, ILogger logger = null)
        {
            var gameHashFile = Path.Combine(path, "hashes.game.txt");
            var binHashesFile = Path.Combine(path, "hashes.binhashes.txt");
            if (await TryLoadFile(gameHashFile, HashTable.Game, logger) &&
                await TryLoadFile(binHashesFile, HashTable.BinHashes, logger)) return true;
            logger?.Error("Couldn't load existing hash tables");
            return false;
        }

        private static async Task<string> Download(RepositoryContentInfo content, string directory,
            ILogger logger = null)
        {
            var filePath = Path.Combine(directory, $"{content.Name}");
            var shaFilePath = $"{filePath}.sha";
            if (File.Exists(shaFilePath) && await File.ReadAllTextAsync(shaFilePath) == content.Sha &&
                File.Exists(filePath))
                return await File.ReadAllTextAsync(filePath);
            var tmpFilePath = $"{filePath}.tmp";
            logger?.Information("Downloading {FileName}", content.Name);
            var contents = await _httpClient.GetStringAsync(content.DownloadUrl);
            await File.WriteAllTextAsync(tmpFilePath, contents);
            File.Move(tmpFilePath, filePath, true);
            await File.WriteAllTextAsync(shaFilePath, content.Sha);
            return contents;
        }

        private static IEnumerable<KeyValuePair<ulong, string>> GetUlongHashPairs(string content)
        {
            return content
                .Split('\n')
                .Select(line => line.Split(' '))
                .Where(split => split.Length == 2)
                .Select(split => new KeyValuePair<ulong, string>(Convert.ToUInt64(split[0], 16), split[1]));
        }

        private static IEnumerable<KeyValuePair<uint, string>> GetUintHashPairs(string content)
        {
            return content
                .Split('\n')
                .Select(line => line.Split(' '))
                .Where(split => split.Length == 2)
                .Select(split => new KeyValuePair<uint, string>(Convert.ToUInt32(split[0], 16), split[1]));
        }

        private static void LoadGame(IEnumerable<KeyValuePair<ulong, string>> hashPairs, ILogger logger = null)
        {
            Game ??= new Dictionary<ulong, string>();
            foreach (var (hash, value) in hashPairs)
                TryLoadHash(hash, value, Game, logger);
        }

        private static void LoadBinHashes(IEnumerable<KeyValuePair<uint, string>> hashPairs, ILogger logger = null)
        {
            BinHashes ??= new Dictionary<uint, string>();
            foreach (var (hash, value) in hashPairs)
                TryLoadHash(hash, value, BinHashes, logger);
        }

        private static void TryLoadHash(uint hash, string value, IDictionary<uint, string> hashTable,
            ILogger logger = null)
        {
            try
            {
                hashTable[hash] = value;
            }
            catch (Exception e)
            {
                logger?.Warning(e, "Couldn't set hash {Hash} to {Value}", hash, value);
            }
        }

        private static void TryLoadHash(ulong hash, string value, IDictionary<ulong, string> hashTable,
            ILogger logger = null)
        {
            try
            {
                hashTable[hash] = value;
            }
            catch (Exception e)
            {
                logger?.Warning(e, "Couldn't set hash {Hash} to {Value}", hash, value);
            }
        }
    }
}
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

        public static async Task LoadFile(string filePath, HashTable hashTable, ILogger logger = null)
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
        }

        public static async Task LoadLatest(ILogger logger = null)
        {
            _gitHubClient ??= new GitHubClient(new ProductHeaderValue("LeagueBulkConvert"));
            _httpClient ??= new HttpClient();
            var path = Path.Combine(Path.GetTempPath(), "LeagueConvert");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var repositoryContent in await _gitHubClient.Repository.Content.GetAllContents("CommunityDragon",
                "CDTB",
                "cdragontoolbox"))
            {
                string content;
                switch (repositoryContent.Name)
                {
                    case "hashes.game.txt":
                        content = await GetLatest(repositoryContent, path, logger);
                        logger?.Information("Loading downloaded {FileName}", repositoryContent.Name);
                        LoadGame(GetUlongHashPairs(content), logger);
                        break;
                    case "hashes.binhashes.txt":
                        content = await GetLatest(repositoryContent, path, logger);
                        logger?.Information("Loading downloaded {FileName}", repositoryContent.Name);
                        LoadBinHashes(GetUintHashPairs(content), logger);
                        break;
                }
            }
        }

        private static async Task<string> GetLatest(RepositoryContentInfo content, string directory,
            ILogger logger = null)
        {
            var filePath = Path.Combine(directory, $"{content.Name}");
            var shaFilePath = $"{filePath}.sha";
            if (File.Exists(shaFilePath) && await File.ReadAllTextAsync(shaFilePath) == content.Sha)
                return await File.ReadAllTextAsync(filePath);
            var tmpFilePath = $"{filePath}.tmp";
            try
            {
                var contents = await _httpClient.GetStringAsync(content.Url);
                await File.WriteAllTextAsync(tmpFilePath, contents);
                File.Move(tmpFilePath, filePath);
                await File.WriteAllTextAsync(shaFilePath, content.Sha);
                return contents;
            }
            catch (Exception e)
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException();
                logger?.Warning(e, "Couldn't update {FileName}, using possibly outdated version", content.Name);
                return await File.ReadAllTextAsync(filePath);
            }
        }

        private static IEnumerable<KeyValuePair<ulong, string>> GetUlongHashPairs(string content)
        {
            return content.Split('\n').Select(line =>
            {
                var split = line.Split(' ');
                return new KeyValuePair<ulong, string>(Convert.ToUInt64(split[0]), split[1]);
            });
        }

        private static IEnumerable<KeyValuePair<uint, string>> GetUintHashPairs(string content)
        {
            return content.Split('\n').Select(line =>
            {
                var split = line.Split(' ');
                return new KeyValuePair<uint, string>(Convert.ToUInt32(split[0]), split[1]);
            });
        }

        private static void LoadGame(IEnumerable<KeyValuePair<ulong, string>> lines, ILogger logger = null)
        {
            Game ??= new Dictionary<ulong, string>();
            foreach (var (hash, value) in lines)
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
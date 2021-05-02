using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit;
using Serilog;

namespace LeagueBulkConvert
{
    public static class HashTables
    {
        private static readonly GitHubClient GitHubClient = new(new ProductHeaderValue("LeagueBulkConvert"));
        private static readonly HttpClient HttpClient = new();

        public static async Task<bool> TryLoad(ILogger logger = null)
        {
            try
            {
                logger?.Information("Downloading latest hashtables");
                var repositoryContents = await GitHubClient.Repository.Content.GetAllContents("CommunityDragon",
                    "CDTB",
                    "cdragontoolbox");
                foreach (var file in repositoryContents.Where(f =>
                    f.Name is "hashes.binhashes.txt" or "hashes.game.txt"))
                {
                    var filePath = $"hashes/{file.Name}";
                    var shaFilePath = $"{filePath}.sha";
                    if (File.Exists(filePath) && File.Exists(shaFilePath) &&
                        await File.ReadAllTextAsync(shaFilePath) == file.Sha) continue;
                    var tempFilePath = $"{filePath}.tmp";
                    await File.WriteAllTextAsync(tempFilePath,
                        await HttpClient.GetStringAsync(file.DownloadUrl));
                    File.Move(tempFilePath, filePath);
                    await File.WriteAllTextAsync(shaFilePath, file.Sha);
                }

                return true;
            }
            catch (Exception e)
            {
                if (File.Exists("hashes/hashes.binhashes.txt") && File.Exists("hashes/hashes.game.txt"))
                {
                    logger?.Error(e, "Couldn't update hashtables, using current version");
                    return true;
                }

                logger?.Fatal(e, "Couldn't download hashtables, cancelling!");
                return false;
            }
        }
    }
}
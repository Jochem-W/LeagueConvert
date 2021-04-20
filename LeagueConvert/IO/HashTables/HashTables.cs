using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LeagueConvert.IO.HashTables
{
    public static class HashTables
    {
        public static IDictionary<ulong, string> Game { get; private set; }

        public static IDictionary<uint, string> BinHashes { get; private set; }

        public static Task Load(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName == null)
                return null;
            var split = fileName.Split('.');
            return split[1] switch
            {
                "game" => LoadGame(filePath),
                "binhashes" => LoadBinHashes(filePath),
                _ => null
            };
        }

        private static async Task LoadGame(string filePath)
        {
            Game = new Dictionary<ulong, string>();
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                var split = line.Split(' ');
                Game[Convert.ToUInt64(split[0], 16)] = split[1];
            }
        }

        private static async Task LoadBinHashes(string filePath)
        {
            BinHashes = new Dictionary<uint, string>();
            foreach (var line in await File.ReadAllLinesAsync(filePath))
            {
                var split = line.Split(' ');
                BinHashes[Convert.ToUInt32(split[0], 16)] = split[1];
            }
        }
    }
}
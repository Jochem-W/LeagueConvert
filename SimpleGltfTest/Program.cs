using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LeagueConvert.Enums;
using LeagueConvert.IO.HashTables;
using LeagueConvert.IO.Skin.Extensions;
using LeagueConvert.IO.WadFile;
using SimpleGltf.Json.Extensions;

namespace SimpleGltfTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await HashTables.TryLoadLatest();
            var timeSpanList = new List<TimeSpan>();
            var stopwatch = new Stopwatch();
            for (var i = 0; i < 1; i++)
            {
                stopwatch.Reset();
                stopwatch.Start();
                await AllWads(@"C:\Riot Games\League of Legends", @"D:\models\skn", SkinMode.MeshAndTextures);
                await AllWads(@"C:\Riot Games\League of Legends", @"D:\models\skl", SkinMode.WithSkeleton);
                await AllWads(@"C:\Riot Games\League of Legends", @"D:\models\anm", SkinMode.WithAnimations);
                stopwatch.Stop();
                timeSpanList.Add(stopwatch.Elapsed);
            }

            var average = timeSpanList.Aggregate(TimeSpan.Zero, (current, timeSpan) => current + timeSpan) /
                          timeSpanList.Count;
            Console.WriteLine($"Time elapsed: {average.TotalMilliseconds} ms");
        }

        private static async Task AllWads(string path, string output, SkinMode mode)
        {
            if (Directory.Exists(output))
                Directory.Delete(output);
            foreach (var file in Directory.EnumerateFiles(path, "*.wad.client",
                SearchOption.AllDirectories))
                await SingleWad(file, output, mode);
        }

        private static async Task SingleWad(string path, string output, SkinMode mode)
        {
            using var wad = new StringWad(path, true);
            await Convert(wad, output, mode);
        }

        private static async Task Convert(StringWad wad, string output, SkinMode mode)
        {
            await foreach (var skin in wad.GetSkins())
            {
                await skin.Load(mode);
                await using var gltfAsset = await skin.GetGltfAsset();
                if (gltfAsset == null)
                    continue;
                await gltfAsset.Save(Path.Combine(output, skin.Character,
                    $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"));
            }
        }
    }
}
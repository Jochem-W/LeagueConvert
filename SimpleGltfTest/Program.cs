using System.IO;
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
        private const string Output = @"C:\Users\Joche\Downloads\models";

        private static async Task Main(string[] args)
        {
            await HashTables.TryLoadLatest();
            if (Directory.Exists(Output))
                Directory.Delete(Output, true);
            //await AllWads(@"C:\Riot Games\League of Legends");
            await SingleWad(@"C:\Riot Games\League of Legends\Game\DATA\FINAL\Champions\Aatrox.wad.client");
        }

        private static async Task AllWads(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.wad.client",
                SearchOption.AllDirectories))
                await SingleWad(file);
        }

        private static async Task SingleWad(string path)
        {
            using var wad = new StringWad(path, true);
            await Convert(wad);
        }

        private static async Task Convert(StringWad wad)
        {
            await foreach (var skin in wad.GetSkins())
            {
                await skin.Load(SkinMode.MeshAndTextures);
                await using var gltfAsset = await skin.GetGltfAsset();
                if (gltfAsset == null)
                    continue;
                await gltfAsset.Save(Path.Combine(Output, skin.Character,
                    $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"));
            }
        }
    }
}
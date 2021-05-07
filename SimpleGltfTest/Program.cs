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
        private static async Task Main(string[] args)
        {
            await HashTables.TryLoadLatest();
            if (Directory.Exists(@"C:\Users\Joche\Downloads\models"))
                Directory.Delete(@"C:\Users\Joche\Downloads\models", true);
            using var wad =
                new StringWad(@"C:\Riot Games\League of Legends\Game\DATA\FINAL\Champions\Aatrox.wad.client");
            await Test(wad);
            /*foreach (var file in Directory.EnumerateFiles(@"C:\Riot Games\League of Legends", "*.wad.client",
                SearchOption.AllDirectories))
            {
                using var wad = new StringWad(file);
                await Test(wad);
            }*/
        }

        private static async Task Test(StringWad wad)
        {
            await foreach (var skin in wad.GetSkins())
            {
                await skin.Load(SkinMode.MeshAndTextures);
                await using var gltfAsset = await skin.GetGltfAsset();
                if (gltfAsset == null)
                    continue;
                await gltfAsset.Save(Path.Combine(@"C:\Users\Joche\Downloads\models", skin.Character,
                    $"skin{skin.Id.ToString().PadLeft(2, '0')}.glb"));
            }
        }
    }
}
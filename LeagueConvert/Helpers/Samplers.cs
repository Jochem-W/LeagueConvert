using System.Collections.Generic;

namespace LeagueConvert.Helpers
{
    public static class Samplers
    {
        public static IEnumerable<string> Diffuse { get; } = new[]
        {
            "Diffuse_Texture",
            "DiffuseTexture",
            "Diffuse_Color",
            "Diffuse_Texture_Primary",
            "Main_Texture",
            "Diff_Tex",
            "Diffuse_Sword_Texture",
            "Color_Texture",
            "Diffuse"
        };

        /*public static async IAsyncEnumerable<KeyValuePair<string, string>> EnumerateSamplerNamesAsync(string leaguePath,
            IDictionary<ulong, string> gameHashes)
        {
            foreach (var wadFile in Directory.EnumerateFiles(leaguePath, "*.wad.client", SearchOption.AllDirectories))
            {
                var wad = new StringWad(wadFile, gameHashes);
                foreach (var (_, entry) in wad.Entries.Where(pair => pair.Key.EndsWith(".bin")))
                {
                    var stream = await entry.GetStream().Version3ToVersion2();
                    BinTree binTree;
                    try
                    {
                        binTree = new BinTree(stream);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    finally
                    {
                        await stream.DisposeAsync();
                    }

                    foreach (var staticMaterialDef in binTree.Objects.Where(o => o.MetaClassHash == 4288492553))
                    foreach (var samplerValue in staticMaterialDef.Properties.Where(p => p.NameHash == 175050421)
                        .Cast<BinTreeContainer>())
                    foreach (var staticMaterialShaderSamplerDef in samplerValue.Properties
                        .Cast<BinTreeEmbedded>().Where(p => p.MetaClassHash == 151302480))
                    {
                        var samplerName =
                            (BinTreeString) staticMaterialShaderSamplerDef.Properties.FirstOrDefault(p =>
                                p.NameHash == 48757580);
                        var textureName =
                            (BinTreeString) staticMaterialShaderSamplerDef.Properties.FirstOrDefault(p =>
                                p.NameHash == 3004290287);
                        if (samplerName == null || textureName == null)
                            continue;
                        yield return new KeyValuePair<string, string>(samplerName.Value, textureName.Value);
                    }
                }

                wad.Dispose();
            }
        }*/
    }
}
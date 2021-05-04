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
    }
}
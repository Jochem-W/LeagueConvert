using LeagueConvert.Enums;

namespace LeagueConvert.Helpers;

public static class Samplers
{
    private static readonly List<string> Unsupported = new();

    private static readonly string[] Diffuse =
    {
        // "Alt_Diffuse_Texture",
        // "Alt_Diffuse",
        // "Base_Texture",
        "Color_Texture",
        "Diff_Tex",
        "Diffuse_Color",
        "Diffuse_Sword_Texture",
        // "Diffuse_Texture_2",
        "Diffuse_Texture_Primary",
        // "Diffuse_Texture_Ult",
        "Diffuse_Texture",
        "Diffuse",
        "DiffuseTexture",
        "Main_Texture"
        // "Secondary_Texture",
        // "ShadowTexture",
        // "Swapped_Texture",
        // "WP_Base_Texture"
    };

    public static SamplerType FromString(string samplerType)
    {
        if (Diffuse.Contains(samplerType)) return SamplerType.Diffuse;
        return SamplerType.Unknown;
    }
}
namespace SimpleGltf.Json.Extensions;

public static class PbrMetallicRoughnessExtensions
{
    public static void SetBaseColorTexture(this MaterialPbrMetallicRoughness materialPbrMetallicRoughness,
        Texture texture)
    {
        materialPbrMetallicRoughness.BaseColorTexture = new TextureInfo(texture);
    }
}
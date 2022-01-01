namespace SimpleGltf.Json.Extensions;

public static class MaterialExtensions
{
    public static PbrMetallicRoughness CreatePbrMetallicRoughness(this Material material)
    {
        var pbrMetallicRoughness = new PbrMetallicRoughness();
        material.PbrMetallicRoughness = pbrMetallicRoughness;
        return pbrMetallicRoughness;
    }
}
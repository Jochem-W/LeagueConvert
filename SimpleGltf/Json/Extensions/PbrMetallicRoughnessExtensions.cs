using System.Numerics;

namespace SimpleGltf.Json.Extensions
{
    public static class PbrMetallicRoughnessExtensions
    {
        public static void SetBaseColorTexture(this PbrMetallicRoughness pbrMetallicRoughness, Texture texture)
        {
            pbrMetallicRoughness.BaseColorTexture = new TextureInfo(texture);
        }
    }
}
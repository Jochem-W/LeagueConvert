using System.Numerics;

namespace SimpleGltf.Json.Extensions
{
    public static class PbrMetallicRoughnessExtensions
    {
        public static void SetBaseColorTexture(this PbrMetallicRoughness pbrMetallicRoughness, Texture texture,
            Vector4? baseColorFactor = null, int texCoord = 0)
        {
            pbrMetallicRoughness.BaseColorFactor = baseColorFactor;
            pbrMetallicRoughness.BaseColorTexture = new TextureInfo(texture, texCoord);
        }
    }
}
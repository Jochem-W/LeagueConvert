namespace SimpleGltf.Json.Extensions
{
    public static class MaterialExtensions
    {
        public static PbrMetallicRoughness CreatePbrMetallicRoughness(this Material material)
        {
            var pbrMetallicRoughness = new PbrMetallicRoughness(null, null, null, null, null);
            material.PbrMetallicRoughness = pbrMetallicRoughness;
            return pbrMetallicRoughness;
        }
    }
}
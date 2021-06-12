using System.Collections;
using System.Collections.Generic;

namespace SimpleGltf.Json.Extensions
{
    public static class MaterialExtensions
    {
        private const string Unlit = "KHR_materials_unlit";
        
        public static PbrMetallicRoughness CreatePbrMetallicRoughness(this Material material)
        {
            var pbrMetallicRoughness = new PbrMetallicRoughness();
            material.PbrMetallicRoughness = pbrMetallicRoughness;
            return pbrMetallicRoughness;
        }

        public static void SetUnlit(this Material material)
        {
            material.Extensions ??= new Dictionary<string, IDictionary>();
            material.Extensions[Unlit] = new Dictionary<string, int>();
            material.GltfAsset.ExtensionsUsed ??= new List<string>();
            if (!material.GltfAsset.ExtensionsUsed.Contains(Unlit))
                material.GltfAsset.ExtensionsUsed.Add(Unlit);
        }
    }
}
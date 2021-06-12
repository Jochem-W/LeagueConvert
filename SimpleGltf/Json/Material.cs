using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Material : IIndexable
    {
        internal Material(GltfAsset gltfAsset)
        {
            gltfAsset.Materials ??= new List<Material>();
            Index = gltfAsset.Materials.Count;
            gltfAsset.Materials.Add(this);
        }
        
        [JsonIgnore] public int Index { get; }

        public PbrMetallicRoughness PbrMetallicRoughness { get; internal set; }

        public string Name { get; init; }
    }
}
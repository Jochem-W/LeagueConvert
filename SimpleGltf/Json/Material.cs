using System.Collections.Generic;
using System.Text.Json.Serialization;

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

        public PbrMetallicRoughness PbrMetallicRoughness { get; internal set; }

        public string Name { get; init; }

        [JsonIgnore] public int Index { get; }
    }
}
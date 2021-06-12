using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Material : IIndexable
    {
        internal GltfAsset GltfAsset;
        
        internal Material(GltfAsset gltfAsset)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Materials ??= new List<Material>();
            Index = GltfAsset.Materials.Count;
            GltfAsset.Materials.Add(this);
        }

        public PbrMetallicRoughness PbrMetallicRoughness { get; internal set; }

        public string Name { get; init; }
        
        public IDictionary<string, IDictionary> Extensions { get; internal set; }

        [JsonIgnore] public int Index { get; }
    }
}
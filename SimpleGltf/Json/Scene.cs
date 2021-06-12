using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Scene : IIndexable
    {
        internal Scene(GltfAsset gltfAsset)
        {
            gltfAsset.Scenes ??= new List<Scene>();
            Index = gltfAsset.Scenes.Count;
            gltfAsset.Scenes.Add(this);
        }

        [JsonConverter(typeof(EnumerableIndexableConverter<Node>))]
        public IList<Node> Nodes { get; set; }

        [JsonIgnore] public int Index { get; }
    }
}
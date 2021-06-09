using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Scene
    {
        internal readonly GltfAsset GltfAsset;

        internal Scene(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Scenes ??= new List<Scene>();
            GltfAsset.Scenes.Add(this);
            Name = name;
        }

        [JsonConverter(typeof(NodeListConverter))]
        public IList<Node> Nodes { get; set; }

        public string Name { get; }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Scene
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        internal Scene(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Scenes ??= new List<Scene>();
            Index = GltfAsset.Scenes.Count;
            GltfAsset.Scenes.Add(this);
            Name = name;
        }
        
        [JsonIgnore] public IList<Node> Nodes { get; set; }

        [JsonPropertyName("nodes")] public IEnumerable<int> NodesIndices => Nodes.Select(node => node.Index);

        public string Name { get; }
    }
}
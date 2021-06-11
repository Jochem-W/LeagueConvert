using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Scene
    {
        internal readonly int Index;

        internal Scene(GltfAsset gltfAsset, string name)
        {
            gltfAsset.Scenes ??= new List<Scene>();
            Index = gltfAsset.Scenes.Count;
            gltfAsset.Scenes.Add(this);
            Name = name;
        }

        [JsonIgnore] public IList<Node> Nodes { get; set; }

        [JsonPropertyName("nodes")] public IEnumerable<int> NodesIndices => Nodes.Select(node => node.Index);

        public string Name { get; }
    }
}
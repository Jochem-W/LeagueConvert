using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Scene
    {
        private readonly GltfAsset _gltfAsset;

        internal Scene(GltfAsset gltfAsset, string name)
        {
            _gltfAsset = gltfAsset;
            _gltfAsset.Scenes ??= new List<Scene>();
            _gltfAsset.Scenes.Add(this);
            Name = name;
        }

        [JsonIgnore] public IList<Node> Nodes { get; set; }

        [JsonPropertyName("nodes")]
        public IEnumerable<int> NodeReferences => Nodes?.Select(node => _gltfAsset.Nodes.IndexOf(node));

        public string Name { get; }
    }
}
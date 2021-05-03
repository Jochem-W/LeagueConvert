using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    internal class Scene
    {
        internal readonly GltfAsset GltfAsset;
        internal IList<Node> Nodes;

        internal Scene(GltfAsset gltfAsset, string name, bool setDefault)
        {
            GltfAsset = gltfAsset;
            Name = name;
            if (setDefault)
                GltfAsset.Scene = this;
            GltfAsset.Scenes ??= new List<Scene>();
            GltfAsset.Scenes.Add(this);
        }

        [JsonPropertyName("nodes")]
        public IEnumerable<int> NodeReferences => Nodes?.Select(node => GltfAsset.Nodes.IndexOf(node));

        public string Name { get; }
    }
}
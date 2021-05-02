using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    internal class Scene
    {
        internal readonly GltfAsset GltfAsset;
        internal IList<Node> Nodes;

        internal Scene(GltfAsset gltfAsset, bool setDefault)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Scenes ??= new List<Scene>();
            GltfAsset.Scenes.Add(this);
            if (setDefault)
                GltfAsset.Scene = this;
        }

        [JsonPropertyName("nodes")]
        public IEnumerable<int> NodeReferences => Nodes?.Select(node => GltfAsset.Nodes.IndexOf(node));
    }
}
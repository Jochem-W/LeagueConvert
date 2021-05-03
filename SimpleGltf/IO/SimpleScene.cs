using System.Collections.Generic;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.IO
{
    public class SimpleScene
    {
        private readonly IList<SimpleNode> _nodes;
        internal readonly Scene Scene;

        internal readonly SimpleGltfAsset SimpleGltfAsset;

        internal SimpleScene(SimpleGltfAsset simpleGltfAsset, string name = null, bool setDefault = true)
        {
            SimpleGltfAsset = simpleGltfAsset;
            _nodes = new List<SimpleNode>();
            Scene = SimpleGltfAsset.GltfAsset.CreateScene(name, setDefault);
        }

        public IEnumerable<SimpleNode> Nodes => _nodes;

        public SimpleNode CreateNode(string name = null)
        {
            var node = new SimpleNode(this, name);
            _nodes.Add(node);
            return node;
        }
    }
}
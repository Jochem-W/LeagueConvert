using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleScene
    {
        private readonly IList<SimpleNode> _nodes;
        internal readonly Scene Scene;

        internal SimpleScene(GltfAsset gltfAsset, bool setDefault = true)
        {
            _nodes = new List<SimpleNode>();
            Scene = new Scene(gltfAsset, setDefault);
        }

        public IEnumerable<SimpleNode> Nodes => _nodes;

        public SimpleNode CreateNode(string name = null)
        {
            var node = new SimpleNode(Scene, name);
            _nodes.Add(node);
            return node;
        }
    }
}
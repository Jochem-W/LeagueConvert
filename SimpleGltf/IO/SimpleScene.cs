using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleScene
    {
        private readonly IList<SimpleNode> _nodes;
        private readonly SimpleGltfAsset _simpleGltfAsset;

        internal readonly Scene Scene;

        internal SimpleScene(SimpleGltfAsset simpleGltfAsset, bool setDefault = true)
        {
            _simpleGltfAsset = simpleGltfAsset;
            _nodes = new List<SimpleNode>();
            Scene = new Scene(simpleGltfAsset.GltfAsset, setDefault);
        }

        public IEnumerable<SimpleNode> Nodes => _nodes;

        public SimpleNode CreateNode(string name = null)
        {
            var node = new SimpleNode(_simpleGltfAsset, Scene, name);
            _nodes.Add(node);
            return node;
        }
    }
}
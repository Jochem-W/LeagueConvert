using System;
using System.Collections.Generic;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.IO
{
    public class SimpleNode
    {
        private readonly IList<SimpleNode> _children;
        internal readonly Node Node;

        internal readonly SimpleGltfAsset SimpleGltfAsset;

        private SimpleNode(SimpleGltfAsset simpleGltfAsset)
        {
            SimpleGltfAsset = simpleGltfAsset;
            _children = new List<SimpleNode>();
        }

        internal SimpleNode(SimpleGltfAsset simpleGltfAsset, string name) : this(simpleGltfAsset)
        {
            Node = simpleGltfAsset.GltfAsset.CreateNode(name);
        }

        internal SimpleNode(SimpleScene simpleScene, string name) : this(simpleScene.SimpleGltfAsset)
        {
            Node = simpleScene.Scene.CreateNode(name);
        }

        internal SimpleNode(SimpleNode simpleNode, string name) : this(simpleNode.SimpleGltfAsset)
        {
            Node = simpleNode.Node.CreateChild(name);
        }

        public IEnumerable<SimpleNode> Children => _children;

        public SimpleMesh Mesh { get; private set; }

        public string Name => Node.Name;

        public SimpleNode CreateChild(string name = null)
        {
            var node = new SimpleNode(this, name);
            _children.Add(node);
            return node;
        }

        public SimpleMesh CreateMesh(string name = null)
        {
            if (Mesh != null)
                throw new NotImplementedException();
            Mesh = new SimpleMesh(SimpleGltfAsset, Node, name);
            return Mesh;
        }
    }
}
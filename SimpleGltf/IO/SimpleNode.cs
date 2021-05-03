using System;
using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleNode
    {
        private readonly IList<SimpleNode> _children;
        private readonly SimpleGltfAsset _simpleGltfAsset;

        internal readonly Node Node;

        private SimpleNode(SimpleGltfAsset simpleGltfAsset)
        {
            _simpleGltfAsset = simpleGltfAsset;
            _children = new List<SimpleNode>();
        }

        internal SimpleNode(SimpleGltfAsset simpleGltfAsset, GltfAsset gltfAsset, string name) : this(simpleGltfAsset)
        {
            Node = new Node(gltfAsset, name);
        }

        internal SimpleNode(SimpleGltfAsset simpleGltfAsset, Scene scene, string name) : this(simpleGltfAsset)
        {
            Node = new Node(scene, name);
        }

        internal SimpleNode(SimpleGltfAsset simpleGltfAsset, Node node, string name) : this(simpleGltfAsset)
        {
            Node = new Node(node, name);
        }

        public IEnumerable<SimpleNode> Children => _children;

        public SimpleMesh Mesh { get; private set; }

        public string Name
        {
            get => Node.Name;
            set => Node.Name = value;
        }

        public SimpleNode CreateChild(string name = null)
        {
            var node = new SimpleNode(_simpleGltfAsset, Node, name);
            _children.Add(node);
            return node;
        }

        public SimpleMesh CreateMesh()
        {
            if (Mesh != null)
                throw new NotImplementedException();
            Mesh = new SimpleMesh(_simpleGltfAsset, Node);
            return Mesh;
        }
    }
}
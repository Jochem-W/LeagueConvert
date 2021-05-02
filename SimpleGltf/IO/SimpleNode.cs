using System;
using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleNode
    {
        private readonly IList<SimpleNode> _children;

        internal readonly Node Node;

        private SimpleNode()
        {
            _children = new List<SimpleNode>();
        }

        internal SimpleNode(GltfAsset gltfAsset, string name) : this()
        {
            Node = new Node(gltfAsset, name);
        }

        internal SimpleNode(Scene scene, string name) : this()
        {
            Node = new Node(scene, name);
        }

        internal SimpleNode(Node node, string name) : this()
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
            var node = new SimpleNode(Node, name);
            _children.Add(node);
            return node;
        }

        public SimpleMesh CreateMesh()
        {
            if (Mesh != null)
                throw new NotImplementedException();
            Mesh = new SimpleMesh(Node);
            return Mesh;
        }
    }
}
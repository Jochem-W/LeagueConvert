using System.Collections.Generic;

namespace SimpleGltf.Json
{
    internal class Mesh
    {
        internal readonly GltfAsset GltfAsset;

        internal Mesh(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            Name = name;
            GltfAsset.Meshes ??= new List<Mesh>();
            GltfAsset.Meshes.Add(this);
        }

        internal Mesh(Node node, string name) : this(node.GltfAsset, name)
        {
            node.Mesh = this;
        }

        public IList<Primitive> Primitives { get; internal set; }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }
    }
}
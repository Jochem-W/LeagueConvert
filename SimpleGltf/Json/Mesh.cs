using System.Collections.Generic;

namespace SimpleGltf.Json
{
    internal class Mesh
    {
        internal readonly GltfAsset GltfAsset;

        internal Mesh(Node node)
        {
            GltfAsset = node.GltfAsset;
            node.Mesh = this;
            GltfAsset.Meshes ??= new List<Mesh>();
            GltfAsset.Meshes.Add(this);
        }

        public IList<Primitive> Primitives { get; set; }
    }
}
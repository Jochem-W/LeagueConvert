using System.Collections.Generic;

namespace SimpleGltf.Json
{
    public class Mesh
    {
        internal readonly int Index;
        internal readonly GltfAsset GltfAsset;

        internal Mesh(GltfAsset gltfAsset, string name)
        {
            GltfAsset = gltfAsset;
            GltfAsset.Meshes ??= new List<Mesh>();
            Index = GltfAsset.Meshes.Count;
            GltfAsset.Meshes.Add(this);
            Name = name;
        }

        public IList<Primitive> Primitives { get; internal set; }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }
    }
}
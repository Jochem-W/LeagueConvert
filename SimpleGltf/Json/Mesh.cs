using System.Collections.Generic;

namespace SimpleGltf.Json
{
    public class Mesh
    {
        internal readonly int Index;

        internal Mesh(GltfAsset gltfAsset, string name)
        {
            gltfAsset.Meshes ??= new List<Mesh>();
            Index = gltfAsset.Meshes.Count;
            gltfAsset.Meshes.Add(this);
            Name = name;
        }

        public IList<Primitive> Primitives { get; internal set; }

        //public IList<float> Weights { get; internal set; }

        public string Name { get; }
    }
}
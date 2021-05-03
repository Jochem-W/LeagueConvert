using System.Collections.Generic;
using SimpleGltf.Json;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.IO
{
    public class SimpleMesh
    {
        private readonly IList<SimplePrimitive> _primitives;
        internal readonly Mesh Mesh;

        internal readonly SimpleGltfAsset SimpleGltfAsset;

        internal SimpleMesh(SimpleGltfAsset simpleGltfAsset, Node node, string name)
        {
            SimpleGltfAsset = simpleGltfAsset;
            _primitives = new List<SimplePrimitive>();
            Mesh = node.CreateMesh(name);
        }

        public IEnumerable<SimplePrimitive> Primitives => _primitives;

        public SimplePrimitive CreatePrimitive()
        {
            var primitive = new SimplePrimitive(SimpleGltfAsset, Mesh);
            _primitives.Add(primitive);
            return primitive;
        }
    }
}
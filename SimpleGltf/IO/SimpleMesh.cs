using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleMesh
    {
        private readonly IList<SimplePrimitive> _primitives;
        private readonly SimpleGltfAsset _simpleGltfAsset;
        internal readonly Mesh Mesh;

        internal SimpleMesh(SimpleGltfAsset simpleGltfAsset, Node node)
        {
            _simpleGltfAsset = simpleGltfAsset;
            _primitives = new List<SimplePrimitive>();
            Mesh = new Mesh(node);
        }

        public IEnumerable<SimplePrimitive> Primitives => _primitives;

        public SimplePrimitive CreatePrimitive()
        {
            var primitive = new SimplePrimitive(_simpleGltfAsset, Mesh);
            _primitives.Add(primitive);
            return primitive;
        }
    }
}
using System.Collections.Generic;
using SimpleGltf.Json;

namespace SimpleGltf.IO
{
    public class SimpleMesh
    {
        private readonly IList<SimplePrimitive> _primitives;
        internal readonly Mesh Mesh;

        internal SimpleMesh(Node node)
        {
            _primitives = new List<SimplePrimitive>();
            Mesh = new Mesh(node);
        }

        public IEnumerable<SimplePrimitive> Primitives => _primitives;

        public SimplePrimitive CreatePrimitive()
        {
            var primitive = new SimplePrimitive(Mesh);
            _primitives.Add(primitive);
            return primitive;
        }
    }
}
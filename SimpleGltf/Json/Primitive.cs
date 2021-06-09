using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Primitive
    {
        private readonly Mesh _mesh;


        internal Primitive(Mesh mesh)
        {
            _mesh = mesh;
            Attributes = new Dictionary<string, Accessor>();
            _mesh.Primitives ??= new List<Primitive>();
            _mesh.Primitives.Add(this);
        }

        [JsonConverter(typeof(AttributesConverter))]
        public IDictionary<string, Accessor> Attributes { get; }

        [JsonConverter(typeof(AccessorConverter))]
        public Accessor Indices { get; set; }

        [JsonConverter(typeof(MaterialConverter))]
        public Material Material { get; set; }
    }
}
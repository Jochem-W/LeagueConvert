using System.Collections.Generic;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Primitive
    {
        internal Primitive(Mesh mesh)
        {
            mesh.Primitives.Add(this);
            Attributes = new Dictionary<string, Accessor>();
        }

        [JsonConverter(typeof(DictionaryIndexableConverter<Accessor>))]
        public IDictionary<string, Accessor> Attributes { get; }

        [JsonConverter(typeof(IndexableConverter<Accessor>))]
        public Accessor Indices { get; set; }

        [JsonConverter(typeof(IndexableConverter<Material>))]
        public Material Material { get; set; }
    }
}
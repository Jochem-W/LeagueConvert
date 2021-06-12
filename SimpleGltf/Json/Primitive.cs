using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class Primitive
    {
        internal Primitive(Mesh mesh)
        {
            mesh.Primitives.Add(this);
            Attributes = new Dictionary<string, IAccessor>();
        }

        [JsonConverter(typeof(DictionaryIndexableConverter<IAccessor>))] public IDictionary<string, IAccessor> Attributes { get; }

        [JsonConverter(typeof(IndexableConverter<IAccessor>))] public IAccessor Indices { get; set; }

        [JsonConverter(typeof(IndexableConverter<Material>))] public Material Material { get; set; }
    }
}
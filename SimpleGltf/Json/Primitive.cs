using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public class Primitive
    {
        internal Primitive(Mesh mesh)
        {
            Attributes = new Dictionary<string, IAccessor>();
            mesh.Primitives ??= new List<Primitive>();
            mesh.Primitives.Add(this);
        }

        [JsonIgnore] public IDictionary<string, IAccessor> Attributes { get; }

        [JsonPropertyName("attributes")]
        public IDictionary<string, int> AttributesIndices =>
            Attributes.ToDictionary(pair => pair.Key, pair => pair.Value.Index);

        [JsonIgnore] public IAccessor Indices { get; set; }

        [JsonPropertyName("indices")] public int IndicesIndex => Indices.Index;

        [JsonIgnore] public Material Material { get; set; }

        [JsonPropertyName("material")] public int MaterialIndex => Material.Index;
    }
}
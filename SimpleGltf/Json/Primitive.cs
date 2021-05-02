using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    internal class Primitive
    {
        internal readonly IDictionary<string, Accessor> Attributes;
        internal readonly Mesh Mesh;
        internal Accessor Indices;

        internal Primitive(Mesh mesh)
        {
            Mesh = mesh;
            Attributes = new Dictionary<string, Accessor>();
            Mesh.Primitives ??= new List<Primitive>();
            Mesh.Primitives.Add(this);
        }

        [JsonPropertyName("attributes")]
        public IDictionary<string, int?> AttributeReferences => new Dictionary<string, int?>(Attributes.Select(pair =>
            new KeyValuePair<string, int?>(pair.Key, Mesh.GltfAsset.Accessors?.NullableIndexOf(pair.Value))));

        [JsonPropertyName("indices")]
        public int? IndicesReference => Mesh.GltfAsset.Accessors?.NullableIndexOf(Indices);
    }
}
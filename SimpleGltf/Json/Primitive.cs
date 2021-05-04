using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using SimpleGltf.Extensions;

namespace SimpleGltf.Json
{
    public class Primitive
    {
        private readonly Mesh _mesh;
        internal readonly IDictionary<string, Accessor> Attributes;
        
        internal Primitive(Mesh mesh)
        {
            _mesh = mesh;
            Attributes = new Dictionary<string, Accessor>();
            _mesh.Primitives ??= new List<Primitive>();
            _mesh.Primitives.Add(this);
        }

        [JsonPropertyName("attributes")]
        public IDictionary<string, int> AttributeReferences => new Dictionary<string, int>(Attributes.Select(pair =>
            new KeyValuePair<string, int>(pair.Key, _mesh.GltfAsset.Accessors.IndexOf(pair.Value))));

        [JsonIgnore] public Accessor Indices { get; set; }
        
        [JsonPropertyName("indices")]
        public int? IndicesReference => Indices == null ? null : _mesh.GltfAsset.Accessors.IndexOf(Indices);
        
        [JsonIgnore] public Material Material { get; set; }

        public int? MaterialReference => Material == null ? null : _mesh.GltfAsset.Materials.IndexOf(Material);
    }
}
using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json;

public class MeshPrimitive
{
    internal readonly IDictionary<string, Accessor> AttributesDictionary;

    internal MeshPrimitive(Mesh mesh)
    {
        mesh.PrimitiveList.Add(this);
        AttributesDictionary = new Dictionary<string, Accessor>();
    }

    [JsonConverter(typeof(DictionaryIndexableConverter<Accessor>))]
    public IEnumerable<KeyValuePair<string, Accessor>> Attributes => AttributesDictionary;

    [JsonConverter(typeof(IndexableConverter<Accessor>))]
    public Accessor Indices { get; set; }

    [JsonConverter(typeof(IndexableConverter<Material>))]
    public Material Material { get; set; }
}
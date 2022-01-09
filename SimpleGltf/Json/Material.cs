using System.Collections;
using System.Text.Json.Serialization;
using SimpleGltf.Json.Extensions;

namespace SimpleGltf.Json;

public class Material : IIndexable
{
    private static readonly IDictionary<string, IDictionary> ExtensionsDictionary =
        new Dictionary<string, IDictionary>(new[]
        {
            new KeyValuePair<string, IDictionary>("KHR_materials_unlit", new Dictionary<string, int>())
        });

    internal Material(GltfAsset gltfAsset)
    {
        Index = gltfAsset.MaterialList.Count;
        gltfAsset.MaterialList.Add(this);
    }

    public PbrMetallicRoughness PbrMetallicRoughness { get; internal set; }

    public string Name { get; init; }

    [JsonConverter(typeof(ExtensionsConverter))]
    public IEnumerable<KeyValuePair<string, IDictionary>> Extensions => ExtensionsDictionary;

    [JsonIgnore] public int Index { get; }
}
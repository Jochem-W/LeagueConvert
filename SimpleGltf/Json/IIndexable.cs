using System.Text.Json.Serialization;

namespace SimpleGltf.Json
{
    public interface IIndexable
    {
        [JsonIgnore] int Index { get; }
    }
}
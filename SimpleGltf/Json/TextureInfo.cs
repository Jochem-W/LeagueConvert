using System.Text.Json.Serialization;
using SimpleGltf.Json.Converters;

namespace SimpleGltf.Json
{
    public class TextureInfo
    {
        internal TextureInfo(Texture texture)
        {
            Texture = texture;
        }

        [JsonConverter(typeof(IndexableConverter<Texture>))]
        [JsonPropertyName("index")]
        public Texture Texture { get; }
    }
}
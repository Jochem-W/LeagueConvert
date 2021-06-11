using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class IAccessorConverter : JsonConverter<IAccessor>
    {
        public override IAccessor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IAccessor value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GltfAsset.Accessors.IndexOf(value));
        }
    }
}
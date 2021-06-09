using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    public class AttributesConverter : JsonConverter<IDictionary<string, Accessor>>
    {
        public override IDictionary<string, Accessor> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, Accessor> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var (name, accessor) in value)
                writer.WriteNumber(name, accessor.GltfAsset.Accessors.IndexOf(accessor));
            writer.WriteEndObject();
        }
    }
}
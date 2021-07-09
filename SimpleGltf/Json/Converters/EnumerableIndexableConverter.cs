using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleGltf.Json.Converters
{
    internal class EnumerableIndexableConverter<T> : JsonConverter<IEnumerable<T>> where T : IIndexable
    {
        public override IList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var v in value)
                writer.WriteNumberValue(v.Index);
            writer.WriteEndArray();
        }
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleGltf.Enums;

namespace SimpleGltf.Json.Converters;

internal class MimeTypeConverter : JsonConverter<MimeType?>
{
    public override MimeType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, MimeType? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case MimeType.Png:
                writer.WriteStringValue("image/png");
                break;
            case MimeType.Jpg:
                writer.WriteStringValue("image/jpeg");
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
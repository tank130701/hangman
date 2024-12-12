using System.Text.Json;
using System.Text.Json.Serialization;
namespace client.Infrastructure
{
    // Универсальный конвертер для произвольного содержимого payload
    public class JsonStringConverter : JsonConverter<JsonElement?>
    {
        public override JsonElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonDocument.ParseValue(ref reader).RootElement;
        }

        public override void Write(Utf8JsonWriter writer, JsonElement? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                value.Value.WriteTo(writer);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
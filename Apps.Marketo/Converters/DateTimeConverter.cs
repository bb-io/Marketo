using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apps.Marketo;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (DateTime.TryParseExact(reader.GetString(), "yyyy-MM-ddTHH:mm:ssZzzz", null, DateTimeStyles.RoundtripKind, 
                    out DateTime result))
                return result;
        }

        throw new JsonException("The JSON value could not be converted to DateTime.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ssZzzz"));
    }
}
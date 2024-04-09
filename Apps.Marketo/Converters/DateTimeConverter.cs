using Newtonsoft.Json;
using System.Globalization;

namespace Apps.Marketo;

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            if (DateTime.TryParseExact(reader.Value.ToString(), "yyyy-MM-ddTHH:mm:ssZzzz", null, DateTimeStyles.RoundtripKind,
                    out DateTime result))
                return result;
        }

        throw new JsonException("The JSON value could not be converted to DateTime.");
    }

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ssZzzz"));
    }
}
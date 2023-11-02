using System.Text.Json;

namespace Apps.Marketo;

public class FollowupValueConverter : System.Text.Json.Serialization.JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal();

        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();
        
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (decimal.TryParse(value.ToString(), out var numberValue))
        {
            var intValue = (int)numberValue;
            writer.WriteNumberValue(intValue);
        }
        else
            writer.WriteStringValue(value.ToString());
    }

}
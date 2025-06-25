using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndanteTribe.IO.Json;

internal sealed class HeaderJsonConverter : JsonConverter<Dictionary<string, (int, int)>>
{
    public override Dictionary<string, (int, int)> Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions __)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
        }

        var result = new Dictionary<string, (int, int)>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return result;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var key = reader.GetString()!;

            reader.Read();
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Expected start of array for ValueTuple. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected first item of ValueTuple to be an integer. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
            }
            var item1 = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException($"Expected second item of ValueTuple to be an integer. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
            }
            var item2 = reader.GetInt32();

            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException($"Expected end of array for ValueTuple. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
            }

            result.Add(key, (item1, item2));
        }

        throw new JsonException($"Expected EndObject token. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, (int, int)> value, JsonSerializerOptions _)
    {
        writer.WriteStartObject();

        foreach (var (key, (item1, item2)) in value)
        {
            writer.WritePropertyName(key);
            writer.WriteStartArray();
            writer.WriteNumberValue(item1);
            writer.WriteNumberValue(item2);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AndanteTribe.IO.Json;

internal sealed class IntIntValueTupleJsonConverter : JsonConverter<ValueTuple<int, int>>
{
    public override (int, int) Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions __)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            throw new JsonException($"If Data is Null, ValueTuple is not null. TokenType: {reader.TokenType}, Position: {reader.TokenStartIndex}");
        }

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

        return (item1, item2);
    }

    public override void Write(Utf8JsonWriter writer, (int, int) value, JsonSerializerOptions _)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Item1);
        writer.WriteNumberValue(value.Item2);
        writer.WriteEndArray();
    }
}
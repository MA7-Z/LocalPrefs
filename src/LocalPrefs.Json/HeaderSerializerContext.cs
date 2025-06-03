using System.Text.Json.Serialization;

namespace AndanteTribe.IO.Json;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<string, LightRange>))]
internal partial class HeaderSerializerContext : JsonSerializerContext
{
}
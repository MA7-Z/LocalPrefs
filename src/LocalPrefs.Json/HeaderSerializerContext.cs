using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AndanteTribe.IO.Json;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
[JsonSerializable(typeof(Dictionary<string, LightRange>))]
[ExcludeFromCodeCoverage]
internal partial class HeaderSerializerContext : JsonSerializerContext
{
}
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace AndanteTribe.IO.Json;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    Converters = [typeof(IntIntValueTupleJsonConverter)])]
[JsonSerializable(typeof(Dictionary<string, ValueTuple<int, int>>))]
[ExcludeFromCodeCoverage]
internal partial class HeaderSerializerContext : JsonSerializerContext
{
}
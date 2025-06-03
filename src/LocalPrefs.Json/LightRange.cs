using System.Runtime.InteropServices;

namespace AndanteTribe.IO.Json;

[StructLayout(LayoutKind.Auto)]
internal record struct LightRange(int Offset, int Count);
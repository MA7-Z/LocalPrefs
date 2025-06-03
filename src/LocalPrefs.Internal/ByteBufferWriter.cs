using System.Buffers;

namespace AndanteTribe.IO.Internal;

internal sealed class ByteBufferWriter(byte[] buffer) : IBufferWriter<byte>
{
    private byte[] _buffer = buffer;
    public int CurrentOffset { get; set; }

    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, CurrentOffset);
    public ReadOnlyMemory<byte> WrittenMemory => new(_buffer, 0, CurrentOffset);

    public void Advance(int count) => CurrentOffset += count;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        var nextSize = CurrentOffset + sizeHint;
        if (_buffer.Length < nextSize)
        {
            Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, nextSize));
        }

        if (sizeHint == 0)
        {
            var result = new Memory<byte>(_buffer, CurrentOffset, _buffer.Length - CurrentOffset);
            return result.Length == 0 ? GetMemory(sizeHint: 1024) : result;
        }

        return new Memory<byte>(_buffer, CurrentOffset, sizeHint);
    }

    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

    public void Clear() => CurrentOffset = 0;
}
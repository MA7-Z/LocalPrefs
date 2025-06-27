using System.Buffers;

namespace AndanteTribe.IO.Internal;

internal sealed class ByteBufferWriter(byte[] buffer) : IBufferWriter<byte>
{
    private byte[] _buffer = buffer;
    public int CurrentOffset { get; set; }
    private (int offset, int count) _block = (-1, 0);

    public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, CurrentOffset);
    public ReadOnlyMemory<byte> WrittenMemory => new(_buffer, 0, CurrentOffset);

    public void Advance(int count)
    {
        if (_block.offset != -1 && _block.count >= 0)
        {
            _block = (_block.offset + count, _block.count - count);
        }
        else
        {
            CurrentOffset += count;
        }
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (_block.offset != -1 && _block.count >= 0)
        {
            if (_block.count < sizeHint)
            {
                var nextSize = CurrentOffset + sizeHint - _block.count;
                if (_buffer.Length < nextSize)
                {
                    Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, nextSize));
                }
                _buffer.AsSpan(_block.offset + _block.count, CurrentOffset - _block.offset - _block.count)
                    .CopyTo(_buffer.AsSpan(_block.offset + sizeHint));
                CurrentOffset += sizeHint - _block.count;
                _block = _block with { count = sizeHint };
            }

            var result = new Memory<byte>(_buffer, _block.offset, _block.count);
            return sizeHint == 0 && result.Length == 0 ? GetMemory(sizeHint: 1024) : result;
        }
        else
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
    }

    public Span<byte> GetSpan(int sizeHint = 0) => GetMemory(sizeHint).Span;

    public void Clear()
    {
        CurrentOffset = 0;
        _block = (-1, 0);
    }

    public WriteBlockHandle GetWriteBlockScope(int offset, int count) => new(this, offset, count);

    public readonly struct WriteBlockHandle : IDisposable
    {
        private readonly ByteBufferWriter _writer;
        private readonly int _offset;

        public int Consumed => _writer._block.offset - _offset;

        public WriteBlockHandle(ByteBufferWriter writer, int offset, int count)
        {
            if (0 <= offset && offset < writer._buffer.Length && 0 <= count && count <= writer._buffer.Length - offset)
            {
                writer._block = (_offset = offset, count);
                _writer = writer;
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(offset), "Offset and count must be within the bounds of the buffer.");
        }

        public void Dispose()
        {
            var (offset, count) = _writer._block;
            if (count > 0)
            {
                _writer._buffer.AsSpan(offset + count).CopyTo(_writer._buffer.AsSpan(offset));
                _writer.CurrentOffset -= count;
            }
            _writer._block = (-1, 0);
        }
    }
}
#if UNITY_WEBGL
#nullable enable

using System;
using System.IO;

namespace AndanteTribe.IO.Unity
{
    public class LSStream : Stream
    {
        public readonly string Path;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException("Length is not supported for IndexedDBStream.");
        public override long Position
        {
            get => throw new NotSupportedException("Position is not supported for IndexedDBStream.");
            set => throw new NotSupportedException("Position is not supported for IndexedDBStream.");
        }

        public LSStream(string path) => Path = path;

        public override void Flush()
        {
            // Flush is typically implemented as an empty method to ensure full compatibility with other Stream types.
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = LSUtils.ReadAllText(Path);
            return Convert.TryFromBase64String(data, buffer.AsSpan(offset, count), out var written) ? written : 0;
        }

        public override int ReadByte()
        {
            var data = LSUtils.ReadAllText(Path);
            var buffer = (Span<byte>)stackalloc byte[1];
            if (!Convert.TryFromBase64String(data, buffer, out var written) || written == 0)
            {
                return -1;
            }

            return buffer[0];
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("Seek is not supported for IndexedDBStream.");

        public override void SetLength(long value) => throw new NotSupportedException("SetLength is not supported for IndexedDBStream.");

        public override void Write(byte[] buffer, int offset, int count) =>
            LSUtils.WriteAllBytes(Path, buffer.AsSpan(offset, count));
    }
}

#endif
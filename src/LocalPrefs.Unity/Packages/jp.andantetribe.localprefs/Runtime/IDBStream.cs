#if UNITY_WEBGL
#nullable enable

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AndanteTribe.IO.Unity
{
    /// <summary>
    /// Represents a stream for IndexedDB operations.
    /// </summary>
    /// <remarks>No multi-threading support because multi-threading is not allowed in the WebGL environment.</remarks>
    public class IDBStream : Stream
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

        public IDBStream(string path) => Path = path;

        public override void Flush()
        {
            // Flush is typically implemented as an empty method to ensure full compatibility with other Stream types.
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(new Memory<byte>(buffer, offset, count)).GetAwaiter().GetResult();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(new Memory<byte>(buffer, offset, count), cancellationToken).AsTask();

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = IDBValueTaskSourcePool.Shared.Get();
            source.Buffer = buffer;
            var eventID = EventID.GetNext(source);

            await using var _ = cancellationToken.RegisterWithoutCaptureExecutionContext(() => IDBUtils.CancelEventInternal(eventID));

            IDBUtils.ReadAllBytesInternal(Path, EventID.GetNext(source));
            return (await new ValueTask<(byte[] _, int size)>(source, source.Version)).size;
        }

        public override int ReadByte()
        {
            var oneByteArray = ArrayPool<byte>.Shared.Rent(1);
            try
            {
                var r = Read(oneByteArray, 0, 1);
                return r == 0 ? -1 : oneByteArray[0];
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(oneByteArray);
            }
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException("Seek is not supported for IndexedDBStream.");

        public override void SetLength(long value) => throw new NotSupportedException("SetLength is not supported for IndexedDBStream.");

        public override void Write(byte[] buffer, int offset, int count) =>
            WriteAsync(new(buffer, offset, count)).GetAwaiter().GetResult();

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return IDBUtils.WriteAllBytesAsync(Path, new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).AsTask();
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return IDBUtils.WriteAllBytesAsync(Path, buffer, cancellationToken);
        }
    }
}

#endif
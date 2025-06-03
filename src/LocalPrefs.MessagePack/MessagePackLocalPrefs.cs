using System.Buffers;
using MessagePack;
using MessagePack.Formatters;
using AndanteTribe.IO.Internal;

namespace AndanteTribe.IO.MessagePack;

/// <summary>
/// MessagePack-based implementation of <see cref="ILocalPrefs"/> that provides efficient key-value storage
/// with persistent data on the filesystem. This implementation uses <see cref="MessagePack"/> for serialization,
/// which offers high-performance binary serialization with smaller output size compared to JSON.
/// The class maintains an in-memory index of stored data for optimized reading and writing operations,
/// and supports LZ4 compression by default.
/// </summary>
public class MessagePackLocalPrefs : ILocalPrefs
{
    private readonly string _savePath;
    private readonly MessagePackSerializerOptions? _options;
    private readonly IFileAccessor _fileAccessor;
    private readonly Dictionary<string, (int offset, int count)> _header;
    private readonly ByteBufferWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackLocalPrefs"/> class with a custom formatter resolver.
    /// This constructor provides a convenient way to specify a custom formatter resolver for <see cref="MessagePack"/> serialization.
    /// The resolver will be applied to the default MessagePack serializer options.
    /// </summary>
    /// <param name="savePath">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    /// <param name="resolver">Optional custom formatter resolver for <see cref="MessagePack"/> serialization. If null, default resolver is used.</param>
    /// <param name="fileAccessor">Optional file system accessor for reading/writing operations. If null, the default implementation is used.</param>
    public MessagePackLocalPrefs(string savePath, IFormatterResolver? resolver, IFileAccessor? fileAccessor = null)
        : this(savePath, resolver == null ? null : MessagePackSerializer.DefaultOptions.WithResolver(resolver), fileAccessor)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackLocalPrefs"/> class with custom serializer options.
    /// This constructor loads existing data from the specified file path if available,
    /// or initializes a new storage structure if the file doesn't exist.
    /// The implementation maintains a header dictionary that maps keys to their position
    /// in the data buffer for efficient retrieval and updates.
    /// LZ4 block compression is applied by default to reduce storage size.
    /// </summary>
    /// <param name="savePath">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    /// <param name="options">Optional MessagePack serializer options to customize serialization behavior. If null, default options are used with LZ4 compression.</param>
    /// <param name="fileAccessor">Optional file system accessor for reading/writing operations. If null, the default implementation is used.</param>
    public MessagePackLocalPrefs(string savePath, MessagePackSerializerOptions? options = null, IFileAccessor? fileAccessor = null)
    {
        _savePath = savePath;
        _options = (options ?? MessagePackSerializer.DefaultOptions).WithCompression(MessagePackCompression.Lz4Block);
        _fileAccessor = fileAccessor ?? IFileAccessor.Default;

        var dataArray = _fileAccessor.ReadAllBytes(savePath);
        _writer = new ByteBufferWriter(dataArray);
        if (dataArray.Length > 0)
        {
            var reader = new MessagePackReader(dataArray);
            var formatter = new DictionaryFormatter<string, (int, int)>();
            _header = formatter.Deserialize(ref reader, HeaderFormatterResolver.StandardOptions) ?? new();

            var consumed = (int)reader.Consumed;
            var dataLength = dataArray.Length - consumed;
            dataArray.AsSpan(consumed, dataLength).CopyTo(dataArray.AsSpan(0, dataLength));
            _writer.CurrentOffset = dataLength;
        }
        else
        {
            _header = new();
        }
    }

    /// <inheritdoc />
    public T? Load<T>(string key)
    {
        if (_header.TryGetValue(key, out var v))
        {
            var (offset, count) = v;
            var reader = new MessagePackReader(_writer.WrittenMemory.Slice(offset, count));
            return MessagePackSerializer.Deserialize<T>(ref reader, _options);
        }

        return default;
    }

    /// <inheritdoc />
    public async ValueTask SaveAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_header.TryGetValue(key, out var prev))
        {
            var trailingOffset = prev.offset + prev.count;
            using (ArrayPool<byte>.Shared.Rent(_writer.CurrentOffset - trailingOffset, out var trailingData))
            {
                _writer.WrittenSpan[trailingOffset..].CopyTo(trailingData);

                _writer.CurrentOffset = prev.offset;
                MessagePackSerializer.Serialize(_writer, value, _options, cancellationToken);
                var count = _writer.CurrentOffset - prev.offset;
                _header[key] = (prev.offset, count);

                trailingData.CopyTo(_writer.GetSpan(trailingData.Length));
                _writer.Advance(trailingData.Length);

                using (ArrayPool<string>.Shared.Rent(_header.Count, out var updateKeys))
                {
                    var i = 0;
                    foreach (var (k, (o, _)) in _header)
                    {
                        if (o > prev.offset)
                        {
                            updateKeys[i++] = k;
                        }
                    }

                    var diff = count - prev.count;
                    foreach (var k in updateKeys[..i])
                    {
                        var (o, c) = _header[k];
                        _header[k] = (o + diff, c);
                    }
                }
            }
        }
        else
        {
            var currentOffset = _writer.CurrentOffset;
            MessagePackSerializer.Serialize(_writer, value, _options, cancellationToken);
            _header.Add(key, (currentOffset, _writer.CurrentOffset - currentOffset));
        }

        await using var stream = _fileAccessor.GetWriteStream(_savePath);
        await MessagePackSerializer.SerializeAsync(stream, _header, HeaderFormatterResolver.StandardOptions, cancellationToken);
        await stream.WriteAsync(_writer.WrittenMemory, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prev = _header[key];
        _header.Remove(key);

        var trailingOffset = prev.offset + prev.count;
        using (ArrayPool<byte>.Shared.Rent(_writer.CurrentOffset - trailingOffset, out var trailingData))
        {
            _writer.WrittenSpan[trailingOffset..].CopyTo(trailingData);
            _writer.CurrentOffset = prev.offset;
            trailingData.CopyTo(_writer.GetSpan(trailingData.Length));
            _writer.Advance(trailingData.Length);
        }

        using (ArrayPool<string>.Shared.Rent(_header.Count, out var updateKeys))
        {
            var i = 0;
            foreach (var (k, (o, _)) in _header)
            {
                if (o > prev.offset)
                {
                    updateKeys[i++] = k;
                }
            }

            foreach (var k in updateKeys[..i])
            {
                var (o, c) = _header[k];
                _header[k] = (o - prev.count, c);
            }
        }

        await using var stream = _fileAccessor.GetWriteStream(_savePath);
        await MessagePackSerializer.SerializeAsync(stream, _header, HeaderFormatterResolver.StandardOptions, cancellationToken);
        await stream.WriteAsync(_writer.WrittenMemory, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _header.Clear();
        _writer.Clear();

        return _fileAccessor.DeleteAsync(_savePath, cancellationToken);
    }

    /// <inheritdoc />
    public bool HasKey(string key) => _header.ContainsKey(key);
}

using System.Buffers;
using System.Text.Json;
using AndanteTribe.IO.Internal;

namespace AndanteTribe.IO.Json;

/// <summary>
/// JSON-based implementation of <see cref="ILocalPrefs"/> that provides efficient key-value storage
/// with persistent data on the filesystem. This implementation uses <see cref="System.Text.Json"/> for serialization
/// and maintains an in-memory index of stored data for optimized reading and writing operations.
/// </summary>
public class JsonLocalPrefs : ILocalPrefs
{
    private readonly string _savePath;
    private readonly JsonSerializerOptions? _options;
    private readonly IFileAccessor _fileAccessor;
    private readonly Dictionary<string, LightRange> _header;
    private readonly ByteBufferWriter _writer;
    private readonly Utf8JsonWriter _jsonWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLocalPrefs"/> class.
    /// This constructor loads existing data from the specified file path if available,
    /// or initializes a new storage structure if the file doesn't exist.
    /// The implementation maintains a header dictionary that maps keys to their position
    /// in the data buffer for efficient retrieval and updates.
    /// </summary>
    /// <param name="savePath">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    /// <param name="options">Optional JSON serializer options to customize serialization behavior. If null, default options are used.</param>
    /// <param name="fileAccessor">Optional file system accessor for reading/writing operations. If null, the default implementation is used.</param>
    public JsonLocalPrefs(string savePath, JsonSerializerOptions? options = null, IFileAccessor? fileAccessor = null)
    {
        _savePath = savePath;
        _options = options ?? JsonSerializerOptions.Default;
        _fileAccessor = fileAccessor ?? IFileAccessor.Default;

        var dataArray = _fileAccessor.ReadAllBytes(savePath);
        _writer = new ByteBufferWriter(dataArray);
        if (dataArray.Length > 0)
        {
            var reader = new Utf8JsonReader(dataArray);
            _header = JsonSerializer.Deserialize<Dictionary<string, LightRange>>(ref reader, HeaderSerializerContext.Default.Options) ?? new();

            var consumed = (int)reader.BytesConsumed;
            var dataLength = dataArray.Length - consumed;
            dataArray.AsSpan(consumed, dataLength).CopyTo(dataArray.AsSpan(0, dataLength));
            _writer.CurrentOffset = dataLength;
        }
        else
        {
            _header = new();
        }

        _jsonWriter = new Utf8JsonWriter(_writer);
    }

    /// <inheritdoc />
    public T? Load<T>(string key)
    {
        if (_header.TryGetValue(key, out var v))
        {
            var (offset, count) = v;
            var reader = new Utf8JsonReader(_writer.WrittenSpan.Slice(offset, count));
            return JsonSerializer.Deserialize<T>(ref reader, _options);
        }

        return default;
    }

    /// <inheritdoc />
    public async ValueTask SaveAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_header.TryGetValue(key, out var prev))
        {
            var trailingOffset = prev.Offset + prev.Count;
            using (ArrayPool<byte>.Shared.Rent(_writer.CurrentOffset - trailingOffset, out var trailingData))
            {
                _writer.WrittenSpan[trailingOffset..].CopyTo(trailingData);

                _writer.CurrentOffset = prev.Offset;
                JsonSerializer.Serialize(_jsonWriter, value, _options);
                _jsonWriter.Reset();
                var count = _writer.CurrentOffset - prev.Offset;
                _header[key] = prev with { Count = count };

                trailingData.CopyTo(_writer.GetSpan(trailingData.Length));
                _writer.Advance(trailingData.Length);

                using (ArrayPool<string>.Shared.Rent(_header.Count, out var updateKeys))
                {
                    var i = 0;
                    foreach (var (k, (o, _)) in _header)
                    {
                        if (o > prev.Offset)
                        {
                            updateKeys[i++] = k;
                        }
                    }

                    var diff = count - prev.Count;
                    foreach (var k in updateKeys[..i])
                    {
                        var v = _header[k];
                        _header[k] = v with { Offset = v.Offset + diff };
                    }
                }
            }
        }
        else
        {
            var currentOffset = _writer.CurrentOffset;
            JsonSerializer.Serialize(_jsonWriter, value, _options);
            _jsonWriter.Reset();
            _header.Add(key, new(currentOffset, _writer.CurrentOffset - currentOffset));
        }

        await using var stream = _fileAccessor.GetWriteStream(_savePath);
        await JsonSerializer.SerializeAsync(stream, _header, HeaderSerializerContext.Default.Options, cancellationToken);
        await stream.WriteAsync(_writer.WrittenMemory, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prev = _header[key];
        _header.Remove(key);

        var trailingOffset = prev.Offset + prev.Count;
        using (ArrayPool<byte>.Shared.Rent(_writer.CurrentOffset - trailingOffset, out var trailingData))
        {
            _writer.WrittenSpan[trailingOffset..].CopyTo(trailingData);
            _writer.CurrentOffset = prev.Offset;
            trailingData.CopyTo(_writer.GetSpan(trailingData.Length));
            _writer.Advance(trailingData.Length);
        }

        using (ArrayPool<string>.Shared.Rent(_header.Count, out var updateKeys))
        {
            var i = 0;
            foreach (var (k, (o, _)) in _header)
            {
                if (o > prev.Offset)
                {
                    updateKeys[i++] = k;
                }
            }

            foreach (var k in updateKeys[..i])
            {
                var v = _header[k];
                _header[k] = v with { Offset = v.Offset - prev.Count };
            }
        }

        await using var stream = _fileAccessor.GetWriteStream(_savePath);
        await JsonSerializer.SerializeAsync(stream, _header, HeaderSerializerContext.Default.Options, cancellationToken);
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

using System.Text.Json;
using System.Text.Json.Serialization;
using AndanteTribe.IO.Internal;

namespace AndanteTribe.IO.Json;

/// <summary>
/// JSON-based implementation of <see cref="ILocalPrefs"/> that provides efficient key-value storage
/// with persistent data on the filesystem. This implementation uses <see cref="System.Text.Json"/> for serialization
/// and maintains an in-memory index of stored data for optimized reading and writing operations.
/// </summary>
public class JsonLocalPrefs : ILocalPrefs
{
    private static readonly JsonSerializerOptions s_headerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters = { new IntIntValueTupleJsonConverter() },
    };

    private readonly JsonSerializerOptions? _options;
    private readonly IFileAccessor _fileAccessor;
    private readonly Dictionary<string, (int offset, int count)> _header;
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
    public JsonLocalPrefs(in string savePath, JsonSerializerOptions? options = null) : this(IFileAccessor.Create(savePath), options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLocalPrefs"/> class.
    /// This constructor loads existing data from the specified file path if available,
    /// or initializes a new storage structure if the file doesn't exist.
    /// The implementation maintains a header dictionary that maps keys to their position
    /// in the data buffer for efficient retrieval and updates.
    /// </summary>
    /// <param name="fileAccessor">Optional file system accessor for reading/writing operations. If null, the default implementation is used.</param>
    /// <param name="options">Optional JSON serializer options to customize serialization behavior. If null, default options are used.</param>
    public JsonLocalPrefs(IFileAccessor fileAccessor, JsonSerializerOptions? options = null)
    {
        _fileAccessor = fileAccessor;
        _options = options ?? JsonSerializerOptions.Default;

        var dataArray = _fileAccessor.ReadAllBytes();
        _writer = new ByteBufferWriter(dataArray);
        if (dataArray.Length > 0)
        {
            var reader = new Utf8JsonReader(dataArray);
            _header = JsonSerializer.Deserialize<Dictionary<string, (int, int)>>(ref reader, s_headerOptions) ?? new();

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
            var trailingOffset = prev.offset + prev.count;
            using var trailingData = new PooledList<byte>(_writer.CurrentOffset - trailingOffset);
            trailingData.AddRange(_writer.WrittenSpan[trailingOffset..]);

            _writer.CurrentOffset = prev.offset;
            JsonSerializer.Serialize(_jsonWriter, value, _options);
            _jsonWriter.Reset();
            var count = _writer.CurrentOffset - prev.offset;
            _header[key] = prev with { count = count };

            trailingData.AsSpan().CopyTo(_writer.GetSpan(trailingData.Count));
            _writer.Advance(trailingData.Count);

            using var updateKeys = new PooledList<string>(_header.Count);
            foreach (var (k, (o, _)) in _header)
            {
                if (o > prev.offset)
                {
                    updateKeys.Add(k);
                }
            }

            var diff = count - prev.count;
            foreach (var k in updateKeys.AsSpan())
            {
                var v = _header[k];
                _header[k] = v with { offset = v.offset + diff };
            }
        }
        else
        {
            var currentOffset = _writer.CurrentOffset;
            JsonSerializer.Serialize(_jsonWriter, value, _options);
            _jsonWriter.Reset();
            _header.Add(key, (currentOffset, _writer.CurrentOffset - currentOffset));
        }

        await using var stream = _fileAccessor.GetWriteStream();
        await JsonSerializer.SerializeAsync(stream, _header, s_headerOptions, cancellationToken);
        await stream.WriteAsync(_writer.WrittenMemory, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prev = _header[key];
        _header.Remove(key);

        var trailingOffset = prev.offset + prev.count;
        using (var trailingData = new PooledList<byte>(_writer.CurrentOffset - trailingOffset))
        {
            trailingData.AddRange(_writer.WrittenSpan[trailingOffset..]);
            _writer.CurrentOffset = prev.offset;
            trailingData.AsSpan().CopyTo(_writer.GetSpan(trailingData.Count));
            _writer.Advance(trailingData.Count);
        }

        using (var updateKeys = new PooledList<string>(_header.Count))
        {
            foreach (var (k, (o, _)) in _header)
            {
                if (o > prev.offset)
                {
                    updateKeys.Add(k);
                }
            }

            foreach (var k in updateKeys.AsSpan())
            {
                var v = _header[k];
                _header[k] = v with { offset = v.offset - prev.count };
            }
        }

        await using var stream = _fileAccessor.GetWriteStream();
        await JsonSerializer.SerializeAsync(stream, _header, s_headerOptions, cancellationToken);
        await stream.WriteAsync(_writer.WrittenMemory, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _header.Clear();
        _writer.Clear();

        return _fileAccessor.DeleteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public bool HasKey(string key) => _header.ContainsKey(key);
}
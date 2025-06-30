using System.Security.Cryptography;

namespace AndanteTribe.IO;

/// <summary>
/// A file accessor that encrypts and decrypts file content using AES encryption.
/// Implements the decorator pattern by wrapping another <see cref="IFileAccessor"/> instance.
/// </summary>
public class CryptoFileAccessor : IFileAccessor
{
    private readonly IFileAccessor _fileAccessor;
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly CipherMode _mode;

    /// <inheritdoc />
    public string SavePath { get; init; }

    /// <summary>
    /// The underlying file accessor that performs actual file operations.
    /// </summary>
    /// <param name="fileAccessor">FileAccessor to be decorated with encryption.</param>
    /// <param name="key">Encryption key used for AES encryption.</param>
    public CryptoFileAccessor(IFileAccessor fileAccessor, byte[] key) : this(fileAccessor, key, [], CipherMode.ECB)
    {
    }

    /// <summary>
    /// A file accessor that encrypts and decrypts file content using AES encryption.
    /// Implements the decorator pattern by wrapping another FileAccessor instance.
    /// </summary>
    /// <param name="fileAccessor">The underlying file accessor to be decorated with encryption.</param>
    /// <param name="key">The encryption key used for AES encryption.</param>
    /// <param name="iv">The initialization vector used for AES encryption.</param>
    /// <param name="mode">The cipher mode to use for AES encryption. Defaults to CBC.</param>
    public CryptoFileAccessor(IFileAccessor fileAccessor, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC)
    {
        _fileAccessor = fileAccessor;
        _key = key;
        _iv = iv;
        _mode = mode;
        SavePath = fileAccessor.SavePath;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoFileAccessor"/> class with a specified file path and encryption key.
    /// </summary>
    /// <param name="path">Path to the file where preference data will be stored.</param>
    /// <param name="key">Encryption key used for AES encryption.</param>
    public CryptoFileAccessor(in string path, byte[] key) : this(path, key, [], CipherMode.ECB)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoFileAccessor"/> class with a specified file path.
    /// </summary>
    /// <param name="path">Path to the file where preference data will be stored.</param>
    /// <param name="key">Encryption key used for AES encryption.</param>
    /// <param name="iv">Initialization vector used for AES encryption.</param>
    /// <param name="mode">Cipher mode to use for AES encryption. Defaults to CBC.</param>
    public CryptoFileAccessor(in string path, byte[] key, byte[] iv, CipherMode mode = CipherMode.CBC)
    {
        _fileAccessor = IFileAccessor.Create(path);
        _key = key;
        _iv = iv;
        _mode = mode;
        SavePath = path;
    }

    /// <inheritdoc />
    public byte[] ReadAllBytes()
    {
        var encryptedBytes = _fileAccessor.ReadAllBytes();
        if (encryptedBytes.Length == 0)
        {
            return [];
        }

        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var decryptedStream = new MemoryStream();
        cryptoStream.CopyTo(decryptedStream);
        return decryptedStream.ToArray();
    }

    /// <inheritdoc />
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        await using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(bytes.Span);
        cryptoStream.FlushFinalBlock();
        await _fileAccessor.WriteAsync(new(memoryStream.GetBuffer(), 0, (int)memoryStream.Length), cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(CancellationToken cancellationToken = default) => _fileAccessor.DeleteAsync(cancellationToken);

    /// <summary>
    /// Creates an AES algorithm instance with the provided key, IV, and cipher mode.
    /// </summary>
    /// <returns>A configured AES algorithm instance.</returns>
    private Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Key = _key;
        if (_iv.Length != 0)
        {
            aes.IV = _iv;
        }
        aes.Mode = _mode;
        return aes;
    }
}
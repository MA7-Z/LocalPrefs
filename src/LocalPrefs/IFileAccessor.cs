namespace AndanteTribe.IO;

/// <summary>
/// Interface that provides access to the file system.
/// Abstracts file read/write operations for local preferences.
/// </summary>
public interface IFileAccessor
{
    /// <summary>
    /// The file path where preference data will be stored.
    /// </summary>
    string SavePath { protected internal get; init; }

    /// <summary>
    /// Reads the entire file into a byte array.
    /// </summary>
    /// <returns>A byte array containing the file's contents.</returns>
    byte[] ReadAllBytes();

    /// <summary>
    /// Writes a byte array to the file asynchronously.
    /// </summary>
    /// <param name="bytes">Bytes to write to the file.</param>
    /// <param name="cancellationToken"> A token to cancel the asynchronous operation.</param>
    ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a default <see cref="FileAccessor"/> for the specified path.
    /// </summary>
    /// <param name="path">Path to the file where preference data will be stored.</param>
    /// <returns>An instance of <see cref="IFileAccessor"/> that uses the default file system operations.</returns>
    static IFileAccessor Create(in string path) => new DefaultFileAccessor(path);

    /// <summary>
    /// Default implementation of the IFileAccessor interface.
    /// Provides standard file system operations using <see cref="System.IO"/>.
    /// </summary>
    /// <param name="path">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    private sealed class DefaultFileAccessor(in string path) : IFileAccessor
    {
        /// <inheritdoc />
        public string SavePath { get; init; } = path;

        /// <inheritdoc />
        public byte[] ReadAllBytes() => File.Exists(SavePath) ? File.ReadAllBytes(SavePath) : [];

        /// <inheritdoc />
        public async ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var stream = File.Create(SavePath, 4096, FileOptions.Asynchronous);
            await stream.WriteAsync(bytes, cancellationToken);
        }

        /// <inheritdoc />
        public ValueTask DeleteAsync(CancellationToken cancellationToken = default)
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            return default;
        }
    }
}
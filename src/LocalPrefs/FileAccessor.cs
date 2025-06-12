namespace AndanteTribe.IO;

/// <summary>
/// Abstract class that provides access to the file system.
/// Abstracts file read/write operations for local preferences.
/// </summary>
public abstract class FileAccessor(string savePath)
{
    /// <summary>
    /// The file path where preference data will be stored.
    /// </summary>
    protected internal readonly string SavePath = savePath;

    /// <summary>
    /// Reads the entire file into a byte array.
    /// </summary>
    /// <returns>A byte array containing the file's contents.</returns>
    public abstract byte[] ReadAllBytes();

    /// <summary>
    /// Gets a stream for writing to a file.
    /// </summary>
    /// <returns>A stream that can be used to write to the file.</returns>
    public abstract Stream GetWriteStream();

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public abstract ValueTask DeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a default <see cref="FileAccessor"/> for the specified path.
    /// </summary>
    /// <param name="path">Path to the file where preference data will be stored.</param>
    /// <returns>An instance of <see cref="FileAccessor"/> that uses the default file system operations.</returns>
    public static FileAccessor Create(in string path) => new DefaultFileAccessor(path);

    /// <summary>
    /// Default implementation of the FileAccessor interface.
    /// Provides standard file system operations using <see cref="System.IO"/>.
    /// </summary>
    /// <param name="path">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    private sealed class DefaultFileAccessor(in string path) : FileAccessor(path)
    {
        /// <inheritdoc />
        public override byte[] ReadAllBytes() =>
            File.Exists(SavePath) ? File.ReadAllBytes(SavePath) : [];

        /// <inheritdoc />
        public override Stream GetWriteStream() =>
            new FileStream(SavePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, true);

        /// <inheritdoc />
        public override ValueTask DeleteAsync(CancellationToken cancellationToken = default)
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            return default;
        }
    }
}
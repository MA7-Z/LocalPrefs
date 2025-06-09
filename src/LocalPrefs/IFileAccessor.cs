namespace AndanteTribe.IO;

/// <summary>
/// Interface that provides access to the file system.
/// Abstracts file read/write operations for local preferences.
/// </summary>
public interface IFileAccessor
{
    /// <summary>
    /// Reads the entire file into a byte array.
    /// </summary>
    /// <returns>A byte array containing the file's contents.</returns>
    byte[] ReadAllBytes();

    /// <summary>
    /// Gets a stream for writing to a file.
    /// </summary>
    /// <returns>A stream that can be used to write to the file.</returns>
    Stream GetWriteStream();

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a default <see cref="IFileAccessor"/> for the specified path.
    /// </summary>
    /// <param name="path">Path to the file where preference data will be stored.</param>
    /// <returns>An instance of <see cref="IFileAccessor"/> that uses the default file system operations.</returns>
    static IFileAccessor Create(in string path) => new DefaultFileAccessor(path);

    /// <summary>
    /// Default implementation of the IFileAccessor interface.
    /// Provides standard file system operations using <see cref="System.IO"/>.
    /// </summary>
    /// <param name="savePath">The file path where preference data will be stored. The file will be created if it doesn't exist.</param>
    private sealed class DefaultFileAccessor(string savePath) : IFileAccessor
    {
        /// <inheritdoc />
        byte[] IFileAccessor.ReadAllBytes() =>
            File.Exists(savePath) ? File.ReadAllBytes(savePath) : [];

        /// <inheritdoc />
        Stream IFileAccessor.GetWriteStream() =>
            new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, true);

        /// <inheritdoc />
        ValueTask IFileAccessor.DeleteAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            return default;
        }
    }
}
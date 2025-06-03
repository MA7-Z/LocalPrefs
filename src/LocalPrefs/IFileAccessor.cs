using System.Diagnostics.CodeAnalysis;

namespace AndanteTribe.IO;

/// <summary>
/// Interface that provides access to the file system.
/// Abstracts file read/write operations for local preferences.
/// </summary>
public interface IFileAccessor
{
    private static IFileAccessor? s_default;

    /// <summary>
    /// Returns the default implementation of <see cref="IFileAccessor"/>.
    /// This implementation uses <see cref="System.IO"/> for file operations.
    /// </summary>
    static IFileAccessor Default
    {
        get => s_default ??= new DefaultFileAccessor();
        [ExcludeFromCodeCoverage]
        internal set => s_default = value;
    }

    /// <summary>
    /// Reads the entire file into a byte array.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A byte array containing the file's contents.</returns>
    byte[] ReadAllBytes(in string path);

    /// <summary>
    /// Gets a stream for writing to a file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A stream that can be used to write to the file.</returns>
    Stream GetWriteStream(in string path);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="path">The path to the file to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Default implementation of the IFileAccessor interface.
    /// Provides standard file system operations using <see cref="System.IO"/>.
    /// </summary>
    private sealed class DefaultFileAccessor : IFileAccessor
    {
        /// <inheritdoc />
        byte[] IFileAccessor.ReadAllBytes(in string path) =>
            File.Exists(path) ? File.ReadAllBytes(path) : [];

        /// <inheritdoc />
        Stream IFileAccessor.GetWriteStream(in string path) =>
            new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1, true);

        /// <inheritdoc />
        ValueTask IFileAccessor.DeleteAsync(string path, CancellationToken cancellationToken)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            return default;
        }
    }
}
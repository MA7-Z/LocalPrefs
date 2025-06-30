#if UNITY_WEBGL
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AndanteTribe.IO.Unity
{
    /// <summary>
    /// Provides file access functionality for WebGL builds using Local Storage.
    /// </summary>
    public class LSAccessor : IFileAccessor
    {
        private readonly string _savePath;

        string IFileAccessor.SavePath
        {
            get => _savePath;
            init => _savePath = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LSAccessor"/> class with the specified path.
        /// </summary>
        /// <param name="path">The key to the Local Storage file.</param>
        public LSAccessor(in string path) => _savePath = path;

        /// <inheritdoc />
        public byte[] ReadAllBytes() => LSUtils.ReadAllBytes(_savePath);

        /// <inheritdoc />
        public ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LSUtils.WriteAllBytes(_savePath, bytes.Span);
            return default;
        }

        /// <inheritdoc />
        public ValueTask DeleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LSUtils.Delete(_savePath);
            return default;
        }
    }
}

#endif
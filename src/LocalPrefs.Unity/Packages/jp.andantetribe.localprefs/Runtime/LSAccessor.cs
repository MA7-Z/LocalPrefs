#if UNITY_WEBGL
#nullable enable

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AndanteTribe.IO.Unity
{
    /// <summary>
    /// Provides file access functionality for WebGL builds using Local Storage.
    /// </summary>
    public class LSAccessor : IFileAccessor
    {
        private readonly string _path;
        private readonly LSStream _cacheWriteStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="LSAccessor"/> class with the specified path.
        /// </summary>
        /// <param name="path">The key to the Local Storage file.</param>
        public LSAccessor(in string path)
        {
            _path = path;
            _cacheWriteStream = new LSStream(path);
        }

        /// <inheritdoc />
        public byte[] ReadAllBytes() => LSUtils.ReadAllBytes(_path);

        /// <inheritdoc />
        public Stream GetWriteStream() => _cacheWriteStream;

        /// <inheritdoc />
        public ValueTask DeleteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LSUtils.Delete(_path);
            return default;
        }
    }
}

#endif
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
    public class LSAccessor : FileAccessor
    {
        private readonly LSStream _cacheWriteStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="LSAccessor"/> class with the specified path.
        /// </summary>
        /// <param name="path">The key to the Local Storage file.</param>
        public LSAccessor(in string path) : base(path) => _cacheWriteStream = new LSStream(path);

        /// <inheritdoc />
        public override byte[] ReadAllBytes() => LSUtils.ReadAllBytes(SavePath);

        /// <inheritdoc />
        public override Stream GetWriteStream() => _cacheWriteStream;

        /// <inheritdoc />
        public override ValueTask DeleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LSUtils.Delete(SavePath);
            return default;
        }
    }
}

#endif
#if UNITY_WEBGL
#nullable enable

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AndanteTribe.IO.Unity
{
    public class LSAccessor : IFileAccessor
    {
        private LSStream? _cacheWriteStream;

        /// <inheritdoc />
        public byte[] ReadAllBytes(in string path) => LSUtils.ReadAllBytes(path);

        /// <inheritdoc />
        public Stream GetWriteStream(in string path)
        {
            if (_cacheWriteStream == null)
            {
                _cacheWriteStream = new LSStream(path);
            }
            else if (_cacheWriteStream.Path != path)
            {
                _cacheWriteStream.Dispose();
                _cacheWriteStream = new LSStream(path);
            }

            return _cacheWriteStream;
        }

        /// <inheritdoc />
        public ValueTask DeleteAsync(string path, CancellationToken cancellationToken)
        {
            LSUtils.Delete(path);
            return default;
        }
    }
}

#endif
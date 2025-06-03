#if UNITY_WEBGL
#nullable enable

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using UnityEngine.Pool;

namespace AndanteTribe.IO.Unity
{
    internal sealed class IDBValueTaskSourcePool : ObjectPool<IDBValueTaskSource>
    {
        public static readonly IDBValueTaskSourcePool Shared = new();

        private IDBValueTaskSourcePool() : base(static () => new IDBValueTaskSource(), static x => x.Reset())
        {
        }
    }

    internal sealed class IDBValueTaskSource : IValueTaskSource<(byte[], int)>, IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<(byte[], int)> _core = new()
        {
            RunContinuationsAsynchronously = false
        };

        public Memory<byte> Buffer { get; set; }

        public void SetResult() => _core.SetResult((Array.Empty<byte>(), 0));

        public unsafe void SetResult(IntPtr dataPtr, int length)
        {
            var dataSpan = new Span<byte>(dataPtr.ToPointer(), length);
            if (!Buffer.IsEmpty)
            {
                var size = Math.Min(length, Buffer.Length);
                dataSpan[..size].CopyTo(Buffer.Span);
                _core.SetResult((Array.Empty<byte>(), size));
            }
            else
            {
                _core.SetResult((dataSpan.ToArray(), length));
            }
        }

        public void SetException(Exception error) => _core.SetException(error);

        public void SetCanceled() => _core.SetException(new TaskCanceledException());

        public void Reset()
        {
            _core.Reset();
            Buffer = default;
        }

        public short Version => _core.Version;

        [DebuggerNonUserCode]
        public (byte[], int) GetResult(short token)
        {
            try
            {
                return _core.GetResult(token);
            }
            finally
            {
                IDBValueTaskSourcePool.Shared.Release(this);
            }
        }

        void IValueTaskSource.GetResult(short token)
        {
            try
            {
                _core.GetResult(token);
            }
            finally
            {
                IDBValueTaskSourcePool.Shared.Release(this);
            }
        }

        public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _core.OnCompleted(continuation, state, token, flags);
    }
}

#endif
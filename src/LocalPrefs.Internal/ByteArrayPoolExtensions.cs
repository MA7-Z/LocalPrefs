using System.Buffers;
using System.Runtime.CompilerServices;

namespace AndanteTribe.IO.Internal;

internal static class ArrayPoolExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PoolHandle<T> Rent<T>(this ArrayPool<T> pool, int size, out Span<T> bytes)
    {
        var array = pool.Rent(size);
        bytes = array.AsSpan(0, size);
        return new PoolHandle<T>(pool, array);
    }

    public readonly struct PoolHandle<T> : IDisposable
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;

        internal PoolHandle(ArrayPool<T> pool, T[] array)
        {
            _pool = pool;
            _array = array;
        }

        void IDisposable.Dispose() => _pool.Return(_array);
    }
}
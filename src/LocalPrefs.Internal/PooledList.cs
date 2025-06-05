using System.Buffers;

namespace AndanteTribe.IO.Internal;

internal ref struct PooledList<T>(int capacity) : IDisposable
{
    private T[] _array = ArrayPool<T>.Shared.Rent(capacity);
    public int Count { get; private set; }
    public int Capacity => _array.Length;
    public bool IsDisposed => Count == -1;

    public void Add(in T item)
    {
        ThrowIfDisposed();
        if (Count == Capacity)
        {
            var newArray = ArrayPool<T>.Shared.Rent(_array.Length * 2);
            _array.AsSpan().CopyTo(newArray);
            ArrayPool<T>.Shared.Return(_array);
            _array = newArray;
        }
        _array[Count++] = item;
    }

    public void AddRange(scoped ReadOnlySpan<T> items)
    {
        ThrowIfDisposed();

        if (Capacity < Count + items.Length)
        {
            var newSize = Capacity * 2;
            while (newSize < Count + items.Length)
            {
                newSize *= 2;
            }

            var newArray = ArrayPool<T>.Shared.Rent(newSize);
            _array.AsSpan(0, Count).CopyTo(newArray);
            ArrayPool<T>.Shared.Return(_array);
            _array = newArray;
        }
        items.CopyTo(_array.AsSpan(Count));
        Count += items.Length;
    }

    public void Clear()
    {
        ThrowIfDisposed();

        _array.AsSpan(0, Count).Clear();

        Count = 0;
    }

    public void Dispose()
    {
        ThrowIfDisposed();

        ArrayPool<T>.Shared.Return(_array);
        _array = [];
        Count = -1; // Mark as disposed
    }

    public ReadOnlySpan<T> AsSpan()
    {
        ThrowIfDisposed();
        return new ReadOnlySpan<T>(_array, 0, Count);
    }

    private void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(PooledList<T>));
        }
    }
}
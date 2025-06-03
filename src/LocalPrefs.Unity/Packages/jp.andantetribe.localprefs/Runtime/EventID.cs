#if UNITY_WEBGL
#nullable enable

using System;

namespace AndanteTribe.IO.Unity
{
    internal readonly struct EventID : IEquatable<EventID>, IComparable<EventID>
    {
        private static uint s_version;

        private readonly uint _value;
        private readonly IDBValueTaskSource? _source;

        public IDBValueTaskSource Source => _source ?? throw new InvalidOperationException();

        private EventID(uint value, IDBValueTaskSource? source = null)
        {
            _value = value;
            _source = source;
        }

        public bool Equals(EventID other)=> _value == other._value;

        public override bool Equals(object? obj) =>
            obj is EventID other && Equals(other);

        public override int GetHashCode() => (int)_value;

        public int CompareTo(EventID other) =>
            _value < other._value ? -1 : (_value > other._value ? 1 : 0);

        public static EventID GetNext(IDBValueTaskSource? source)
        {
            return new EventID(++s_version, source);
        }

        public static explicit operator uint(in EventID id) => id._value;
        public static explicit operator EventID(uint value) => new EventID(value);
    }
}

#endif
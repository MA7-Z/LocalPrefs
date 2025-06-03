using MessagePack;
using MessagePack.Formatters;

namespace AndanteTribe.IO.MessagePack;

internal sealed class IntIntValueTupleFormatter : IMessagePackFormatter<ValueTuple<int, int>>
{
    public void Serialize(ref MessagePackWriter writer, (int, int) value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.WriteInt32(value.Item1);
        writer.WriteInt32(value.Item2);
    }

    public (int, int) Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil)
        {
            throw new InvalidOperationException("If Data is Nil, ValueTuple is not null.");
        }

        var length = reader.ReadArrayHeader();
        if (length != 2)
        {
            throw new InvalidOperationException("Invalid ValueTuple length.");
        }

        var item1 = reader.ReadInt32();
        var item2 = reader.ReadInt32();

        return (item1, item2);
    }
}
using System.IO;

namespace OpenLore.network_manager.packets;

public class PacketReader
{
    private readonly BinaryReader Reader;
    public MemoryStream Stream;

    public PacketReader(byte[] bytes)
    {
        Stream = new MemoryStream(bytes);
        Reader = new BinaryReader(Stream);
    }

    public long Remaining()
    {
        return Stream.Length - Stream.Position;
    }

    public void Reset()
    {
        Stream.Position = 0;
    }

    public byte ReadByte()
    {
        return Reader.ReadByte();
    }

    public short ReadShortBE()
    {
        return (short)((ReadByte() << 8) | ReadByte());
    }

    public ushort ReadUShortBE()
    {
        return (ushort)((ReadByte() << 8) | ReadByte());
    }

    public ushort ReadUShortLE()
    {
        return (ushort)(ReadByte() | (ReadByte() << 8));
    }

    public int ReadIntBE()
    {
        return (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();
    }

    public uint ReadUIntBE()
    {
        return (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
    }

    public uint ReadUIntLE()
    {
        return (uint)(ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24));
    }

    public byte[] ReadBytes(long amount)
    {
        return Reader.ReadBytes((int)amount);
    }

    public string ReadString()
    {
        var s = "";
        byte c;
        while ((c = Reader.ReadByte()) != 0) s += (char)c;
        return s;
    }
}
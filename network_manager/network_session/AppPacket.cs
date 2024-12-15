using System;
using EQGodot.network_manager.packets;

namespace EQGodot.network_manager.network_session;

public abstract class AppPacket
{
    public PacketReader Reader;

    protected PacketWriter Writer;

    public AppPacket()
    {
        Writer = new PacketWriter();
    }

    public AppPacket(PacketReader reader)
    {
        Reader = reader;
        Read();
    }

    public byte[] ToBytes()
    {
        if (Writer == null) throw new NotImplementedException();
        Write();
        var result = Writer.ToBytes();
        Writer = new PacketWriter();
        return result;
    }

    public abstract void Write();
    public abstract void Read();
}
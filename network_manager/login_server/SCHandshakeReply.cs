using System;
using EQGodot.network_manager.network_session;
using EQGodot.network_manager.packets;

namespace EQGodot.network_manager.login_server;

public class SCHandshakeReply(PacketReader reader) : AppPacket(reader)
{
    public string Message;

    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        Reader.ReadUShortBE();
        Reader.ReadUShortBE();
        Reader.ReadUShortBE();
        Reader.ReadUIntLE();
        Reader.ReadUIntLE();
        Reader.ReadByte();
        Message = Reader.ReadString();
    }
}
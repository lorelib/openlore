using System;
using OpenLore.network_manager.network_session;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.login_server;

public class SCJoinServerReply(PacketReader reader) : AppPacket(reader)
{
    public uint AccountId;
    public ushort EQLSStr;
    public byte IsApproved;
    public ushort ServerId;

    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        AccountId = Reader.ReadUIntLE();
        Reader.ReadUIntLE();
        Reader.ReadUShortLE();
        IsApproved = Reader.ReadByte();
        EQLSStr = Reader.ReadUShortLE();
        Reader.ReadByte();
        Reader.ReadUShortLE();
        ServerId = Reader.ReadUShortLE();
    }
}
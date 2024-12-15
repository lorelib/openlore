using System;
using Godot;
using OpenLore.network_manager.network_session;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.login_server;

public class SCGetServerListReply(PacketReader reader) : AppPacket(reader)
{
    public uint Count;
    public EQServerDescription[] Servers;

    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        Reader.ReadUIntLE();
        Reader.ReadUIntLE();
        Reader.ReadUIntLE();
        Reader.ReadUIntLE();
        Count = Reader.ReadUIntLE();
        GD.Print($"Processing {Count} servers");
        Servers = new EQServerDescription[Count];
        for (uint i = 0; i < Count; i++)
            Servers[i] = new EQServerDescription
            {
                Address = Reader.ReadString(),
                ServerType = Reader.ReadUIntLE(),
                Id = Reader.ReadUIntLE(),
                LongName = Reader.ReadString(),
                Language = Reader.ReadString(),
                Region = Reader.ReadString(),
                Status = Reader.ReadUIntLE(),
                Players = Reader.ReadUIntLE()
            };
    }
}
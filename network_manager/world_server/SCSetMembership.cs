using System;
using EQGodot.network_manager.network_session;
using EQGodot.network_manager.packets;
using Godot;

namespace EQGodot.network_manager.world_server;

public class SCSetMembership(PacketReader reader) : AppPacket(reader)
{
    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        GD.Print($"SCSetMembership {Reader.ReadBytes(Reader.Remaining()).HexEncode()}");
    }
}
using System;
using Godot;
using OpenLore.network_manager.network_session;

namespace OpenLore.network_manager.login_server;

public class CSJoinServer(uint serverId) : AppPacket
{
    private readonly uint ServerId = serverId;

    public override void Write()
    {
        GD.Print($"Joining server {ServerId}");
        Writer.WriteShortLE(5);
        Writer.WriteIntBE(0);
        Writer.WriteIntBE(0);
        Writer.WriteUIntLE(ServerId);
    }

    public override void Read()
    {
        throw new NotImplementedException();
    }
}
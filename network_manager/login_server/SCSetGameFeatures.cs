using System;
using OpenLore.network_manager.network_session;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.login_server;

public class SCSetGameFeatures(PacketReader reader) : AppPacket(reader)
{
    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        // TODO: not sure if we care about this packet
    }
}
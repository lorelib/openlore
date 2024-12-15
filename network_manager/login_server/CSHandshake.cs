using System;
using OpenLore.network_manager.network_session;

namespace OpenLore.network_manager.login_server;

public class CSHandshake : AppPacket
{
    public override void Write()
    {
        Writer.WriteIntBE(0x0200);
        Writer.WriteShortBE(0);
        Writer.WriteIntBE(0);
        Writer.WriteShortBE(0x0B00);
    }

    public override void Read()
    {
        throw new NotImplementedException();
    }
}
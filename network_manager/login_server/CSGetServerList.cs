using System;
using EQGodot.network_manager.network_session;

namespace EQGodot.network_manager.login_server;

public class CSGetServerList : AppPacket
{
    public override void Write()
    {
        Writer.WriteUIntLE(0x04000000);
    }

    public override void Read()
    {
        throw new NotImplementedException();
    }
}
using System;
using OpenLore.network_manager.network_session;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.login_server;

public class SCPlayerLoginReply(PacketReader reader) : AppPacket(reader)
{
    public byte EQLSStr;
    public uint FailedAttempts;
    public byte[] KeyComponents;
    public uint LSID;
    public byte Message;

    public override void Write()
    {
        throw new NotImplementedException();
    }

    public override void Read()
    {
        Reader.ReadUIntBE();
        Reader.ReadUIntBE();
        Reader.ReadUShortBE();
        var contents = Reader.ReadBytes(Reader.Remaining());
        var empty = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        var des = new DESCrypto(empty, empty);
        var subReader = new PacketReader(des.Decrypt(contents));
        Message = subReader.ReadByte();
        EQLSStr = subReader.ReadByte();
        subReader.ReadUShortBE();
        subReader.ReadUIntLE();
        LSID = subReader.ReadUIntLE();
        KeyComponents = subReader.ReadBytes(10);
        FailedAttempts = subReader.ReadUIntLE();
    }
}
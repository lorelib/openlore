using System;
using Godot;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.network_session;

public partial class NetworkSession : PacketPeerUdp
{
    [Signal]
    public delegate void PacketReceivedEventHandler(byte[] packet);

    [Signal]
    public delegate void SessionEstablishedEventHandler();

    private uint ConnectCode;
    private byte CRCBytes;
    private bool Disconnected = true;
    private byte[] EncodeKey;
    private byte EncodePass2;
    private byte FilterMode;
    private byte[] FragmentContents;
    private uint FragmentLength;
    private uint FragmentOffset;
    private byte[][] FuturePackets = new byte[0x10000][];
    private ushort LastAckReceived = 65535;
    private ushort LastAckSent;
    private uint MaxPacketSize = 512;
    private uint ProtocolVersion = 2;
    private readonly byte[][] SentPackets = new byte[0x10000][];
    private ushort SequenceIn;
    private ushort SequenceOut;

    public new Error ConnectToHost(string host, int port)
    {
        ConnectCode = (uint)GlobalVariables.Rand.Next();
        var result = SetDestAddress(host, port);
        GD.Print($"Connected to {host}:{port} => {result}");
        Disconnected = false;
        var writer = new PacketWriter();
        writer.WriteShortBE(0x01);
        writer.WriteUIntBE(ProtocolVersion);
        writer.WriteUIntBE(ConnectCode);
        writer.WriteUIntBE(MaxPacketSize);
        SendPacket(writer);
        return result;
    }

    public void Process()
    {
        if (GetAvailablePacketCount() <= 0) return;
        var reader = GetPacketReader();
        ProcessPacket(reader, true);
    }

    public void Disconnect()
    {
        var writer = new PacketWriter();
        writer.WriteShortBE(0x05); // Opcode
        writer.WriteShortBE(0x05); // Unknown
        AppendCRC(writer);
        SendPacket(writer);
        Close();
        Disconnected = true;
    }

    private void ProcessPacket(PacketReader reader, bool topLevel)
    {
        var opcode = reader.ReadUShortBE();
        switch (opcode)
        {
            case 0x02:
                topLevel = false;
                break;
            case 0x06:
                topLevel = false;
                break;
        }

        // we only process CRC if the packet is top level/not combined
        if (topLevel)
        {
            reader.Reset();
            var size = (int)reader.Remaining();
            var contents = reader.ReadBytes(size - 2);
            var crc = reader.ReadUShortBE();
            var calculated = CRC.CalculateCRC16(contents, EncodeKey);
            if (calculated != crc)
            {
                GD.PrintErr(" CRC Invalid");
                return;
            }

            reader = new PacketReader(contents);
            opcode = reader.ReadUShortBE();
        }

        switch (opcode)
        {
            case 0x02:
                ConnectionEstablished(reader);
                break;
            case 0x03:
                ProcessCombined(reader);
                break;
            case 0x06:
                ProcessKeepAlive(reader);
                break;
            case 0x09:
                ProcessAppPacket(reader);
                break;
            case 0x0D:
                ProcessFragment(reader);
                break;
            case 0x15:
                ProcessAck(reader);
                break;
            default:
                GD.PrintErr($"Opcode {opcode:X04} not implemented");
                throw new NotImplementedException();
        }
    }

    private PacketReader GetPacketReader()
    {
        var packet = GetPacket();
        // GD.Print(" <== ", packet.HexEncode());
        return new PacketReader(packet);
    }

    private void SendPacket(PacketWriter writer)
    {
        var send = writer.ToBytes();
        // GD.Print(" ==> ", send.HexEncode());
        PutPacket(send);
    }

    public void SendAppPacket(AppPacket packet, OpcodeManager opcode)
    {
        var writer = new PacketWriter();
        writer.WriteShortBE(0x09);
        writer.WriteByte((byte)(SequenceOut >> 8));
        writer.WriteByte((byte)SequenceOut);
        var data = opcode.Encode(writer, packet);
        SentPackets[SequenceOut] = data;
        // GD.Print(" APP OUT ", data.HexEncode());
        AppendCRC(writer);
        SendPacket(writer);
        SequenceOut++;
    }

    private void SendAck(ushort sequence)
    {
        var writer = new PacketWriter();
        writer.WriteUShortBE(0x15);
        writer.WriteUShortBE(sequence);
        AppendCRC(writer);
        SendPacket(writer);
    }

    private void ConnectionEstablished(PacketReader reader)
    {
        var connectCode = reader.ReadUIntBE();
        if (connectCode != ConnectCode)
        {
            GD.PrintErr("Connection code does not match");
            return;
        }

        var encode = reader.ReadUIntBE();
        EncodeKey =
        [
            (byte)(encode >> 24),
            (byte)(encode >> 16),
            (byte)(encode >> 8),
            (byte)encode
        ];
        CRCBytes = reader.ReadByte();
        FilterMode = reader.ReadByte();
        EncodePass2 = reader.ReadByte();
        MaxPacketSize = reader.ReadUIntBE();
        EmitSignal(SignalName.SessionEstablished);
    }

    private void AppendCRC(PacketWriter writer)
    {
        if (CRCBytes == 2)
        {
            var crc = CRC.CalculateCRC16(writer.ToBytes(), EncodeKey);
            writer.WriteUShortBE(crc);
        }
    }

    private void ProcessKeepAlive(PacketReader reader)
    {
        var writer = new PacketWriter();
        writer.WriteUShortBE(0x6);
        writer.WriteUShortBE(reader.ReadUShortBE());
        SendPacket(writer);
    }

    private void ProcessCombined(PacketReader reader)
    {
        while (reader.Remaining() > 2)
        {
            var size = reader.ReadByte();
            var packet = reader.ReadBytes(size);
            var subreader = new PacketReader(packet);
            ProcessPacket(subreader, false);
        }
    }

    private void ProcessAck(PacketReader reader)
    {
        var sequence = reader.ReadUShortBE();
        if (SentPackets[sequence] != null) SentPackets[sequence] = null;
    }

    private void ProcessAppPacket(PacketReader reader)
    {
        var sequence = reader.ReadUShortBE();
        var packet = reader.ReadBytes((int)reader.Remaining());
        // GD.Print(" APP IN  ", packet.HexEncode());
        if (sequence == SequenceIn)
        {
            SendAck(sequence);
            SequenceIn++;
            EmitSignal(SignalName.PacketReceived, packet);
        }
    }

    private void ProcessFragment(PacketReader reader)
    {
        var sequence = reader.ReadUShortBE();
        if (sequence == SequenceIn)
        {
            SendAck(sequence);
            SequenceIn++;
        }
        else
        {
            return;
        }

        if (FragmentContents == null)
        {
            FragmentLength = reader.ReadUIntBE();
            FragmentContents = new byte[FragmentLength];
            FragmentOffset = 0;
        }

        var newFragment = reader.ReadBytes((int)reader.Remaining());
        Array.Copy(newFragment, 0, FragmentContents, FragmentOffset, newFragment.Length);
        FragmentOffset += (uint)newFragment.Length;
        if (FragmentOffset >= FragmentLength)
        {
            // GD.Print(" APP IN  ", FragmentContents.HexEncode());
            EmitSignal(SignalName.PacketReceived, FragmentContents);
            FragmentContents = null;
        }
    }
}
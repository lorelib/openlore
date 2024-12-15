using System;
using System.Collections.Generic;
using Godot;
using OpenLore.network_manager.packets;

namespace OpenLore.network_manager.network_session;

public class OpcodeManager
{
    private readonly Dictionary<Type, ushort> ClassToOpcode = [];
    private readonly Dictionary<ushort, Type> OpcodeToClass = [];

    public void Register<T>(ushort opcode) where T : AppPacket
    {
        Register(opcode, typeof(T));
    }

    private void Register(ushort opcode, Type type)
    {
        GD.Print($"Registered {opcode} of type {type}");
        OpcodeToClass[opcode] = type;
        ClassToOpcode[type] = opcode;
    }

    public AppPacket Decode(PacketReader reader)
    {
        var opcode = reader.ReadUShortLE();
        if (!OpcodeToClass.ContainsKey(opcode)) GD.PrintErr($"Unknown opcode received {opcode:X}");
        var type = OpcodeToClass[opcode];
        return (AppPacket)Activator.CreateInstance(type, reader);
    }

    public byte[] Encode(PacketWriter writer, AppPacket packet)
    {
        var opcode = ClassToOpcode[packet.GetType()];
        writer.WriteUShortLE(opcode);
        writer.WriteBytes(packet.ToBytes());
        // GD.Print($"Sending {writer.ToBytes().Length} {writer.ToBytes().HexEncode()}");
        return writer.ToBytes();
    }
}
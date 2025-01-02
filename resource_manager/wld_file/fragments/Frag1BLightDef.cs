using System.Collections.Generic;
using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag1BLightDef : WldFragment
{
    [Export] public int Flags;
    public List<Color> Colors = [];
    [Export] public uint FrameCurrentRef;
    [Export] public uint Sleep;
    public List<float> LightLevels = [];

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        Flags = Reader.ReadInt32();
        var frames = Reader.ReadInt32();
        if ((Flags & 0x01) != 0)
        {
            FrameCurrentRef = Reader.ReadUInt32();
        }
        
        if ((Flags & 0x02) != 0)
        {
            Sleep = Reader.ReadUInt32();
        }
        
        if ((Flags & 0x04) != 0)
        {
            for (var i = 0; i < frames; i++)
            {
                LightLevels.Add(Reader.ReadSingle());
            }
        }
        
        if ((Flags & 0x10) != 0)
        {
            for (var i = 0; i < frames; i++)
            {
                var red = Reader.ReadSingle();
                var green = Reader.ReadSingle();
                var blue = Reader.ReadSingle();
                Colors.Add(new Color(red, green, blue));
            }
        }
    }
}
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag26BlitSpriteDef : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag05SimpleSprite SimpleSprite;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        Flags = Reader.ReadInt32(); // flags? always 0
        SimpleSprite = wld.GetFragment(Reader.ReadInt32()) as Frag05SimpleSprite;
        var value12 = Reader.ReadInt32(); // always the same value. unlikely a float, or bytes. Not color.
    }
}
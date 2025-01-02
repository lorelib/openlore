using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
public partial class Frag27BlitSprite : WldFragment
{
    [Export] public Frag26BlitSpriteDef BlitSpriteDef;
    [Export] public int Flags;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        BlitSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag26BlitSpriteDef;
        Flags = Reader.ReadInt32();
        if (Flags != 0) GD.PrintErr($"Frag27BlitSprite flags: {Flags}");
    }
}
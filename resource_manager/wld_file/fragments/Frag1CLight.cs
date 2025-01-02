using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag1CLight : WldFragment
{
    [Export] public Frag1BLightDef LightSource { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        LightSource = wld.GetFragment(Reader.ReadInt32()) as Frag1BLightDef;
    }
}
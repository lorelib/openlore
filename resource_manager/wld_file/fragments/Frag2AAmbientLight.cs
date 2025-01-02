using Godot;
using Godot.Collections;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag2AAmbientLight : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag1CLight LightReference;
    [Export] public Array<int> Regions;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        LightReference = wld.GetFragment(Reader.ReadInt32()) as Frag1CLight;
        Flags = Reader.ReadInt32();
        var regionCount = Reader.ReadInt32();

        Regions = [];
        for (var i = 0; i < regionCount; ++i)
        {
            var regionId = Reader.ReadInt32();
            Regions.Add(regionId);
        }
    }
}
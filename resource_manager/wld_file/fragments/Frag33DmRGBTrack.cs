using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag33DmRGBTrack : WldFragment
{
    [Export] public Frag32DmRGBTrackDef VertexColors;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        VertexColors = wld.GetFragment(Reader.ReadInt32()) as Frag32DmRGBTrackDef;
    }
}
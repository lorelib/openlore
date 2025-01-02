using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag05SimpleSprite : WldFragment
{
    [Export] public int Flags;
    [Export] public Frag04SimpleSpriteDef SimpleSpriteDef { get; private set; }

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        SimpleSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag04SimpleSpriteDef;

        // Either 0 or 80 - unknown
        Flags = Reader.ReadInt32();
    }

    public List<string> GetAllBitmapNames()
    {
        return SimpleSpriteDef.BitmapNames.Select(bitmapName => bitmapName.Filename).ToList();
    }
}
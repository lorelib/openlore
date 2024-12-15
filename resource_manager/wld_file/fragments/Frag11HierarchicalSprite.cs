using System.IO;
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
internal partial class Frag11HierarchicalSprite : WldFragment
{
    [Export] public Frag10HierarchicalSpriteDef HierarchicalSpriteDef;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);

        var reader = new BinaryReader(new MemoryStream(data));

        // Reference is usually 0
        // Confirmed
        Name = wld.GetName(Reader.ReadInt32());
        HierarchicalSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag10HierarchicalSpriteDef;

        if (HierarchicalSpriteDef == null) GD.PrintErr("Bad skeleton hierarchy reference");

        var params1 = reader.ReadInt32();

        // Params are 0
        // Confirmed
        if (params1 != 0)
            GD.Print(
                $"Frag11HierarchicalSprite {index} -> {HierarchicalSpriteDef.Index} {wld.Name} {HierarchicalSpriteDef.Name}: has params1 {params1:X}");

        // Confirmed end
        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            // GD.Print($"Frag11HierarchicalSprite {index} -> {HierarchicalSpriteDef.Index} {wld.Name} {HierarchicalSpriteDef.Name}: has a remainder {reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)).HexEncode()}");
        }
    }
}
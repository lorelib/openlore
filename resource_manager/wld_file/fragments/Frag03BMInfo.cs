using Godot;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag03BMInfo : WldFragment
{
    [Export] public string Filename;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        // The client supports more than one bitmap reference but is never used
        var bitmapCount = Reader.ReadInt32();

        if (bitmapCount > 1) GD.PrintErr("BitmapName: Bitmap count exceeds 1.");

        int nameLength = Reader.ReadInt16();

        // Decode the bitmap name and trim the null character (c style strings)
        var nameBytes = Reader.ReadBytes(nameLength);
        Filename = WldStringDecoder.Decode(nameBytes);
        Filename = Filename.ToLower()[..(Filename.Length - 1)];
    }
}
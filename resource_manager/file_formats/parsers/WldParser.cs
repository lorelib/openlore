using System.IO;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.pack_file;
using OpenLore.resource_manager.wld_file;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.file_formats.parsers;

public static class WldParser
{
    public static WldFile Parse(PfsFile pfsFile, LoreResourceLoader loader)
    {
        var reader = new BinaryReader(new MemoryStream(pfsFile.FileBytes));

        var magicNumber = reader.ReadInt32();
        if (magicNumber != 0x54503D02)
        {
            GD.PrintErr("WldParser: Invalid magic number");
            return null;
        }

        var version = reader.ReadInt32();
        if (version != 0x00015500 && version != 0x1000C800)
        {
            GD.PrintErr("WldParser: Invalid version");
            return null;
        }

        var wldFile = new WldFile();
        if (version == 0x1000C800) wldFile.NewFormat = true;

        var fragmentCount = reader.ReadUInt32();
        var bspRegionCount = reader.ReadUInt32();
        var maxObjectBytes = reader.ReadInt32();
        var stringHashSize = reader.ReadInt32();
        var stringCount = reader.ReadInt32();

        var encoded = reader.ReadBytes(stringHashSize);
        wldFile.SetStrings(new WldStrings(encoded));

        for (var i = 1; i <= fragmentCount; ++i)
        {
            var fragSize = reader.ReadInt32();
            var fragType = reader.ReadInt32();
            var fragmentContents = reader.ReadBytes(fragSize);

            var newFragment = !WldFragmentBuilder.Fragments.TryGetValue(
                fragType,
                out var value
            )
                ? new FragXXFallback()
                : value();

            if (newFragment is FragXXFallback)
            {
                GD.PrintErr($"WldFile {pfsFile.Name}: Unhandled fragment type: {fragType:x}");
            }

            newFragment.Initialize(i, fragType, fragSize, fragmentContents, wldFile, loader);
            wldFile.AddFragment(newFragment);
        }

        return wldFile;
    }
}
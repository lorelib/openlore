using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag22Region : WldFragment
{
    [Export] public int Flags;
    [Export] public bool ContainsPolygons;
    [Export] public Frag36DmSpriteDef2 Mesh;
    [Export] public Frag29Zone RegionType;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        // Flags
        // 0x181 - Regions with polygons
        // 0x81 - Regions without
        // Bit 5 - PVS is WORDS
        // Bit 7 - PVS is bytes
        Flags = Reader.ReadInt32();

        if (Flags == 0x181) ContainsPolygons = true;

        // Always 0
        var unknown1 = Reader.ReadInt32();
        var data1Size = Reader.ReadInt32();
        var data2Size = Reader.ReadInt32();

        // Always 0
        var unknown2 = Reader.ReadInt32();
        var data3Size = Reader.ReadInt32();
        var data4Size = Reader.ReadInt32();

        // Always 0
        var unknown3 = Reader.ReadInt32();
        var data5Size = Reader.ReadInt32();
        var data6Size = Reader.ReadInt32();

        // Move past data1 and 2
        Reader.BaseStream.Position += 12 * data1Size + 12 * data2Size;

        // Move past data3
        for (var i = 0; i < data3Size; ++i)
        {
            var data3Flags = Reader.ReadInt32();
            var data3Size2 = Reader.ReadInt32();
            Reader.BaseStream.Position += data3Size2 * 4;
        }

        // Move past the data 4
        for (var i = 0; i < data4Size; ++i)
        {
            // Unhandled for now
        }

        // Move past the data5
        for (var i = 0; i < data5Size; i++) Reader.BaseStream.Position += 7 * 4;

        // Get the size of the PVS and allocate memory
        var pvsSize = Reader.ReadInt16();
        Reader.BaseStream.Position += pvsSize;

        // Move past the unknowns 
        var bytes = Reader.ReadUInt32();
        Reader.BaseStream.Position += 16;

        // Get the mesh reference index and link to it
        if (ContainsPolygons) Mesh = wld.GetFragment(Reader.ReadInt32()) as Frag36DmSpriteDef2;
    }

    public void SetRegionFlag(Frag29Zone bspRegionType)
    {
        RegionType = bspRegionType;
    }
}
using Godot;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag34ParticleCloudDef : WldFragment
{
    [Export] private Frag26BlitSpriteDef _particleSprite;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        //File.WriteAllBytes("ParticleClouds/" + Name, data);

        var flags = Reader.ReadInt32(); // always 4
        var value08 = Reader.ReadInt32(); // always 3
        var value12 = Reader.ReadInt32(); // Values are 1, 3, or 4
        var value16 = Reader.ReadByte();
        var value17 = Reader.ReadByte();
        var value18 = Reader.ReadByte();
        var value19 = Reader.ReadByte();
        var value20 = Reader.ReadInt32(); // 200, 30, particle count? 
        var value24 = Reader.ReadInt32(); // always 0
        var value28 = Reader.ReadInt32(); // always 0
        var value32 = Reader.ReadInt32(); // always 0
        var value36 = Reader.ReadInt32(); // always 0
        var value40 = Reader.ReadInt32(); // always 0
        var value44 = Reader.ReadSingle(); // confirmed float
        var value48 = Reader.ReadSingle(); // looks like a float
        var value52 = Reader.ReadInt32(); // looks like an int. numbers like 1000, 100, 750, 500, 1600, 2500.
        var value56 = Reader.ReadSingle(); // looks like a float. low numbers. 4, 5, 8, 10, 0
        var value60 = Reader.ReadSingle(); // float 0 or 1
        var value64 = Reader.ReadSingle(); // float 0 or -1
        var value68 = Reader.ReadSingle(); // float 0 or -1
        var value72 = Reader.ReadInt32(); // probably int 13, 15, 20, 600, 83? or bytes
        var value76 = Reader.ReadSingle(); // confirmed float 0.4, 0.5, 1.5, 0.1
        var value80 = Reader.ReadSingle(); // float 0.4, 1.9
        _particleSprite = wld.GetFragment(Reader.ReadInt32()) as Frag26BlitSpriteDef;
    }
}
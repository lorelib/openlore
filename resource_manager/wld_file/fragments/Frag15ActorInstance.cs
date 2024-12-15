using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag15ActorInstance : WldFragment
{
    [Export] public Frag32DmRGBTrackDef Colors;
    [Export] public string Unknown2;
    [Export] public string ObjectName;
    [Export] public Vector3 Position;
    [Export] public Vector3 Rotation;
    [Export] public Vector3 Scale;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        // in main zone, points to 0x16, in object wld, it contains the object name
        ObjectName = wld.GetName(Reader.ReadInt32());

        // Main zone: 0x2E, Objects: 0x32E
        var flags = Reader.ReadInt32();

        // Fragment reference
        // In main zone, it points to a 0x16 fragment
        // In objects.wld, it is 0
        Unknown2 = wld.GetName(Reader.ReadInt32());

        Position = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());

        // Rotation is strange. There is never any x rotation (roll)
        // The z rotation is negated
        var value0 = Reader.ReadSingle();
        var value1 = Reader.ReadSingle();
        var value2 = Reader.ReadSingle();

        var modifier = 1.0f / 512.0f * 360.0f;

        Rotation = new Vector3(0f, value1 * modifier, -(value0 * modifier));

        // Only scale y is used
        float scaleX, scaleY, scaleZ;
        scaleX = Reader.ReadSingle();
        scaleY = Reader.ReadSingle();
        scaleZ = Reader.ReadSingle();

        Scale = new Vector3(scaleX, scaleY, scaleZ);

        var colorFragment = Reader.ReadInt32();

        if (colorFragment != 0) Colors = (wld.GetFragment(colorFragment) as Frag33DmRGBTrack)?.VertexColors;
    }
}
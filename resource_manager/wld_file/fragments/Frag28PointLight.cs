using System.Linq;
using Godot;
using OpenLore.resource_manager.interfaces;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag28PointLight : WldFragment, IIntoGodotLight
{
    [Export] public Frag1CLight LightReference;
    [Export] public int Flags;
    [Export] public Vector3 Position;
    [Export] public float Radius;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        LightReference = wld.GetFragment(Reader.ReadInt32()) as Frag1CLight;
        Flags = Reader.ReadInt32();
        Position = new Vector3(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
        Radius = Reader.ReadSingle();
    }

    public Light3D ToGodotLight()
    {
        return new OmniLight3D()
        {
            Name = Name,
            Position = Position,
            OmniRange = Radius,
            OmniAttenuation = 0.25f,
            LightColor = LightReference.LightSource.Colors.First(),
            LightEnergy = 1,
        };
    }
}
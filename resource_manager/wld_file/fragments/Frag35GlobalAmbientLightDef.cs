using System;
using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

[GlobalClass]
public partial class Frag35GlobalAmbientLightDef : WldFragment
{
    [Export] public Color Color;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);

        // Color is in BGRA format. A is always 255.
        var colorBytes = BitConverter.GetBytes(Reader.ReadInt32());
        Color = new Color
        (
            colorBytes[2],
            colorBytes[1],
            colorBytes[0],
            colorBytes[3]
        );
    }
}
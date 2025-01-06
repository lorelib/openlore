using System;
using System.Collections.Generic;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.wld_file;

// Lantern Extractor class
public static class WldFragmentBuilder
{
    public static readonly Dictionary<int, Func<WldFragment>> Fragments = new()
    {
        { 0x00, () => new Frag00Default() },
        { 0x01, () => new Frag01DefaultPaletteFile() },
        { 0x02, () => new Frag02UserData() },
        { 0x03, () => new Frag03BmInfo() },
        { 0x04, () => new Frag04SimpleSpriteDef() },
        { 0x05, () => new Frag05SimpleSprite() },
        { 0x06, () => new Frag06Sprite2DDef() },
        { 0x07, () => new Frag07Sprite2D() },
        { 0x08, () => new Frag08Sprite3DDef() },
        { 0x09, () => new Frag09Sprite3D() },

        { 0x10, () => new Frag10HierarchicalSpriteDef() },
        { 0x11, () => new Frag11HierarchicalSprite() },
        { 0x12, () => new Frag12TrackDef() },
        { 0x13, () => new Frag13Track() },
        { 0x14, () => new Frag14ActorDef() },
        { 0x15, () => new Frag15ActorInstance() },
        { 0x16, () => new Frag16Sphere() },
        { 0x17, () => new Frag17PolyhedronDef() },
        { 0x18, () => new Frag18Polyhedron() },

        { 0x1B, () => new Frag1BLightDef() },
        { 0x1C, () => new Frag1CLight() },

        { 0x21, () => new Frag21WorldTree() },
        { 0x22, () => new Frag22Region() },

        { 0x26, () => new Frag26BlitSpriteDef() },
        { 0x27, () => new Frag27BlitSprite() },
        { 0x28, () => new Frag28PointLight() },
        { 0x29, () => new Frag29Zone() },
        { 0x2A, () => new Frag2AAmbientLight() },

        { 0x2C, () => new Frag2CDmSpriteDef() },
        { 0x2D, () => new Frag2DDmSprite() },
        { 0x2F, () => new Frag2FSomethingSomething() },
        
        { 0x30, () => new Frag30MaterialDef() },
        { 0x31, () => new Frag31MaterialPalette() },
        { 0x32, () => new Frag32DmRGBTrackDef() },
        { 0x33, () => new Frag33DmRGBTrack() },
        { 0x34, () => new Frag34ParticleCloudDef() },
        { 0x35, () => new Frag35GlobalAmbientLightDef() },
        { 0x36, () => new Frag36DmSpriteDef2() },
        { 0x37, () => new Frag37SomethingSomething() },
    };
}
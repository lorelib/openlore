using System;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.data_types;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag29Zone : WldFragment
{
    [Export] public ZonelineInfo Zoneline;
    [Export] public Array<RegionType> RegionTypes;
    [Export] public Array<int> BspRegionIndices;
    [Export] public string RegionString;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        var flags = Reader.ReadInt32();
        var regionCount = Reader.ReadInt32();

        BspRegionIndices = [];
        for (var i = 0; i < regionCount; ++i) BspRegionIndices.Add(Reader.ReadInt32());

        var regionStringSize = Reader.ReadInt32();

        var regionTypeString = regionStringSize == 0
            ? Name.ToLower()
            : WldStringDecoder.Decode(Reader.ReadBytes(regionStringSize)).ToLower();

        RegionTypes = [];

        if (regionTypeString.StartsWith("wtn_") || regionTypeString.StartsWith("wt_"))
        {
            // Ex: wt_zone, wtn_XXXXXX
            RegionTypes.Add(RegionType.Water);
        }
        else if (regionTypeString.StartsWith("wtntp"))
        {
            RegionTypes.Add(RegionType.Water);
            RegionTypes.Add(RegionType.Zoneline);
            DecodeZoneline(regionTypeString);
            RegionString = regionTypeString;
        }
        else if (regionTypeString.StartsWith("lan_") || regionTypeString.StartsWith("la_"))
        {
            RegionTypes.Add(RegionType.Lava);
        }
        else if (regionTypeString.StartsWith("lantp"))
        {
            // TODO: Figure this out - soldunga
            RegionTypes.Add(RegionType.Lava);
            RegionTypes.Add(RegionType.Zoneline);
            DecodeZoneline(regionTypeString);
            RegionString = regionTypeString;
        }
        else if (regionTypeString.StartsWith("drntp"))
        {
            RegionTypes.Add(RegionType.Zoneline);
            DecodeZoneline(regionTypeString);
            RegionString = regionTypeString;
        }
        else if (regionTypeString.StartsWith("drp_"))
        {
            RegionTypes.Add(RegionType.Pvp);
        }
        else if (regionTypeString.StartsWith("drn_"))
        {
            if (regionTypeString.Contains("_s_"))
                RegionTypes.Add(RegionType.Slippery);
            else
                RegionTypes.Add(RegionType.Unknown);
        }
        else if (regionTypeString.StartsWith("sln_"))
        {
            // gukbottom, cazicthule (gumdrop), runnyeye, velketor
            RegionTypes.Add(RegionType.WaterBlockLOS);
        }
        else if (regionTypeString.StartsWith("vwn_"))
        {
            RegionTypes.Add(RegionType.FreezingWater);
        }
        else
        {
            // All trilogy client region types are accounted for
            // This is here in case newer clients have newer types
            // tox - "wt_zone' - Possible legacy water zonepoint for boat?
            RegionTypes.Add(RegionType.Normal);
        }
    }

    private void DecodeZoneline(string regionTypeString)
    {
        Zoneline = new ZonelineInfo();

        // TODO: Verify this
        if (regionTypeString == "drntp_zone")
        {
            Zoneline.Type = ZonelineType.Reference;
            Zoneline.Index = 0;
            return;
        }

        var zoneId = Convert.ToInt32(regionTypeString.Substring(5, 5));

        if (zoneId == 255)
        {
            var zonelineId = Convert.ToInt32(regionTypeString.Substring(10, 6));
            Zoneline.Type = ZonelineType.Reference;
            Zoneline.Index = zonelineId;

            return;
        }

        Zoneline.ZoneIndex = zoneId;

        var x = GetValueFromRegionString(regionTypeString.Substring(10, 6));
        var y = GetValueFromRegionString(regionTypeString.Substring(16, 6));
        var z = GetValueFromRegionString(regionTypeString.Substring(22, 6));
        var rot = Convert.ToInt32(regionTypeString.Substring(28, 3));

        Zoneline.Type = ZonelineType.Absolute;
        Zoneline.Position = new Vector3(x, y, z);
        Zoneline.Heading = rot;
    }

    private float GetValueFromRegionString(string substring)
    {
        if (substring.StartsWith("-"))
            return -Convert.ToSingle(substring.Substring(1, 5));
        return Convert.ToSingle(substring);
    }
}
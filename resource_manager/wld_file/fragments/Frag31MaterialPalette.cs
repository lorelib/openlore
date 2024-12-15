using System;
using EQGodot.resource_manager.wld_file.data_types;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag31MaterialPalette : WldFragment
{
    [Export] public Array<Frag30MaterialDef> Materials;
    [Export] public Dictionary<string, Dictionary<int, Frag30MaterialDef>> Slots;
    [Export] public int VariantCount;
    [Export] public Array<Frag30MaterialDef> AdditionalMaterials;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());

        Materials = [];

        var flags = Reader.ReadInt32();
        var materialCount = Reader.ReadInt32();

        for (var i = 0; i < materialCount; ++i)
        {
            var material = wld.GetFragment(Reader.ReadInt32()) as Frag30MaterialDef;

            if (material == null)
            {
                GD.PrintErr("Unable to get material reference for fragment id");
                continue;
            }

            Materials.Add(material);

            // Materials that are referenced in the MaterialList are already handled
            material.IsHandled = true;
        }
    }

    public void BuildSlotMapping()
    {
        Slots = [];

        if (Materials == null || Materials.Count == 0) return;

        foreach (var material in Materials)
        {
            var character = string.Empty;
            var skinId = string.Empty;
            var partName = string.Empty;

            ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

            var key = character + "_" + partName;
            Slots[key] = [];
        }

        AdditionalMaterials = [];
    }

    private static void ParseCharacterSkin(string materialName, out string character, out string skinId,
        out string partName)
    {
        if (materialName.Length != 9)
        {
            character = string.Empty;
            skinId = string.Empty;
            partName = string.Empty;
            return;
        }

        character = materialName.Substring(0, 3);
        skinId = materialName.Substring(5, 2);
        partName = materialName.Substring(3, 2) + materialName.Substring(7, 2);
    }

    public static string GetMaterialPrefix(ShaderTypeEnumType shaderType)
    {
        switch (shaderType)
        {
            case ShaderTypeEnumType.Diffuse:
                return "d_";
            case ShaderTypeEnumType.Invisible:
                return "i_";
            case ShaderTypeEnumType.Boundary:
                return "b_";
            case ShaderTypeEnumType.Transparent25:
                return "t25_";
            case ShaderTypeEnumType.Transparent50:
                return "t50_";
            case ShaderTypeEnumType.Transparent75:
                return "t75_";
            case ShaderTypeEnumType.TransparentAdditive:
                return "ta_";
            case ShaderTypeEnumType.TransparentAdditiveUnlit:
                return "tau_";
            case ShaderTypeEnumType.TransparentMasked:
                return "tm_";
            case ShaderTypeEnumType.DiffuseSkydome:
                return "ds_";
            case ShaderTypeEnumType.TransparentSkydome:
                return "ts_";
            case ShaderTypeEnumType.TransparentAdditiveUnlitSkydome:
                return "taus_";
            default:
                return "d_";
        }
    }

    public void AddVariant(Frag30MaterialDef material)
    {
        var character = string.Empty;
        var skinId = string.Empty;
        var partName = string.Empty;
        ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

        var key = character + "_" + partName;

        if (!Slots.ContainsKey(key)) Slots[key] = [];

        var skinIdNumber = Convert.ToInt32(skinId);
        Slots[key][skinIdNumber] = material;

        if (skinIdNumber > VariantCount) VariantCount = skinIdNumber;

        AdditionalMaterials.Add(material);
    }

    public Array<Frag30MaterialDef> GetMaterialVariants(Frag30MaterialDef material)
    {
        Array<Frag30MaterialDef> additionalSkins = [];

        if (Slots == null) return additionalSkins;

        var character = string.Empty;
        var skinId = string.Empty;
        var partName = string.Empty;
        ParseCharacterSkin(FragmentNameCleaner.CleanName(material), out character, out skinId, out partName);

        var key = character + "_" + partName;

        if (!Slots.ContainsKey(key)) return additionalSkins;

        var variants = Slots[key];
        for (var i = 0; i < VariantCount; ++i)
        {
            if (!variants.ContainsKey(i + 1))
            {
                additionalSkins.Add(null);
                continue;
            }

            additionalSkins.Add(variants[i + 1]);
        }

        return additionalSkins;
    }
}
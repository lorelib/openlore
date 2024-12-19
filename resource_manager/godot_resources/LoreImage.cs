using Godot;

namespace OpenLore.resource_manager.godot_resources;

[GlobalClass]
public partial class LoreImage : Image
{
    private Image _withTransparency;

    public LoreImage()
    {
    }

    public LoreImage(string name, Image image)
    {
        ResourceName = name;
        SetData(GetWidth(), GetHeight(), HasMipmaps(), GetFormat(), GetData());
        foreach (var metaName in GetMetaList())
        {
            _withTransparency.SetMeta(metaName, GetMeta(metaName));
        }
    }

    public Image GetTransparent()
    {
        if (_withTransparency != null) return _withTransparency;
        
        if (HasMeta("palette_present") && (bool)GetMeta("palette_present") == false)
            return this;
        
        if ((string)GetMeta("original_file_type") != "BMP") return this;
        
        var a = (int)GetMeta("transparent_a");
        var r = (int)GetMeta("transparent_r");
        var g = (int)GetMeta("transparent_g");
        var b = (int)GetMeta("transparent_b");

        var data = GetData();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i] != r || data[i + 1] != g || data[i + 2] != b) continue;
            data[i + 0] = 0;
            data[i + 1] = 0;
            data[i + 2] = 0;
            data[i + 3] = 0;
        }

        _withTransparency = CreateFromData(GetWidth(), GetHeight(), false, Format.Rgba8, data);
        _withTransparency.SetMeta("transparency_applied", true);
        _withTransparency.ResourceName = ResourceName;
        foreach (var metaName in GetMetaList())
        {
            _withTransparency.SetMeta(metaName, GetMeta(metaName));
        }

        return _withTransparency;
    }
}
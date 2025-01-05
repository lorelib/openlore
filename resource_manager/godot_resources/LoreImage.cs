using Godot;
using OpenLore.resource_manager.file_formats.converters;
using OpenLore.resource_manager.pack_file;

namespace OpenLore.resource_manager.godot_resources;

[GlobalClass]
public partial class LoreImage : Resource
{
    [Export] private Image _baseline;
    [Export] private Image _withTransparency;

    public LoreImage()
    {
    }

    public LoreImage(PfsFile pfsFile)
    {
        ResourceName = pfsFile.Name;

        if (pfsFile.FileBytes[0] == 'D' &&
            pfsFile.FileBytes[1] == 'D' && pfsFile.FileBytes[2] == 'S')
        {
            _baseline = DdsConverter.FromFile(pfsFile);
        }

        if (pfsFile.FileBytes[0] == 'B' && pfsFile.FileBytes[1] == 'M')
        {
            _baseline = BmpConverter.FromFile(pfsFile);
        }

        SetMeta("pfs_file_name", pfsFile.ArchiveName);
        SetMeta("original_file_name", pfsFile.Name);
        SetMeta("original_file_type", _baseline.GetMeta("original_file_type"));
    }

    public static implicit operator Image(LoreImage image)
    {
        return image._baseline;
    }

    public Image Transparent()
    {
        if (_withTransparency != null)
            return _withTransparency;
        
        if ((string)GetMeta("original_file_type") != "BMP")
            return _baseline;
        
        if (!HasMeta("palette_present"))
            return _baseline;
        
        if ((bool)GetMeta("palette_present") == false)
            return _baseline;

        var a = (int)GetMeta("transparent_a");
        var r = (int)GetMeta("transparent_r");
        var g = (int)GetMeta("transparent_g");
        var b = (int)GetMeta("transparent_b");

        var data = _baseline.GetData();
        for (var i = 0; i < data.Length; i += 4)
        {
            if (data[i] != r || data[i + 1] != g || data[i + 2] != b) continue;
            data[i + 0] = 0;
            data[i + 1] = 0;
            data[i + 2] = 0;
            data[i + 3] = 0;
        }

        _withTransparency =
            Image.CreateFromData(_baseline.GetWidth(), _baseline.GetHeight(), false, Image.Format.Rgba8, data);
        _withTransparency.SetMeta("transparency_applied", true);
        _withTransparency.ResourceName = ResourceName;
        foreach (var metaName in GetMetaList())
        {
            _withTransparency.SetMeta(metaName, GetMeta(metaName));
        }

        return _withTransparency;
    }
}
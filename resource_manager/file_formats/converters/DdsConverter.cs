using System;
using System.Diagnostics;
using Godot;
using OpenLore.resource_manager.pack_file;
using Pfim;

namespace OpenLore.resource_manager.file_formats.converters;

public static class DdsConverter
{
    public static Image FromFile(PfsFile pfsFile)
    {
        try
        {
            var dds = Dds.Create(pfsFile.FileBytes, new PfimConfig());
            var data = dds.Data;
            Debug.Assert(data.Length % 4 == 0);
            for (var j = 0; j < data.Length / 4; j++)
            {
                var b = dds.Data[j * 4 + 0];
                var g = dds.Data[j * 4 + 1];
                var r = dds.Data[j * 4 + 2];
                var a = dds.Data[j * 4 + 3];
                dds.Data[j * 4 + 0] = r;
                dds.Data[j * 4 + 1] = g;
                dds.Data[j * 4 + 2] = b;
                dds.Data[j * 4 + 3] = a;
            }

            var image = Image.CreateFromData(dds.Width, dds.Height, dds.MipMaps.Length > 1, Image.Format.Rgba8,
                dds.Data);
            image.SetMeta("pfs_file_name", pfsFile.ArchiveName);
            image.SetMeta("original_file_name", pfsFile.Name);
            image.SetMeta("original_file_type", "DDS");
            return image;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessDDSImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }

        return null;
    }
}
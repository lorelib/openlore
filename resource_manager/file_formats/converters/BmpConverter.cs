using System;
using System.IO;
using Godot;
using OpenLore.resource_manager.pack_file;

namespace OpenLore.resource_manager.file_formats.converters;

public static class BmpConverter
{
    public static Image FromFile(PfsFile pfsFile)
    {
        try
        {
            var image = new Image();
            var error = image.LoadBmpFromBuffer(pfsFile.FileBytes);
            if (error != Error.Ok)
            {
                GD.PrintErr($"BmpConverter: Exception while processing image from {pfsFile.Name}: {error}");
                return null;
            }

            image.FlipY();
            image.SetMeta("pfs_file_name", pfsFile.ArchiveName);
            image.SetMeta("original_file_name", pfsFile.Name);
            image.SetMeta("original_file_type", "BMP");

            var reader = new BinaryReader(new MemoryStream(pfsFile.FileBytes));
            reader.ReadBytes(14); // Skip standard header
            var dibHeaderSize = reader.ReadUInt32();
            if (dibHeaderSize == 40)
            {
                reader.ReadBytes(10); // Skip bytes in BITMAPINFOHEADER
                var colorDepth = reader.ReadUInt16();
                var compressionMode = reader.ReadUInt32();
                if (compressionMode == 0 && colorDepth == 8)
                {
                    reader.ReadBytes(20);
                    image.SetMeta("palette_present", true);
                    image.SetMeta("transparent_b", reader.ReadByte());
                    image.SetMeta("transparent_g", reader.ReadByte());
                    image.SetMeta("transparent_r", reader.ReadByte());
                    image.SetMeta("transparent_a", reader.ReadByte());
                }
                else
                {
                    GD.PrintErr(
                        $"BmpConverter: Compression mode {compressionMode} with colorDepth {colorDepth} is not supported");
                    image.SetMeta("palette_present", false);
                }
            }
            else
            {
                image.SetMeta("palette_present", false);
            }

            image.ResourceName = pfsFile.Name;
            return image;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessBMPImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }

        return null;
    }
}
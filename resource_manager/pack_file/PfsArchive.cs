using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.wld_file;
using Pfim;

namespace OpenLore.resource_manager.pack_file;

[GlobalClass]
public partial class PfsArchive : Resource
{
    [Export] public string LoadedPath;
    [Export] public Array<Resource> Files = [];
    [Export] public Godot.Collections.Dictionary<string, WldFile> WldFiles = [];
    [Export] public PfsArchiveType Type;

    public async Task<Godot.Collections.Dictionary<string, LoreImage>> ProcessImages()
    {
        List<Task<(string, LoreImage)>> tasks = [];
        for (var i = 0; i < Files.Count; i++)
        {
            if (Files[i] is not PFSFile file)
            {
                GD.PrintErr($"File is not PFSFile on index {i}");
                continue;
            }

            if (file.FileBytes[0] == 'D' &&
                file.FileBytes[1] == 'D' && file.FileBytes[2] == 'S')
            {
                var ddsTask = Task.Run(async () => await ProcessDdsImage(file));
                tasks.Add(ddsTask);
            }

            if (file.FileBytes[0] == 'B' && file.FileBytes[1] == 'M')
            {
                var bmpTask = Task.Run(async () => await ProcessBmpImage(file));
                tasks.Add(bmpTask);
            }
        }

        var images = await Task.WhenAll([..tasks]);
        Godot.Collections.Dictionary<string, LoreImage> result = [];
        foreach (var im in images)
        {
            result.Add(im.Item1, im.Item2);
        }

        return result;
    }

    private async Task<(string, LoreImage)> ProcessDdsImage(PFSFile pfsFile)
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

            try
            {
                var image = new LoreImage(pfsFile.Name, Image.CreateFromData(dds.Width, dds.Height,
                    dds.MipMaps.Length > 1, Image.Format.Rgba8, dds.Data));
                image.SetMeta("pfs_file_name", pfsFile.ArchiveName);
                image.SetMeta("original_file_name", pfsFile.Name);
                image.SetMeta("original_file_type", "DDS");
                return (pfsFile.Name, image);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ProcessDDSImage: Exception while creating image from {pfsFile.Name}: {ex}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessDDSImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }

        return (pfsFile.Name, null);
    }

    private async Task<(string, LoreImage)> ProcessBmpImage(PFSFile pfsFile)
    {
        try
        {
            var image = new LoreImage() { ResourceName = pfsFile.Name };
            var error = image.LoadBmpFromBuffer(pfsFile.FileBytes);
            if (error != Error.Ok)
            {
                GD.PrintErr($"ProcessBMPImage: Exception while processing image from {pfsFile.Name}: {error}");
                return (pfsFile.Name, null);
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
                        $"ProcessBMPImage: Compression mode {compressionMode} with colorDepth {colorDepth} is not supported");
                    image.SetMeta("palette_present", false);
                }
            }
            else
            {
                image.SetMeta("palette_present", false);
            }

            image.ResourceName = pfsFile.Name;
            return (pfsFile.Name, image);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ProcessBMPImage: Exception while processing image from {pfsFile.Name}: {ex}");
        }

        return (pfsFile.Name, null);
    }

    public Godot.Collections.Dictionary<string, WldFile> ProcessWldFiles(EqResourceLoader loader)
    {
        List<Task> wldHandles = [];
        for (var i = 0; i < Files.Count; i++)
            if (Files[i] is PFSFile pfsFile)
                if (pfsFile.Name.EndsWith(".wld"))
                    try
                    {
                        var index = i;
                        var pfs = pfsFile;
                        wldHandles.Add(Task.Run(() => ProcessWldResource(pfs, index, loader)));
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr("Exception while processing ", pfsFile.Name, " ", ex);
                    }

        Task.WaitAll([.. wldHandles]);

        return WldFiles;
    }

    private void ProcessWldResource(PFSFile pfsFile, int index, EqResourceLoader loader)
    {
        var wld = new WldFile(pfsFile, loader);
        Files[index] = wld;
        WldFiles[pfsFile.Name] = wld;
    }
}
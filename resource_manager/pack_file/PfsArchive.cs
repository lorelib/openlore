using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.file_formats.parsers;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.wld_file;

namespace OpenLore.resource_manager.pack_file;

[GlobalClass]
public partial class PfsArchive : Resource
{
    [Export] public string LoadedPath;
    [Export] public Array<Resource> Files = [];
    [Export] public Godot.Collections.Dictionary<string, WldFile> WldFiles = [];

    public async Task<Godot.Collections.Dictionary<string, LoreImage>> ProcessImages()
    {
        List<Task<(string, LoreImage)>> tasks = [];
        for (var i = 0; i < Files.Count; i++)
        {
            if (Files[i] is not PfsFile file)
            {
                GD.PrintErr($"File is not PFSFile on index {i}");
                continue;
            }

            if ((file.FileBytes[0] == 'D' && file.FileBytes[1] == 'D' && file.FileBytes[2] == 'S')
                || (file.FileBytes[0] == 'B' && file.FileBytes[1] == 'M'))
            {
                var task = Task.Run(() => (file.Name, new LoreImage(file)));
                tasks.Add(task);
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

    public Godot.Collections.Dictionary<string, WldFile> ProcessWldFiles(LoreResourceLoader loader)
    {
        List<Task> wldHandles = [];
        for (var i = 0; i < Files.Count; i++)
            if (Files[i] is PfsFile pfsFile)
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

    private void ProcessWldResource(PfsFile pfsFile, int index, LoreResourceLoader loader)
    {
        var wld = WldParser.Parse(pfsFile, loader);
        wld.Process();
        Files[index] = wld;
        WldFiles[pfsFile.Name] = wld;
    }
}
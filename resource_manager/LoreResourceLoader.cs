using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenLore.GameController;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.pack_file;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class LoreResourceLoader : Node
{
    [Export] public string FileName;
    [Export] public string RequestedFileName;
    [Export] public bool Loaded;
    [Export] public bool Failed;
    [Export] public int AgeCounter;

    [Export] public Godot.Collections.Dictionary<string, LoreImage> Images = [];
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    [Export] public Godot.Collections.Dictionary<int, ArrayMesh> Meshes = [];
    [Export] public Godot.Collections.Dictionary<string, Resource> ActorDefs = [];
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];

    private Task<bool> _task;

    public override void _Ready()
    {
        _task = Task.Run(async () => await LoadFile(RequestedFileName));
    }

    public override void _Process(double delta)
    {
        if (Loaded || _task == null || !_task.IsCompleted) return;

        if (_task.IsFaulted || _task.Result == false)
        {
            Failed = true;
        }

        Loaded = true;
        _task = null;

        GD.Print($"LoreResourceLoader: completed processing {Name} age {AgeCounter} failed {Failed}");
    }

    public LoreImage GetImage(string imageName)
    {
        return Failed ? null : Images.GetValueOrDefault(imageName);
    }

    public Resource GetActor(string tag)
    {
        return Failed ? null : ActorDefs.GetValueOrDefault(tag);
    }

    public Dictionary<(string, string), ActorSkeletonPath> GetAnimationsFor(string tag)
    {
        Dictionary<(string, string), ActorSkeletonPath> result = [];
        if (Failed) return result;
        foreach (var animation in ExtraAnimations.Values)
        {
            if (animation.ActorName != tag) continue;
            result[(animation.AnimationName, animation.BoneName)] = animation;
        }

        return result;
    }

    private async Task<bool> LoadFile(string name)
    {
        GD.Print($"LoreResourceLoader: requesting {name} at age {AgeCounter}");
        var assetPath = GameConfig.Instance.AssetPath;
        FileName = await TestFiles([$"{assetPath}/{name}", $"{assetPath}/{name}.eqg", $"{assetPath}/{name}.s3d"]);
        if (FileName == null)
        {
            GD.PrintErr($"LoreResourceLoader: {name} doesn't exist!");
            return false;
        }

        var archive = await PackFileParser.Load(FileName);
        Images = await archive.ProcessImages();

        if (FileName.EndsWith(".s3d"))
        {
            return await ProcessS3DFile(archive);
        }

        if (FileName.EndsWith(".eqg"))
        {
            return await ProcessEQGFile(archive);
        }

        GD.PrintErr($"LoreResourceLoader: {name} is an eqg and unsupported!");
        return false;
    }

    private static async Task<string> TestFiles(string[] names)
    {
        List<Task<string>> tasks = [];
        tasks.AddRange(names.Select(name => Task.Run(() => File.Exists(name) ? name : null)));

        var results = await Task.WhenAll([..tasks]);
        return results.FirstOrDefault(result => result != null);
    }

    private async Task<bool> ProcessS3DFile(PfsArchive archive)
    {
        GD.Print($"LoreResourceLoader: processing S3D {FileName} - images {Images.Count}");
        var wldFiles = archive.ProcessWldFiles(this);
        if (wldFiles.TryGetValue("objects.wld", out var objectsWld))
        {
            GD.PrintErr($"LoreResourceLoader: {Name} contains objects.wld but is unsupported: {objectsWld}");
        }

        if (wldFiles.TryGetValue("lights.wld", out var lightsWld))
        {
            GD.PrintErr($"LoreResourceLoader: {Name} contains lights.wld but is unsupported: {lightsWld}");
        }

        if (wldFiles.TryGetValue($"{Name}.wld", out var mainWld))
        {
            ActorDefs = mainWld.ActorDefs;
            ExtraAnimations = mainWld.ExtraAnimations;
        }

        return true;
    }

    private async Task<bool> ProcessEQGFile(PfsArchive archive)
    {
        GD.Print($"LoreResourceLoader: processing EQG {FileName} - images {Images.Count}");
        return true;
    }
}
using Godot;

namespace EQGodot.addons.pfs_loader.Importers;

[Tool]
[GlobalClass]
public partial class EqEditorSceneImporter : EditorSceneFormatImporter
{
    public override string[] _GetExtensions()
    {
        // GD.Print("EQEditorSceneImporter::_GetExtensions()");
        return ["s3d"];
    }

    public override Variant _GetOptionVisibility(string path, bool forAnimation, string option)
    {
        // GD.Print("EQEditorSceneImporter::_GetOptionVisibility()");
        return base._GetOptionVisibility(path, forAnimation, option);
    }

    public override uint _GetImportFlags()
    {
        // GD.Print("EQEditorSceneImporter::_GetImportFlags()");
        return (uint)ImportScene | (uint)ImportAnimation;
    }

    public override GodotObject _ImportScene(string path, uint flags, Godot.Collections.Dictionary options)
    {
        // GD.Print("EQEditorSceneImporter::_ImportScene()");
        // var resource = PackFileParser.Load(path);
        var root = new Node3D();
        root.Name = path;
        // var extraAnimations = new Dictionary<string, ActorSkeletonPath>();
        // var actordefs = new Dictionary<string, HierarchicalActorDefinition>();
        // // GD.Print(resource);
        // foreach (var wld in resource.WldFiles.Values)
        // {
        //     foreach (var actorDef in wld.ActorDefs.Values)
        //     {
        //         if (actorDef is not HierarchicalActorDefinition act) continue;
        //         var name = act.ResourceName;
        //         actordefs[name] = act;
        //         GD.Print($"Loaded {name}");
        //     }
        //
        //     foreach (var animation in wld.ExtraAnimations.Values)
        //     {
        //         if (extraAnimations.TryAdd(animation.Name, animation)) continue;
        //         GD.Print($"{animation.Name} already exists");
        //     }
        // }
        //
        // foreach (var animation in extraAnimations.Values)
        // {
        //     var animationName = animation.Name.Substr(0, 3);
        //     var actorName = animation.Name.Substr(3, 3);
        //     var boneName = animation.Name[6..].Replace("_track", "");
        //     if (boneName == "") boneName = "root";
        //
        //     if (!actordefs.TryGetValue(actorName, out var actor)) continue;
        //     if (actor.BonesByName.TryGetValue(boneName, out var bone)) bone.AnimationTracks[animationName] = animation;
        // }
        //
        // foreach (var actor in actordefs.Values.OfType<HierarchicalActorDefinition>())
        // {
        //     // var instance = actor.InstantiateCharacter();
        //     // instance.Owner = root;
        //     // root.AddChild(instance);
        // }

        return root;
    }
}
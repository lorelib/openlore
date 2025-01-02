using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenLore.resource_manager.godot_resources;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class ResourceManager : Node
{
    // Where instantiation of objects in the zone is going to happen
    private Node3D _sceneRoot;
    private LoreGlobalResources _globalResources;
    private LoreZoneResources _zoneResources;

    public override void _Ready()
    {
        GD.Print("Starting Resource Manager!");
        _sceneRoot = GetNode<Node3D>("SceneRoot");
        _globalResources = GetNode<LoreGlobalResources>("GlobalResources");
        _zoneResources = GetNode<LoreZoneResources>("ZoneResources");
    }

    public override void _Process(double delta)
    {
    }

    public Image GetImage(string imageName)
    {
        return _zoneResources.GetImage(imageName) ?? _globalResources.GetImage(imageName);
    }

    public HierarchicalActorInstance InstantiateCharacter(string tag)
    {
        return InstantiateHierarchicalInto(tag, _sceneRoot);
    }

    public HierarchicalActorInstance InstantiateHierarchicalInto(string tag, Node where)
    {
        GD.Print($"Instantiating character: {tag}");
        var actor = _zoneResources.GetActor(tag) ?? _globalResources.GetActor(tag);
        if (actor == null) GD.Print($"Instantiating character: {tag} not found");
        if (actor is not HierarchicalActorDefinition hierarchicalActor) return null;
        GD.Print($"Instantiating Hierarchical Actor {hierarchicalActor.ResourceName} on {where.Name}");
        var character = hierarchicalActor.InstantiateCharacter(this);
        where.AddChild(character);
        return character;
    }

    public List<ActorSkeletonPath> GetAnimationsFor(string actorName)
    {
        GD.Print($"Getting animations for {actorName}");
        var result = _zoneResources.GetAnimationsFor(actorName);
        foreach (var animation in _globalResources.GetAnimationsFor(actorName))
        {
            result[animation.Key] = animation.Value;
        }

        GD.Print($"Got {result.Count} for {actorName}");
        return result.Values.ToList();
    }

    //             case PfsArchiveType.Zone:
    //                 var lights = archive.WldFiles.GetValueOrDefault("lights.wld");
    //                 if (lights == null)
    //                 {
    //                     GD.PrintErr("No lights.wld found");
    //                     continue;
    //                 }
    //                 GD.Print($"Processing lights");
    //                 _activeZoneLights = lights.ZoneLights;
    //                 
    //                 var objects = archive.WldFiles.GetValueOrDefault("objects.wld");
    //                 if (objects == null)
    //                 {
    //                     GD.PrintErr("No objects.wld found");
    //                     continue;
    //                 }
    //                 GD.Print($"Processing objects");
    //                 // TODO: Process objects.wld
    //                 
    //                 foreach (var wldFile in archive.WldFiles)
    //                 {
    //                     if (wldFile.Value.WorldTree != null)
    //                     {
    //                         GD.Print($"OnPFSArchiveLoaded: activating zone {wldFile.Key}");
    //                         _activeZone = wldFile.Value.WorldTree;
    //                     }
    //                 }
    //
    //
    // public void InstantiateZone()
    // {
    //     if (_activeZone == null) return;
    //     GD.Print($"Instantiating zone {_activeZone}");
    //     _sceneRoot.AddChild(_activeZone.ToGodotZone());
    //     foreach (var l in _activeZoneLights)
    //     {
    //         _sceneRoot.AddChild(l.ToGodotLight());
    //     }
    // }
    //
    
    public void LoadZone(string zoneName)
    {
        _zoneResources.StartLoadingZone(zoneName);
    }
}
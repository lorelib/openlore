using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.godot_resources.converters;
using OpenLore.resource_manager.pack_file;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.wld_file;

// Lantern Extractor class
public partial class WldFile : Resource
{
    [Export] public string Name;
    [Export] public bool NewFormat;
    [Export] public Godot.Collections.Dictionary<string, Array<Resource>> Resources = [];

    [Export] public Godot.Collections.Dictionary<string, Resource> ActorDefs = [];
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    private Godot.Collections.Dictionary<int, ArrayMesh> _newMeshes = [];
    [Export] public Array<Frag28PointLight> ZoneLights;

    [Export] public Frag21WorldTree WorldTree;

    private readonly List<WldFragment> _fragments = [];
    private readonly System.Collections.Generic.Dictionary<Type, List<WldFragment>> _fragmentTypeDictionary = [];
    private Godot.Collections.Dictionary<int, byte[]> _fragmentContents = [];
    private Godot.Collections.Dictionary<string, WldFragment> _fragmentNameDictionary = [];
    private Godot.Collections.Dictionary<int, int> _fragmentTypes = [];
    private WldStrings _strings;

    public WldFile()
    {
        _fragments =
        [
            new FragXXFallback()
        ];
    }

    public void SetStrings(WldStrings strings)
    {
        _strings = strings;
    }

    public void AddFragment(WldFragment fragment)
    {
        _fragments.Add(fragment);

        var type = fragment.GetType();
        if (!_fragmentTypeDictionary.ContainsKey(type))
            _fragmentTypeDictionary[type] = [];

        if (
            !string.IsNullOrEmpty(fragment.Name)
        )
            _fragmentNameDictionary.TryAdd(fragment.Name, fragment);

        _fragmentTypeDictionary[type].Add(fragment);
    }

    public void Process()
    {
        BuildMaterials();
        BuildActorDefs();
        BuildAnimations();
        BuildWorldTree();
        BuildLights();
    }

    private List<T> GetFragmentsOfType<T>()
        where T : WldFragment
    {
        return !_fragmentTypeDictionary.ContainsKey(typeof(T))
            ? []
            : _fragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
    }

    public string GetName(int reference)
    {
        return _strings.GetName(reference) ?? _fragments[reference].Name;
    }

    public WldFragment GetFragmentByName(string name)
    {
        return _fragmentNameDictionary.GetValueOrDefault(name);
    }

    public WldFragment GetFragment(int reference)
    {
        return reference switch
        {
            < 0 => _fragmentNameDictionary[_strings.GetName(reference)],
            0 => null,
            _ => _fragments[reference]
        };
    }

    private void AddResource(string name, Resource resource)
    {
        if (!Resources.TryGetValue(name, out var value)) Resources[name] = [];

        Resources[name].Add(resource);
    }

    private void BuildMaterials()
    {
        var materials = GetFragmentsOfType<Frag30MaterialDef>();
        foreach (var material in materials)
        {
            var godotMaterial = material.ToGodotMaterial();
            if (godotMaterial == null) continue;

            Materials.Add(material.Index, godotMaterial);
            AddResource(material.Name, godotMaterial);
        }
    }

    public ArrayMesh GetMesh(int reference)
    {
        if (_newMeshes.TryGetValue(reference, out var existing)) return existing;
        var dmSpriteDef2 = GetFragment(reference) as Frag36DmSpriteDef2;
        if (dmSpriteDef2 == null) return null;
        var mesh = dmSpriteDef2.ToGodotMesh();
        _newMeshes.Add(reference, mesh);
        return mesh;
    }

    private void BuildActorDefs()
    {
        var actorDefs = GetFragmentsOfType<Frag14ActorDef>();
        foreach (var actorDef in actorDefs)
        {
            var hsDef = actorDef.HierarchicalSprite?.HierarchicalSpriteDef;
            if (hsDef != null)
            {
                // GD.Print($@"WldFile {Name}: {actorDef.Name}");
                var godotHierarchicalActor = HierarchicalActorBuilder.Convert(actorDef, hsDef, this);
                ActorDefs.Add(godotHierarchicalActor.ResourceName, godotHierarchicalActor);
                AddResource(actorDef.Name, godotHierarchicalActor);
                continue;
            }

            var blit = actorDef.BlitSprite;
            if (blit != null)
            {
                var def = blit.BlitSpriteDef;
                var sprite = def?.SimpleSprite;
                if (sprite != null)
                {
                    // var godotBlitActor = new BlitActorDefinition
                    // {
                    //     ResourceName = actorDef.Name, Texture = sprite.ToGodotTexture(Archive),
                    //     Flags = blit.Flags,
                    //     Tag = actorDef.Name
                    // };
                    // ActorDefs.Add(actorDef.Index, godotBlitActor);
                    // AddResource(actorDef.Name, godotBlitActor);
                }
                else
                {
                    GD.PrintErr($"WldFile {Name} - {actorDef.Name}: No sprite found");
                }

                continue;
            }

            var dmMesh = actorDef.DmSprite?.NewMesh;
            if (dmMesh != null)
            {
                // actor.NewMeshes.Add(dmMesh.Name, NewMeshes[dmMesh.Index]);
                // ActorDefs.Add(actorDef.Index, actor);
                continue;
            }

            GD.PrintErr(
                $"WldFile {Name}: Skeleton is null for {actorDef.Name} - NewMesh: {actorDef.DmSprite} - Sprite2D: {actorDef.Sprite2D}"
            );
        }
    }

    private void BuildAnimations()
    {
        var animations = GetFragmentsOfType<Frag13Track>();
        foreach (var animation in animations.Where(animation => !animation.IsProcessed))
            ExtraAnimations.Add(
                animation.Index,
                ActorSkeletonPath.FromFrag13Track(animation)
            );
    }

    private void BuildWorldTree()
    {
        var worlds = GetFragmentsOfType<Frag21WorldTree>();
        switch (worlds.Count)
        {
            case 0:
                return;
            case > 1:
                GD.PrintErr($"More than one world tree found for {Name}.");
                break;
        }

        GD.Print("WldFile: Building world tree");
        WorldTree = worlds[0];
        var regions = GetFragmentsOfType<Frag22Region>();
        WorldTree.LinkBspRegions(regions);
    }

    private void BuildLights()
    {
        ZoneLights = new Array<Frag28PointLight>(GetFragmentsOfType<Frag28PointLight>());
    }
}
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
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file;

// Lantern Extractor class
public partial class WldFile : Resource
{
    private const int WldFileIdentifier = 0x54503D02;
    private const int WldFormatOldIdentifier = 0x00015500;
    private const int WldFormatNewIdentifier = 0x1000C800;

    [Export] public bool IsNewWldFormat;
    [Export] public Godot.Collections.Dictionary<string, Array<Resource>> Resources = [];

    [Export] public Godot.Collections.Dictionary<string, Resource> ActorDefs = [];
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    private Godot.Collections.Dictionary<int, ArrayMesh> _newMeshes = [];
    [Export] public Array<Frag28PointLight> ZoneLights;

    [Export] public Frag21WorldTree WorldTree = null;

    private readonly List<WldFragment> _fragments = [];
    private readonly System.Collections.Generic.Dictionary<Type, List<WldFragment>> _fragmentTypeDictionary = [];
    private Godot.Collections.Dictionary<int, byte[]> _fragmentContents = [];
    private Godot.Collections.Dictionary<string, WldFragment> _fragmentNameDictionary = [];
    private Godot.Collections.Dictionary<int, int> _fragmentTypes = [];
    private Godot.Collections.Dictionary<int, string> _strings = [];

    public WldFile()
        : this(null, null)
    {
    }

    public WldFile(PfsFile pfsFile, EqResourceLoader loader)
    {
        if (pfsFile == null)
        {
            GD.PrintErr("Invalid file received, doing nothing");
            return;
        }

        Name = pfsFile.Name;
        var content = pfsFile.FileBytes;

        // GD.Print($"WldFile {Name}: extracting WLD archive of length {content.Length}");
        var reader = new BinaryReader(new MemoryStream(content));

        var identifier = reader.ReadInt32();

        if (identifier != WldFileIdentifier)
        {
            GD.PrintErr("Not a valid WLD file!");
            return;
        }

        _fragments =
        [
            new FragXXFallback()
        ];

        var version = reader.ReadInt32();

        switch (version)
        {
            case WldFormatOldIdentifier:
                break;
            case WldFormatNewIdentifier:
                IsNewWldFormat = true;
                GD.Print("New WLD format not fully supported.");
                break;
            default:
                GD.PrintErr("Unrecognized WLD format.");
                return;
        }

        var fragmentCount = reader.ReadUInt32();
        var bspRegionCount = reader.ReadUInt32();
        var maxObjectBytes = reader.ReadInt32();
        var stringHashSize = reader.ReadUInt32();
        var stringCount = reader.ReadInt32();

        var strings = reader.ReadBytes((int)stringHashSize);
        var decoded = WldStringDecoder.Decode(strings);

        _strings = [];
        var index = 0;
        var splitHash = decoded.Split('\0');

        foreach (var hashString in splitHash)
        {
            _strings[index] = hashString;
            index += hashString.Length + 1;
        }

        for (var i = 1; i <= fragmentCount; ++i)
        {
            var fragSize = reader.ReadUInt32();
            var fragType = reader.ReadInt32();
            var fragmentContents = reader.ReadBytes((int)fragSize);

            _fragmentTypes[i + 1] = fragType;
            _fragmentContents[i + 1] = fragmentContents;

            var newFragment = !WldFragmentBuilder.Fragments.TryGetValue(
                fragType,
                out var value
            )
                ? new FragXXFallback()
                : value();

            if (newFragment is FragXXFallback)
            {
                GD.PrintErr($"WldFile {Name}: Unhandled fragment type: {fragType:x}");
                break;
            }

            newFragment.Initialize(i, fragType, (int)fragSize, fragmentContents, this, loader);

            _fragments.Add(newFragment);
            if (!_fragmentTypeDictionary.ContainsKey(newFragment.GetType()))
                _fragmentTypeDictionary[newFragment.GetType()] = [];

            if (
                !string.IsNullOrEmpty(newFragment.Name)
            )
                _fragmentNameDictionary.TryAdd(newFragment.Name, newFragment);

            _fragmentTypeDictionary[newFragment.GetType()].Add(newFragment);
        }

        // GD.Print($"WldFile {Name}: finished loading.");
        BuildMaterials();
        // BuildNewMeshes();
        BuildActorDefs();
        BuildAnimations();
        
        BuildWorldTree();
        BuildLights();
        // GD.Print($"WldFile {Name}: completed.");
    }

    [Export] public string Name { get; set; }

    private List<T> GetFragmentsOfType<T>()
        where T : WldFragment
    {
        return !_fragmentTypeDictionary.ContainsKey(typeof(T))
            ? []
            : _fragmentTypeDictionary[typeof(T)].Cast<T>().ToList();
    }

    public string GetName(int reference)
    {
        switch (reference)
        {
            case < 0:
            {
                if (!_strings.ContainsKey(-reference)) GD.PrintErr($"WldFile {Name}: String not found at {-reference}");

                return _strings[-reference];
            }
            case 0:
                return string.Empty;
            default:
                return _fragments[reference].Name;
        }
    }

    public WldFragment GetFragmentByName(string name)
    {
        return _fragmentNameDictionary.GetValueOrDefault(name);
    }

    public WldFragment GetFragment(int reference)
    {
        return reference switch
        {
            < 0 => _fragmentNameDictionary[_strings[-reference]],
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
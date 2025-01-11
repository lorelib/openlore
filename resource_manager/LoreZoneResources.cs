using System.IO;
using Godot;
using Godot.Collections;
using OpenLore.GameController;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class LoreZoneResources : LoreResources
{
    private string _activeZoneName;
    [Export] private Frag21WorldTree _activeZoneWorldTree;
    [Export] private Array<Frag28PointLight> _activeZoneLights;

    // TODO

    // Zone loading orchestration
    // Notes, the order in which the original client loads files is
    // - %s_environmentEmitters.txt -> load whatever this points to
    // - %s_%2s_obj2 -> with second argument being country code for asian countries or us
    // - %s_obj2 -> load item definitions
    // - %s_%2s_obj -> with second argument being country code for asian countries or us
    // - %s_obj -> load item definitions
    // - %s_%2s_2_obj -> with second argument being country code for asian countries or us
    // - %s_2_obj -> load item definitions
    // - %s_chr2 -> load character definitions
    // - %s2_chr -> load character definitions
    // - %s_chr -> load character definitions
    // - %s_chr.txt -> load whatever this points to
    // - %s_assets.txt -> load whatever this points to
    // - load main
    // - process objects.wld
    // - process lights.wld
    // - process %s.wld
    public void StartLoadingZone(string zoneName)
    {
        _activeZoneWorldTree = null;
        _activeZoneLights = null;
        _activeZoneName = zoneName;

        var assetPath = GameConfig.Instance.AssetPath;

        StartEqResourceLoad($"{zoneName}_obj2");
        StartEqResourceLoad($"{zoneName}_obj");
        StartEqResourceLoad($"{zoneName}_2_obj");
        StartEqResourceLoad($"{zoneName}_chr2");
        StartEqResourceLoad($"{zoneName}2_chr");
        StartEqResourceLoad($"{zoneName}_chr");

        using var chrReader = new StreamReader($"{assetPath}/{zoneName}_chr.txt");

        {
            var count = int.Parse(chrReader.ReadLine()!);
            for (var i = 0; i < count; i++)
            {
                var line = chrReader.ReadLine();
                var values = line!.Split(',');
                StartEqResourceLoad(values[0]);
            }
        }

        var assetsPath = $"{assetPath}/{zoneName}_assets.txt";
        if (File.Exists(assetsPath))
        {
            using var assetReader = new StreamReader(assetsPath);

            {
                while (!assetReader.EndOfStream)
                {
                    var line = assetReader.ReadLine();
                    StartEqResourceLoad(line);
                }
            }
        }

        var zoneLoader = StartEqResourceLoad(zoneName);
        zoneLoader.ZoneWorldTreeLoaded += OnWorldTreeLoaded;
        zoneLoader.ZonePointLightsLoaded += OnPointLightsLoaded;
    }

    protected override void OnLoadCompleted()
    {
        GD.Print($"LoreZoneResources {_activeZoneName} finished in {Time.GetTicksMsec()}ms since game started");
    }

    private void OnWorldTreeLoaded(Frag21WorldTree worldTree)
    {
        _activeZoneWorldTree = worldTree;
    }

    private void OnPointLightsLoaded(Array<Frag28PointLight> lights)
    {
        _activeZoneLights = lights;
    }

    public bool InstantiateZone(Node3D sceneRoot)
    {
        if (_activeZoneWorldTree == null || _activeZoneLights == null) return false;

        sceneRoot.AddChild(_activeZoneWorldTree.ToGodotZone());
        var count = 0;
        foreach (var l in _activeZoneLights)
        {
            sceneRoot.AddChild(l.ToGodotLight($"PointLight{count++}"));
        }

        return true;
    }
}
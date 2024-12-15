﻿using System.IO;
using Godot;
using OpenLore.GameController;

namespace OpenLore.resource_manager;

public partial class EqGlobalResources : EqResources
{
    
    public override void _Ready()
    {
        var assetPath = GameConfig.Instance.AssetPath;
        using var reader = new StreamReader($"{assetPath}/Resources/GlobalLoad.txt");
        while (reader.ReadLine() is { } line)
        {
            var values = line.Split(',');
            StartEqResourceLoad(values[3]);
        }
    }

    protected override void OnLoadCompleted()
    {
        GD.Print($"EqGlobalResources finished in {Time.GetTicksMsec()}ms since game started");
    }
}
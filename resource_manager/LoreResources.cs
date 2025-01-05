using System.Collections.Generic;
using Godot;
using OpenLore.resource_manager.godot_resources;

namespace OpenLore.resource_manager;

[GlobalClass]
public abstract partial class LoreResources : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];

    private int _ageCounter;

    public bool Completed { get; private set; }

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (Completed) return;

        var children = GetChildren();
        if (children.Count == 0) return;

        var allDone = true;
        foreach (var child in children)
        {
            if (child is LoreResourceLoader { Loaded: false })
            {
                allDone = false;
            }
        }

        if (!allDone || Completed) return;

        Completed = true;
        OnLoadCompleted();
    }

    protected abstract void OnLoadCompleted();

    protected LoreResourceLoader StartEqResourceLoad(string name)
    {
        Completed = false;
        var loader = new LoreResourceLoader()
        {
            Name = name.ToLower(), // Name replaces . with _
            RequestedFileName = name.ToLower(),
            AgeCounter = _ageCounter
        };
        AddChild(loader);
        _ageCounter += 1;
        return loader;
    }

    public Image GetImage(string name)
    {
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not LoreResourceLoader loader) continue;
            var image = loader.GetImage(name);
            if (image != null) return image;
        }

        return null;
    }

    public Resource GetActor(string tag)
    {
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not LoreResourceLoader loader) continue;
            var image = loader.GetActor(tag);
            if (image != null) return image;
        }

        return null;
    }

    public Dictionary<(string, string), ActorSkeletonPath> GetAnimationsFor(string tag)
    {
        Dictionary<(string, string), ActorSkeletonPath> result = [];
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not LoreResourceLoader loader) continue;
            foreach (var animation in loader.GetAnimationsFor(tag).Values)
            {
                result[(animation.AnimationName, animation.BoneName)] = animation;
            }
        }

        return result;
    }
}
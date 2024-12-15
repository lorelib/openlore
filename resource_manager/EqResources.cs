using System.Collections.Generic;
using EQGodot.resource_manager.godot_resources;
using Godot;

namespace EQGodot.resource_manager;

[GlobalClass]
public abstract partial class EqResources : Node
{
    private Godot.Collections.Dictionary<string, BlitActorDefinition> _blitActor = [];
    private Godot.Collections.Dictionary<string, HierarchicalActorDefinition> _hierarchicalActor = [];
    private Godot.Collections.Dictionary<string, ActorSkeletonPath> _extraAnimations = [];

    private int _ageCounter = 0;
    private bool _flaggedFinished = false;

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
        if (_flaggedFinished)
        {
            return;
        }

        var children = GetChildren();
        var allDone = true;
        foreach (var child in children)
        {
            if (child is EqResourceLoader { Loaded: false })
            {
                allDone = false;
            }
        }

        if (!allDone || _flaggedFinished) return;

        _flaggedFinished = true;
        OnLoadCompleted();
    }

    protected abstract void OnLoadCompleted();

    protected void StartEqResourceLoad(string name)
    {
        var loader = new EqResourceLoader()
        {
            Name = name.ToLower(),
            AgeCounter = _ageCounter
        };
        loader.SetProcessThreadGroup(ProcessThreadGroupEnum.SubThread);
        AddChild(loader);
        _ageCounter += 1;
    }

    public Image GetImage(string name)
    {
        var children = GetChildren();
        children.Reverse();
        foreach (var node in children)
        {
            if (node is not EqResourceLoader loader) continue;
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
            if (node is not EqResourceLoader loader) continue;
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
            if (node is not EqResourceLoader loader) continue;
            foreach (var animation in loader.GetAnimationsFor(tag).Values)
            {
                result[(animation.AnimationName, animation.BoneName)] = animation;
            }
        }

        return result;
    }
}
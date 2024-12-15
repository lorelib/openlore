using EQGodot.resource_manager.wld_file.fragments;
using Godot;
using Godot.Collections;

namespace EQGodot.resource_manager.godot_resources;

public partial class ActorSkeletonBone : Resource
{
    [Export] public ActorSkeletonPath BasePosition;
    [Export] public string CleanedFullPath;
    [Export] public string CleanedName;
    [Export] public string FullPath;
    [Export] public int Index;
    [Export] public string Name;
    [Export] public ActorSkeletonBone Parent;
    [Export] public Frag36DmSpriteDef2 NewMesh;
    [Export] public Dictionary<string, ActorSkeletonPath> AnimationTracks = [];

    //public ParticleCloud ParticleCloud;
}
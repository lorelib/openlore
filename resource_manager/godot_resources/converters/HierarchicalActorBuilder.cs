using System.Collections.Generic;
using EQGodot.resource_manager.wld_file;
using EQGodot.resource_manager.wld_file.fragments;
using EQGodot.resource_manager.wld_file.helpers;

namespace EQGodot.resource_manager.godot_resources.converters;

internal static class HierarchicalActorBuilder
{
    public static HierarchicalActorDefinition Convert(Frag14ActorDef actordef,
        Frag10HierarchicalSpriteDef hierarchicalSpriteDef,
        WldFile wld)
    {
        var name = FragmentNameCleaner.CleanName(actordef);
        // GD.Print($"HierarchicalActorBuilder::Convert: {name}");

        HierarchicalActorDefinition actor = new()
        {
            ResourceName = name,
            Tag = name,
            Flags = actordef.Flags,
        };

        hierarchicalSpriteDef.BuildSkeletonData(false);
        foreach (var hdDefMesh in hierarchicalSpriteDef.NewMeshes)
            actor.NewMeshes.Add(hdDefMesh.Name, wld.GetMesh(hdDefMesh.Index));
        foreach (var boneMesh in hierarchicalSpriteDef.NewMeshesByBone)
        {
            actor.NewMeshes.Add(boneMesh.Value.Name, wld.GetMesh(boneMesh.Value.Index));
        }
        
        HashSet<string> createdBones = [];

        foreach (var bone in hierarchicalSpriteDef.Skeleton)
        {
            var boneName = bone.Name[3..].ToLower().Replace("_dag", "");
            if (boneName == "") boneName = "root";
            while (createdBones.Contains(boneName))
            {
                boneName += "_dup";
            }
            var rbone = new ActorSkeletonBone
            {
                ResourceName = boneName,
                Index = bone.Index,
                Name = boneName,
                FullPath = bone.FullPath,
                CleanedName = bone.CleanedName,
                CleanedFullPath = bone.CleanedFullPath,
                NewMesh = bone.NewMesh,
                Parent = bone.Parent != null ? actor.Bones[bone.Parent.Index] : null
            };
            createdBones.Add(boneName);
            var track = bone.Track;
            if (track != null)
            {
                track.IsProcessed = true;
                track.IsPoseAnimation = true;
                rbone.BasePosition = ActorSkeletonPath.FromFrag13Track(track);
            }

            actor.Bones.Add(rbone);
            if (actor.BonesByName.ContainsKey(boneName)) actor.BonesByName.Remove(boneName);
            actor.BonesByName.Add(boneName, rbone);
        }

        return actor;
    }
}
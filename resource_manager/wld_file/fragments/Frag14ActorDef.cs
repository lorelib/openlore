using System.Collections.Generic;
using Godot;
using OpenLore.resource_manager.wld_file.data_types;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
internal partial class Frag14ActorDef : WldFragment
{
    [Export] public ActorType ActorType;
    [Export] public string CallbackName;
    [Export] public int Flags;
    [Export] public string ReferenceName;
    [Export] public Frag07Sprite2D Sprite2D;
    [Export] public Frag09Sprite3D Sprite3D;
    [Export] public Frag11HierarchicalSprite HierarchicalSprite;
    [Export] public Frag27BlitSprite BlitSprite;
    [Export] public Frag2DDmSprite DmSprite;
    [Export] public int BoundsRef;
    [Export] public int CurrentAction;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        Flags = Reader.ReadInt32(); // 0x04

        BitAnalyzer flag = new(Flags);
        var fragment2MustContainZero = flag.IsBitSet(7);

        CallbackName = wld.GetName(Reader.ReadInt32()); // 0x08

        // 1 for both static and animated objects
        var actionCount = Reader.ReadInt32(); // 0x0c

        // The number of components (meshes, skeletons, camera references) the actor has
        // In all Trilogy files, there is only ever 1
        var fragmentRefCount = Reader.ReadInt32(); // 0x10

        // 0 for both static and animated objects
        BoundsRef = Reader.ReadInt32();

        if (flag.IsBitSet(0))
        {
            CurrentAction = Reader.ReadInt32();
        }

        if (flag.IsBitSet(1))
        {
            List<float> location =
            [
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
            ];
            var unk1 = Reader.ReadInt32();
        }

        // Size 1 entries
        for (var i = 0; i < actionCount; ++i)
        {
            // Always 1
            var dataPairCount = Reader.ReadInt32();

            // Unknown purpose
            // Always 0 and 1.00000002E+30
            for (var j = 0; j < dataPairCount; ++j)
            {
                var value = Reader.ReadInt32();
                int value2 = Reader.ReadInt16();
                int value3 = Reader.ReadInt16();
            }
        }

        if (fragmentRefCount > 1) GD.PrintErr("Actor: More than one component references");

        // Can contain either a skeleton reference (animated), mesh reference (static) or a camera reference
        for (var i = 0; i < fragmentRefCount; ++i)
        {
            var fragment = wld.GetFragment(Reader.ReadInt32());

            Sprite2D = fragment as Frag07Sprite2D;
            if (Sprite2D != null) break;

            Sprite3D = fragment as Frag09Sprite3D;
            if (Sprite3D != null) break;

            HierarchicalSprite = fragment as Frag11HierarchicalSprite;
            if (HierarchicalSprite != null)
            {
                HierarchicalSprite.HierarchicalSpriteDef.IsAssigned = true;
                break;
            }

            BlitSprite = fragment as Frag27BlitSprite;
            if (BlitSprite != null) break;

            DmSprite = fragment as Frag2DDmSprite;
            if (DmSprite is { NewMesh: not null })
            {
                DmSprite.NewMesh.IsHandled = true;
                break;
            }

            GD.PrintErr($"Actor: Cannot link fragment with index {fragment.Index} of type {fragment.Type}");
        }

        // Always 0 in qeynos2 objects
        var name3Bytes = Reader.ReadInt32();

        CalculateActorType();
    }

    private void CalculateActorType()
    {
        if (Sprite3D != null)
        {
            ActorType = ActorType.Camera;
            ReferenceName = Sprite3D.Name;
        }
        else if (HierarchicalSprite != null)
        {
            ActorType = ActorType.Skeletal;
            ReferenceName = HierarchicalSprite.Name;
        }
        else if (DmSprite != null)
        {
            ActorType = ActorType.Static;
            ReferenceName = DmSprite.Name;
        }
        else if (BlitSprite != null)
        {
            ActorType = ActorType.Particle;
            ReferenceName = BlitSprite.Name;
        }
        else if (Sprite2D != null)
        {
            ActorType = ActorType.Sprite;
            ReferenceName = Sprite2D.Name;
        }
        else
        {
            GD.PrintErr("Cannot determine actor type!");
        }
    }

    public void AssignSkeletonReference(Frag10HierarchicalSpriteDef skeleton)
    {
        HierarchicalSprite = new Frag11HierarchicalSprite
        {
            HierarchicalSpriteDef = skeleton
        };

        CalculateActorType();
        skeleton.IsAssigned = true;
    }
}
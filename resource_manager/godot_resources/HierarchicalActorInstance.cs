using Godot;

namespace EQGodot.resource_manager.godot_resources;

[GlobalClass]
public partial class HierarchicalActorInstance : Node3D
{
    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void AttachItem(string tag, string bone, ResourceManager resourceManager)
    {
        NodePath wherePath = $"{Name}_skeleton/{bone}_attachment";
        if (!HasNode(wherePath))
        {
            GD.PrintErr($"No attachment point found for {wherePath}");
            return;
        }

        var where = GetNode<BoneAttachment3D>(wherePath);
        resourceManager.InstantiateHierarchicalInto(tag, where);
    }
}
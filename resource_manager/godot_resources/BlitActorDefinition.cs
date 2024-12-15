using Godot;

namespace OpenLore.resource_manager.godot_resources;

[GlobalClass]
public partial class BlitActorDefinition : ActorDefinition
{
    [Export] public int Flags;
    [Export] public string Tag;
    [Export] public Texture2D Texture;

    public Node Instantiate(PackedScene.GenEditState editState = (PackedScene.GenEditState)(0))
    {
        return new Node3D()
        {
            Name = $"Blit: {Tag}",
        };
    }
}
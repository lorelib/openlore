using Godot;

namespace EQGodot.resource_manager.interfaces;

public interface IIntoGodotLight
{
    public Light3D ToGodotLight();
}
using Godot;

namespace OpenLore.resource_manager.interfaces;

public interface IIntoGodotLight
{
    public Light3D ToGodotLight();
}
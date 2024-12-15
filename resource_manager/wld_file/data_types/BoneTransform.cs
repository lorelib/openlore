using Godot;

namespace OpenLore.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class BoneTransform : Resource
{
    [Export] public Quaternion ModelMatrix;
    [Export] public Vector3 Translation;
    [Export] public Quaternion Rotation;
    [Export] public float Scale;
}
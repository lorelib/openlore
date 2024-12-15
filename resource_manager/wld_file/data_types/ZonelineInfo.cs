using Godot;

namespace OpenLore.resource_manager.wld_file.data_types;

[GlobalClass]
public partial class ZonelineInfo : Resource
{
    [Export] public ZonelineType Type;
    [Export] public int Index;
    [Export] public Vector3 Position;
    [Export] public int Heading;
    [Export] public int ZoneIndex;
}
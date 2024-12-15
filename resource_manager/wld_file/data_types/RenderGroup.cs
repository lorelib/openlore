using Godot;

namespace OpenLore.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class RenderGroup : Resource
{
    public int StartPolygon;
    public int PolygonCount;
    public int MaterialIndex;
}
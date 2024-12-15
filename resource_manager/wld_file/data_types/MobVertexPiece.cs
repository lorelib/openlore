using Godot;

namespace OpenLore.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class MobVertexPiece : Resource
{
    public int Start;

    public int Count;

    public int Bone;
}
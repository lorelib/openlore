using Godot;

namespace OpenLore.resource_manager.wld_file.data_types;

// Lantern Extractor class
[GlobalClass]
public partial class Polygon : Resource
{
    public bool IsSolid;

    public int Vertex1;

    public int Vertex2;

    public int Vertex3;

    public int MaterialIndex;

    public Polygon GetCopy()
    {
        return new Polygon
        {
            IsSolid = IsSolid,
            Vertex1 = Vertex1,
            Vertex2 = Vertex2,
            Vertex3 = Vertex3,
            MaterialIndex = MaterialIndex
        };
    }
}
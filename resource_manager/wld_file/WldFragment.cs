using System.IO;
using Godot;

namespace EQGodot.resource_manager.wld_file;

// Lantern Extractor class
[GlobalClass]
public abstract partial class WldFragment : Resource
{
    [Export] public int Index;
    [Export] public int Type;
    [Export] public int Size;
    [Export] public string Name;
    public WldFile Wld;
    public EqResourceLoader Loader;

    protected BinaryReader Reader { get; set; }

    public virtual void Initialize(int index, int type, int size, byte[] data, WldFile wld, EqResourceLoader loader)
    {
        Index = index;
        Size = size;
        Type = type;
        Wld = wld;
        Loader = loader;
        Reader = new BinaryReader(new MemoryStream(data));
    }
}
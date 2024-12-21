using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.helpers;

namespace OpenLore.resource_manager.wld_file;

[GlobalClass]
public partial class WldStrings : Resource
{
    private Dictionary<int, string> _strings = [];

    public WldStrings()
    {
    }

    public WldStrings(byte[] encoded)
    {
        var decoded = WldStringDecoder.Decode(encoded);

        var index = 0;
        var splitHash = decoded.Split('\0');

        foreach (var hashString in splitHash)
        {
            _strings[index] = hashString;
            index += hashString.Length + 1;
        }
    }

    public string GetName(int reference)
    {
        if (reference >= 0) return null;
        
        if (_strings.ContainsKey(-reference))
        {
            return _strings[-reference];
        }
        GD.PrintErr($"Invalid reference");
        return null;
    }
}
using Godot;

namespace EQGodot.network_manager.login_server;

public partial class EQServerDescription : GodotObject
{
    public string Address;
    public uint Id;
    public string Language;
    public string LongName;
    public uint Players;
    public string Region;
    public uint ServerType;
    public uint Status;
}
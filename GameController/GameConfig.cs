using System.Collections.Generic;
using System.IO;
using System.Net;

namespace EQGodot.GameController;

public sealed class GameConfig
{
    private static GameConfig _instance;
    
    public string AssetPath { get; private set; }
    public string LoginServerHostname { get; private set; }
    public int LoginServerPort { get; private set; }


    private GameConfig()
    {
        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var dict = deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("open_lore.yaml"));
        AssetPath = dict["asset_path"];
        LoginServerHostname = dict["login_server_host"];
        LoginServerPort = int.Parse(dict["login_server_port"]);
    }

    public static GameConfig Instance => _instance ??= new GameConfig();
}
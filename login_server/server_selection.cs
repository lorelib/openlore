using System.Collections.Generic;
using EQGodot.network_manager.login_server;
using Godot;

namespace EQGodot.login_server;

public partial class server_selection : Control
{
    [Signal]
    public delegate void ServerJoinStartEventHandler(EQServerDescription server);

    private Dictionary<int, int> ListToPacketMap;
    private int SelectedServer;
    private EQServerDescription[] Servers;

    public void LoadServers(EQServerDescription[] servers)
    {
        ListToPacketMap = [];
        var list = GetNode<ItemList>("%ServerList");
        list.ItemSelected += OnServerSelected;
        Servers = servers;
        for (var i = 0; i < Servers.Length; i++)
        {
            var server = Servers[i];
            var index = list.AddItem($"{server.Address} - {server.LongName} - {server.Players}");
            ListToPacketMap[index] = i;
        }
    }

    private void OnServerSelected(long index)
    {
        SelectedServer = ListToPacketMap[(int)index];
    }

    private void OnServerAccepted()
    {
        EmitSignal(SignalName.ServerJoinStart, Servers[SelectedServer]);
    }
}
using EQGodot.login_server;
using EQGodot.network_manager.login_server;
using EQGodot.network_manager.world_server;
using EQGodot.resource_manager;
using Godot;
using System;

namespace EQGodot.GameController;

public partial class GameController : Node
{
    private Node ActiveScene;
    private EQServerDescription ActiveServer;
    private LoginSession NetworkLoginSession;
    private WorldSession NetworkWorldSession;
    private byte[] PlayerKey;
    private uint PlayerLSID;

    private ResourceManager _resources;

    private enum State
    {
        NONE,
        LOGIN,
        SERVER_SELECTION,
        RENDERING,
    }

    private State _state = State.NONE;

    public override void _Ready()
    {
        _resources = GetNode<ResourceManager>("ResourceManager");
        SwitchState(State.RENDERING);
    }

    public override void _Process(double delta)
    {
    }

    private void SwitchState(State newState)
    {
        if (_state != State.NONE)
        {
            ActiveScene.QueueFree();
        }

        switch (newState)
        {
            case State.LOGIN:
                var loginScene = ResourceLoader.Load<PackedScene>("res://login_server/login_screen.tscn");
                ActiveScene = loginScene.Instantiate<login_screen>();
                ((login_screen)ActiveScene).DoLogin += OnLoginScreenDoLogin;
                AddChild(ActiveScene);
                break;
            case State.SERVER_SELECTION:
                var serverSelectionScene = ResourceLoader.Load<PackedScene>("res://login_server/server_selection.tscn");
                ActiveScene = serverSelectionScene.Instantiate<server_selection>();
                ((server_selection)ActiveScene).ServerJoinStart += OnServerJoinStart;
                AddChild(ActiveScene);
                break;
            case State.RENDERING:
                var baseRenderingScene = ResourceLoader.Load<PackedScene>("res://base_scene.tscn");
                ActiveScene = baseRenderingScene.Instantiate<Node3D>();
                AddChild(ActiveScene);
                break;
            case State.NONE:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }


    private void OnLoginScreenDoLogin(string username, string password)
    {
        NetworkLoginSession = new LoginSession(username, password);
        NetworkLoginSession.MessageUpdate += ((login_screen)ActiveScene).OnMessageUpdate;
        NetworkLoginSession.ServerListReceived += OnServerListReceived;
        NetworkLoginSession.LoggedIn += OnLoggedIn;
        AddChild(NetworkLoginSession);
    }

    private void OnLoggedIn(uint lsid, byte[] key)
    {
        PlayerLSID = lsid;
        PlayerKey = key;
    }

    private void OnServerListReceived(EQServerDescription[] servers)
    {
        SwitchState(State.SERVER_SELECTION);
        ((server_selection)ActiveScene).LoadServers(servers);
    }

    private void OnServerJoinStart(EQServerDescription server)
    {
        ActiveServer = server;
        NetworkLoginSession.ServerJoinAccepted += OnServerJoinAccepted;
        NetworkLoginSession.JoinServer(ActiveServer);
    }

    private void OnServerJoinAccepted()
    {
        SwitchState(State.RENDERING);

        GD.Print($"Accepted on server {ActiveServer.LongName}, proceeding to join world server");

        NetworkWorldSession = new WorldSession(PlayerLSID, PlayerKey, ActiveServer);
        AddChild(NetworkWorldSession);
    }
}
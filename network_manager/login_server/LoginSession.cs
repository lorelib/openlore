using System;
using EQGodot.GameController;
using EQGodot.network_manager.network_session;
using EQGodot.network_manager.packets;
using Godot;

namespace EQGodot.network_manager.login_server;

public partial class LoginSession : Node
{
    [Signal]
    public delegate void LoggedInEventHandler(uint LSID, byte[] KeyComponents);

    [Signal]
    public delegate void MessageUpdateEventHandler(string message);

    [Signal]
    public delegate void ServerJoinAcceptedEventHandler();

    [Signal]
    public delegate void ServerListReceivedEventHandler(EQServerDescription[] servers);

    private NetworkSession Network;
    private readonly OpcodeManager OpcodeManager;
    private string Password;
    private string Username;

    public LoginSession(string username, string password)
    {
        Username = username;
        Password = password;
        OpcodeManager = new OpcodeManager();
        OpcodeManager.Register<CSGetServerList>(0x04);
        OpcodeManager.Register<CSHandshake>(0x01);
        OpcodeManager.Register<CSJoinServer>(0x0D);
        OpcodeManager.Register<CSPlayerLogin>(0x02);
        OpcodeManager.Register<SCGetServerListReply>(0x19);
        OpcodeManager.Register<SCHandshakeReply>(0x17);
        OpcodeManager.Register<SCJoinServerReply>(0x22);
        OpcodeManager.Register<SCPlayerLoginReply>(0x18);
        OpcodeManager.Register<SCSetGameFeatures>(0x31);
        Network = new NetworkSession();
        Network.SessionEstablished += OnConnectionEstablished;
        Network.ConnectToHost(GameConfig.Instance.LoginServerHostname, GameConfig.Instance.LoginServerPort);
        Name = "LoginSession";
    }

    public override void _Process(double delta)
    {
        Network.Process();
    }

    public void JoinServer(EQServerDescription server)
    {
        Network.SendAppPacket(new CSJoinServer(server.Id), OpcodeManager);
    }

    private void OnConnectionEstablished()
    {
        EmitSignal(SignalName.MessageUpdate, "Established connection, logging in");
        Network.PacketReceived += OnPacketReceived;
        Network.SendAppPacket(new CSHandshake(), OpcodeManager);
    }

    private void OnPacketReceived(byte[] packet)
    {
        var reader = new PacketReader(packet);
        var decoded = OpcodeManager.Decode(reader);
        _ = decoded switch
        {
            SCHandshakeReply p => ProcessPacket(p),
            SCSetGameFeatures p => ProcessPacket(p),
            SCPlayerLoginReply p => ProcessPacket(p),
            SCGetServerListReply p => ProcessPacket(p),
            SCJoinServerReply p => ProcessPacket(p),
            _ => throw new NotImplementedException()
        };
    }

    private bool ProcessPacket(SCHandshakeReply packet)
    {
        GD.Print($"Message: {packet.Message}");
        Network.SendAppPacket(new CSPlayerLogin(Username, Password), OpcodeManager);
        return true;
    }

    private bool ProcessPacket(SCSetGameFeatures packet)
    {
        // TODO: not sure if we care about this packet as it contains the expansions data
        return true;
    }

    private bool ProcessPacket(SCPlayerLoginReply packet)
    {
        if (packet.EQLSStr == 101)
        {
            EmitSignal(SignalName.LoggedIn, packet.LSID, packet.KeyComponents);
            EmitSignal(SignalName.MessageUpdate, "Logged in, retrieving server list");
            Network.SendAppPacket(new CSGetServerList(), OpcodeManager);
        }
        else
        {
            EmitSignal(SignalName.MessageUpdate, "There was an error while logging in");
        }

        return true;
    }

    private bool ProcessPacket(SCGetServerListReply packet)
    {
        EmitSignal(SignalName.ServerListReceived, packet.Servers);
        return true;
    }

    private bool ProcessPacket(SCJoinServerReply packet)
    {
        EmitSignal(SignalName.ServerJoinAccepted);
        Network.Disconnect();
        return true;
    }
}
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;
using SDL2Game.Core.Networking.Gui;

namespace SDL2Game.Core.Networking;

public class NetworkExample
{
    private const int PORT = 6969;
    private const string ADDRESS = "127.0.0.1";
    
    private INetworkService m_networkService;
    private IGuiRenderService m_guiRenderService;
    private IGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_variableBinder;
    
    private NetChatWindow m_netChatWindow;
    private ConnectionManager m_connectionManager;
    
    public NetworkExample(INetworkService networkService, IGuiRenderService guiRenderService, IGuiWindowBuilder guiWindowBuilder, IVariableBinder guiVarBinder)
    {
        m_networkService = networkService;
        m_guiRenderService = guiRenderService;
        m_guiWindowBuilder = guiWindowBuilder;
        m_variableBinder = guiVarBinder;

        m_connectionManager = new ConnectionManager(ADDRESS, PORT, m_networkService);
        m_netChatWindow = new NetChatWindow(m_connectionManager, m_networkService, m_guiRenderService, m_guiWindowBuilder, m_variableBinder);
    }

    public void Initialize()
    {
        SubscribeToEvents();
    }

    public void RenderGui()
    {
        m_netChatWindow.RenderGui();   
    }

    public void Shutdown()
    {
        m_networkService.Shutdown();
    }

    private void SubscribeToEvents()
    {
        EventHub.Subscribe<OnClientStatusChanged>(OnClientStatusChanged);
        EventHub.Subscribe<OnServerStatusChanged>(OnServerStatusChanged);
        EventHub.Subscribe<OnServerClientConnectionStatus>(OnServerClientConnectionStatus);
        
        EventHub.Subscribe<OnClientMessageRecieved>(OnClientMessageRecieved);
        EventHub.Subscribe<OnServerMessageRecieved>(OnServerMessageRecieved);
    }
    
    private void OnServerMessageRecieved(object? sender, OnServerMessageRecieved e)
    {
        var message = NetHelper.BytesToString(e.Data.RawBytes);
        Debug.Log($"<color=magenta>SERVER EVENT:</color> <color=yellow>Recieved</color> {message.Message}");
    }

    private void OnClientMessageRecieved(object? sender, OnClientMessageRecieved e)
    {
        var message = NetHelper.BytesToString(e.Data.RawBytes);
        Debug.Log($"<color=blue>CLIENT EVENT:</color> <color=yellow>Recieved</color> {message.Message}");
    }
    
    private void OnServerClientConnectionStatus(object? sender, OnServerClientConnectionStatus e)
    {
        switch (e.ClientStatus)
        {
            case ClientStatus.Connecting:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client connecting...");
                break;
            case ClientStatus.Connected:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client connected!");
                break;
            case ClientStatus.Disconnecting:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client disconnecting...");
                break;
            case ClientStatus.Disconnected:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client disconnected!");
                break;
            case ClientStatus.ServerClosedConnection:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client server closed connection!");
                break;
            case ClientStatus.TimedOut:
                Debug.Log($"<color=blue>SERVER EVENT:</color> [<color=magenta>{e.ClientData.Name}</color>] Client timed out!");
                break;
        }
    }

    private void OnServerStatusChanged(object? sender, OnServerStatusChanged e)
    {
        switch (e.ServerStatus)
        {
            case ServerStatus.Starting:
                Debug.Log($"<color=blue>SERVER EVENT:</color> Server is starting...");
                break;
            case ServerStatus.Started:
                Debug.Log($"<color=blue>SERVER EVENT:</color> Server is started!");
                break;
            case ServerStatus.Ending:
                Debug.Log($"<color=blue>SERVER EVENT:</color> Server is ending!");
                break;
            case ServerStatus.Ended:
                Debug.Log($"<color=blue>SERVER EVENT:</color> Server is ended!");
                break;
        }
    }

    private void OnClientStatusChanged(object? sender, OnClientStatusChanged e)
    {
        switch (e.ClientStatus)
        {
            case ClientStatus.Connecting:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client connecting...");
                break;
            case ClientStatus.Connected:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client connected!");
                break;
            case ClientStatus.Disconnecting:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client disconnecting...");
                break;
            case ClientStatus.Disconnected:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client disconnected!");
                break;
            case ClientStatus.ServerClosedConnection:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client server closed connection!");
                break;
            case ClientStatus.TimedOut:
                Debug.Log($"<color=magenta>CLIENT EVENT:</color> Client timed out!");
                break;
        }
    }
}
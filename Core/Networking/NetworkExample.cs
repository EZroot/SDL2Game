using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;

namespace SDL2Game.Core.Networking;

public class NetworkExample
{
    private INetworkService m_networkService;
    
    public NetworkExample(INetworkService networkService)
    {
        m_networkService = networkService;
    }

    public void Initialize()
    {
        SubscribeToEvents();
        Task.Run(StartServer);
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
    }

    private async Task StartServer()
    {
        await m_networkService.Server.Start(6969);
    }

    private async Task ClientConnection()
    {
        try
        {
            await m_networkService.Client.Connect("127.0.0.1", 6969);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect <color=magenta>CLIENT:</color> {ex.Message}");
        }

        string message = "Hello, Server!";
        byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes(message);
        await m_networkService.Client.SendDataAsync(dataToSend);
    }

    private void OnServerClientConnectionStatus(object? sender, OnServerClientConnectionStatus e)
    {
        switch (e.ClientStatus)
        {
            case ClientStatus.Connecting:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client connecting...");
                break;
            case ClientStatus.Connected:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client connected!");
                break;
            case ClientStatus.Disconnecting:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client disconnecting...");
                break;
            case ClientStatus.Disconnected:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client disconnected!");
                break;
            case ClientStatus.ServerClosedConnection:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client server closed connection!");
                break;
            case ClientStatus.TimedOut:
                Debug.Log($"<color=blue>SERVER:</color> [<color=magenta>{e.ClientData.Name}</color>] Client timed out!");
                break;
        }
    }

    private void OnServerStatusChanged(object? sender, OnServerStatusChanged e)
    {
        switch (e.ServerStatus)
        {
            case ServerStatus.Starting:
                Debug.Log($"<color=blue>SERVER:</color> Server is starting...");
                break;
            case ServerStatus.Started:
                Debug.Log($"<color=blue>SERVER:</color> Server is started!");
                Task.Run(ClientConnection);
                break;
            case ServerStatus.Ending:
                Debug.Log($"<color=blue>SERVER:</color> Server is ending!");
                break;
            case ServerStatus.Ended:
                Debug.Log($"<color=blue>SERVER:</color> Server is ended!");
                break;
        }
    }

    private void OnClientStatusChanged(object? sender, OnClientStatusChanged e)
    {
        switch (e.ClientStatus)
        {
            case ClientStatus.Connecting:
                Debug.Log($"<color=magenta>CLIENT:</color> Client connecting...");
                break;
            case ClientStatus.Connected:
                Debug.Log($"<color=magenta>CLIENT:</color> Client connected!");
                break;
            case ClientStatus.Disconnecting:
                Debug.Log($"<color=magenta>CLIENT:</color> Client disconnecting...");
                break;
            case ClientStatus.Disconnected:
                Debug.Log($"<color=magenta>CLIENT:</color> Client disconnected!");
                break;
            case ClientStatus.ServerClosedConnection:
                Debug.Log($"<color=magenta>CLIENT:</color> Client server closed connection!");
                break;
            case ClientStatus.TimedOut:
                Debug.Log($"<color=magenta>CLIENT:</color> Client timed out!");
                break;
        }
    }
}
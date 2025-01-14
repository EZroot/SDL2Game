using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Game.Core.Networking;

public class ConnectionManager
{
    private INetworkService m_networkService;
    private string m_address;
    private int m_port;
    
    public ConnectionManager(string address, int port, INetworkService networkService)
    {
        m_networkService = networkService;
        m_address = address;
        m_port = port;
    }

    public void StartServer()
    {
        _ = StartServerAsync();
        // Task.Run(StartServerAsync);
    }

    public void StartClient()
    {
        _ = StartClientAsync();
        //Task.Run(StartClientAsync);
    }
    
    public void StopServer()
    {
        _ = StopServerAsync();
        // Task.Run(StopServerAsync);
    }

    public void StopClient()
    {
        _ = StopClientAsync();
        //Task.Run(StopClientAsync);
    }
    
    private async Task StartServerAsync()
    {
        await m_networkService.Server.Start(m_port);
    }
    
    private async Task StopServerAsync()
    {
        await m_networkService.Server.Stop();
    }
    
    private async Task StartClientAsync()
    {
        try
        {
            await m_networkService.Client.Connect(m_address, m_port);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect <color=magenta>CLIENT:</color> {ex.Message}");
        }
    }
    
    private async Task StopClientAsync()
    {
        try
        {
            await m_networkService.Client.Disconnect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect <color=magenta>CLIENT:</color> {ex.Message}");
        }
    }
}
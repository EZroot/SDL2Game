using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;

namespace SDL2Game.Core.Networking.Gui;

public class NetChatWindow
{
    private ConnectionManager m_connectionManager;
    private INetworkService m_networkService;
    private IGuiRenderService m_guiRenderService;
    private IGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_variableBinder;

    private List<string> m_recievedClientMessages = new();
    private List<string> m_recievedServerMessages = new();

    private string m_username = "Default";
    
    private string m_usernameBuffer = "";
    private string m_chatBuffer = "";
    
    private bool m_startServer;
    private bool m_startClient;

    private int m_port;
    private string m_address;

    private ClientStatus m_clientNetStatus = ClientStatus.Disconnected;
    private ServerStatus m_serverNetStatus = ServerStatus.Ended;
    
    public NetChatWindow(ConnectionManager connectionManager, INetworkService networkService, IGuiRenderService guiRenderService, IGuiWindowBuilder guiWindowBuilder,
        IVariableBinder guiVarBinder)
    {
        m_connectionManager = connectionManager;
        m_networkService = networkService;
        m_guiRenderService = guiRenderService;
        m_guiWindowBuilder = guiWindowBuilder;
        m_variableBinder = guiVarBinder;
        
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        EventHub.Subscribe<OnServerStatusChanged>(OnServerStatusChanged);
        EventHub.Subscribe<OnClientStatusChanged>(OnClientStatusChanged);
        
        EventHub.Subscribe<OnClientMessageRecieved>(OnClientMessageRecieved);
        EventHub.Subscribe<OnServerMessageRecieved>(OnServerMessageRecieved);
    }
    
public void RenderGui()
{
    if (NetGuiWindowBindings.IsShowingNetWindow)
    {
        if (ImGui.Begin("Network Chat", ref NetGuiWindowBindings.IsShowingNetWindow, ImGuiWindowFlags.MenuBar ))
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.Button("Options"))
                {
                    
                }
                ImGui.Separator();
                if (ImGui.Button("Start Server"))
                {
                    m_startServer = !m_startServer;

                    if (m_startServer)
                    {
                        m_connectionManager.StartServer();
                    }
                    else
                    {
                        m_connectionManager.StopServer();
                    }
                }

                if (ImGui.Button("Start Client"))
                {
                    m_startClient = !m_startClient;
                    if (m_startClient)
                    {
                        m_connectionManager.StartClient();
                    }
                    else
                    {
                        m_connectionManager.StopClient();
                    }
                }
                ImGui.SeparatorText($"SERVER: {m_serverNetStatus.ToString().ToUpper()} CLIENT: {m_clientNetStatus.ToString().ToUpper()}");
                ImGui.EndMenuBar();
            }

            ImGui.SeparatorText("Username Setup");
            
            if (ImGui.InputText("##SetUsernameInput", ref m_usernameBuffer, 24))
            {
                
            }
            
            ImGui.SameLine();

            if (ImGui.Button("Set"))
            {
                m_username = m_usernameBuffer;
            }
            
            ImGui.SeparatorText("");

            Vector2 availableSize = ImGui.GetContentRegionAvail();
            float childWidth = (availableSize.X / 3) - ImGui.GetStyle().ItemSpacing.X;
            float childHeight = availableSize.Y - 24;

            ImGui.BeginChild("ClientMsgs", new Vector2(childWidth+192, childHeight), ImGuiChildFlags.Borders);
            {
                ImGui.Text($"Client Chat (Status: {m_clientNetStatus})");
                ImGui.Separator();
                foreach (var chat in m_recievedClientMessages)
                {
                    ImGui.TextUnformatted($"{chat}");
                }
            }
            ImGui.EndChild();

            ImGui.SameLine();

            ImGui.BeginChild("ServerMsgs", new Vector2(childWidth-64, childHeight), ImGuiChildFlags.Borders);
            {
                ImGui.Text($"Server Chat (Status: {m_serverNetStatus})");
                ImGui.Separator();
                foreach (var chat in m_recievedServerMessages)
                {
                    ImGui.TextUnformatted($"RECIEVED: {chat}");
                }
            }
            ImGui.EndChild();
            
            ImGui.SameLine();

            ImGui.BeginChild("ConnectedClients", new Vector2(childWidth-124, childHeight), ImGuiChildFlags.Borders);
            {
                try
                {
                    if (m_networkService.Server.Connections != null)
                    {
                        ImGui.Text($"Connections ({m_networkService.Server.Connections.Count})");
                        ImGui.Separator();
                        foreach (var conn in m_networkService.Server.Connections)
                        { 
                            ImGui.Text($"Client - {conn.Id} - [{conn.TcpClient.Client.RemoteEndPoint}] Conn:{conn.TcpClient.Connected}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            ImGui.EndChild();

            ImGui.Separator();
            ImGui.Text($"{m_username}:>");
            
            ImGui.SameLine();

            if (ImGui.InputText("##UserChat", ref m_chatBuffer, 1024))
            {
            }
            
            ImGui.SameLine();

            if (ImGui.Button("Send"))
            {
                if (m_clientNetStatus != ClientStatus.Connected)
                {
                    m_recievedClientMessages.Add("ERROR: Not connected to server!");
                }
                else
                {
                    var msg = $"{m_username}:> {m_chatBuffer}";
                    var data = NetHelper.StringToBytes(msg);
                    m_networkService.Client.SendDataAsync(data.Data);
                    m_chatBuffer = "";
                }
            }

            ImGui.End();
        }
    }
}

    private void OnServerStatusChanged(object? sender, OnServerStatusChanged e)
    {
        m_serverNetStatus = e.ServerStatus;
    }

    private void OnClientStatusChanged(object? sender, OnClientStatusChanged e)
    {
        m_clientNetStatus = e.ClientStatus;
    }

    private void OnServerMessageRecieved(object? sender, OnServerMessageRecieved e)
    {
        var message = NetHelper.BytesToString(e.Data.RawBytes);
        m_recievedServerMessages.Add(message.Message);
        
    }

    private void OnClientMessageRecieved(object? sender, OnClientMessageRecieved e)
    {
        var message = NetHelper.BytesToString(e.Data.RawBytes);
        m_recievedClientMessages.Add(message.Message);
    }
}
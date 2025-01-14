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

    private bool m_isChatWindowOpen = true;
    private List<string> m_recievedClientMessages = new();
    private List<string> m_recievedServerMessages = new();
    
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
        if (m_isChatWindowOpen)
        {
            if (ImGui.Begin("Network Chat", ref m_isChatWindowOpen, ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.Button("Options"))
                    {
                        ImGui.OpenPopup("MoreOptionsPopup");
                    }

                    ImGui.Separator();
                    ImGui.Text(" Net Chat");

                    if (ImGui.BeginPopup("MoreOptionsPopup"))
                    {
                        if (ImGui.Checkbox("Start Server", ref m_startServer))
                        {
                            if (m_startServer)
                            {
                                m_connectionManager.StartServer();
                            }
                            else
                            {
                                m_connectionManager.StopServer();
                            }
                        }
                        
                        if (ImGui.Checkbox("Start Client", ref m_startClient))
                        {
                            if (m_startClient)
                            {
                                m_connectionManager.StartClient();
                            }
                            else
                            {
                                m_connectionManager.StopClient();
                            }
                        }                 
                        
                        ImGui.EndPopup();
                    }

                    ImGui.EndMenuBar();
                }

                Vector2 availableSize = ImGui.GetContentRegionAvail();
                float childWidth = (availableSize.X / 2) - ImGui.GetStyle().ItemSpacing.X;
                float childHeight = availableSize.Y - 32;

                if (ImGui.BeginChild("ClientMsgs", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders))
                {
                    ImGui.Text($"Client Chat (Status: {m_clientNetStatus})");
                    ImGui.Separator();
                    foreach (var chat in m_recievedClientMessages)
                    {
                        ImGui.TextUnformatted($"CLIENT:> {chat}");
                    }
                    ImGui.EndChild();
                }

                ImGui.SameLine();

                if (ImGui.BeginChild("ServerMsgs", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders))
                {
                    ImGui.Text($"Server Chat (Status: {m_serverNetStatus})");
                    ImGui.Separator();
                    foreach (var chat in m_recievedServerMessages)
                    {
                        ImGui.TextUnformatted($"SERVER:> {chat}");
                    }
                    ImGui.EndChild();
                }
                ImGui.Separator();
                ImGui.Text("Msg:>");
                
                ImGui.SameLine();

                if (ImGui.InputText("##UserChat", ref m_chatBuffer, 1024))
                {
                    
                }
                
                ImGui.SameLine();
                
                if (ImGui.Button("Send"))
                {
                    m_recievedClientMessages.Add($"Sent: {m_chatBuffer}");
                    var data = NetHelper.StringToBytes(m_chatBuffer);
                    m_networkService.Client.SendDataAsync(data.Data);
                    m_chatBuffer = "";
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
using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData;

namespace SDL2Game.Core.Networking.Gui
{
    public class NetChatWindow
    {
        private ConnectionManager m_connectionManager;
        private INetworkService m_networkService;
        private IGuiRenderService m_guiRenderService;
        private IGuiWindowBuilder m_guiWindowBuilder;
        private IVariableBinder m_variableBinder;

        // Use ConcurrentQueue for thread-safe message handling
        private readonly ConcurrentQueue<string> m_clientMessageQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> m_serverMessageQueue = new ConcurrentQueue<string>();

        // Local lists for display (accessed only from the main thread)
        private readonly List<string> m_recievedClientMessages = new List<string>();
        private readonly List<string> m_recievedServerMessages = new List<string>();

        private string m_username = "Default";

        private string m_usernameBuffer = "";
        private string m_chatBuffer = "";

        private bool m_startServer;
        private bool m_startClient;

        private int m_port;
        private string m_address;

        private DataType m_protocolDataType = DataType.Message;
        private ClientStatus m_clientNetStatus = ClientStatus.Disconnected;
        private ServerStatus m_serverNetStatus = ServerStatus.Ended;

        private int m_clockStream = 0;
        private int m_clockStreamServer = 0;
        
        public NetChatWindow(
            ConnectionManager connectionManager,
            INetworkService networkService,
            IGuiRenderService guiRenderService,
            IGuiWindowBuilder guiWindowBuilder,
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
            EventHub.Subscribe<OnClientStreamDataReceived>(OnClientStreamDataReceived);
            EventHub.Subscribe<OnServerStreamDataReceived>(OnServerStreamDataReceived);
            EventHub.Subscribe<OnServerMessageRecieved>(OnServerMessageRecieved);
        }

        private void Update()
        {
            if (m_networkService.Server.IsServer)
            {
                m_clockStreamServer += (int)Time.TotalTime;

                var msg = $"{m_clockStreamServer}";
                var messageBytes = NetHelper.StringToBytes(msg);
                var protocolMessage = new ProtocolMessage
                {
                    Type = DataType.Stream,
                    Length = messageBytes.Data.Length,
                    Payload = messageBytes.Data
                };

                var dataToSend = protocolMessage.ToBytes();

                if (m_networkService.Client.IsConnected)
                    m_networkService.Client.SendDataAsync(dataToSend);
            }
        }

        public void RenderGui()
        {
            Update();
            ProcessIncomingMessages();

            if (NetGuiWindowBindings.IsShowingNetWindow)
            {
                if (ImGui.Begin("Network Chat", ref NetGuiWindowBindings.IsShowingNetWindow, ImGuiWindowFlags.MenuBar))
                {
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.Button("Options"))
                        {
                            // Implement options if needed
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

                        ImGui.Separator();
                        ImGui.Text("Protocol:");

                        if (ImGui.BeginCombo("##DataType", EnumHelper.GetDescription(m_protocolDataType), ImGuiComboFlags.WidthFitPreview))
                        {
                            foreach (DataType dataType in Enum.GetValues(typeof(DataType)))
                            {
                                string displayName = dataType.GetDescription();
                                bool isSelected = (dataType == m_protocolDataType);

                                if (ImGui.Selectable(displayName, isSelected))
                                {
                                    m_protocolDataType = dataType;
                                }

                                if (isSelected)
                                {
                                    ImGui.SetItemDefaultFocus();
                                }
                            }

                            ImGui.EndCombo();
                        }

                        ImGui.Separator();
                        ImGui.SeparatorText($"[SERVER: {m_serverNetStatus.ToString().ToUpper()}] [CLIENT: {m_clientNetStatus.ToString().ToUpper()}]");
                        ImGui.EndMenuBar();
                    }
                    
                    ImGui.Text("Streaming example");
                    ImGui.Separator();
                    ImGui.Text($"Server: {m_clockStreamServer} Client: {m_clockStream}");

                    ImGui.SeparatorText("Username Setup");

                    if (ImGui.InputText("##SetUsernameInput", ref m_usernameBuffer, 24))
                    {
                        // Handle username input if needed
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Set"))
                    {
                        if (!string.IsNullOrWhiteSpace(m_usernameBuffer))
                        {
                            m_username = m_usernameBuffer;
                            m_usernameBuffer = ""; // Clear buffer after setting
                        }
                    }

                    ImGui.SeparatorText("");

                    Vector2 availableSize = ImGui.GetContentRegionAvail();
                    float childWidth = (availableSize.X / 3) - ImGui.GetStyle().ItemSpacing.X;
                    float childHeight = availableSize.Y - 24;

                    // Client Messages
                    ImGui.BeginChild("ClientMsgs", new Vector2(childWidth + 192, childHeight), ImGuiChildFlags.Borders);
                    {
                        ImGui.Text($"Client Chat (Status: {m_clientNetStatus})");
                        ImGui.Separator();
                        foreach (var chat in m_recievedClientMessages)
                        {
                            ImGui.TextUnformatted(chat);
                        }
                    }
                    ImGui.EndChild();

                    ImGui.SameLine();

                    // Server Messages
                    ImGui.BeginChild("ServerMsgs", new Vector2(childWidth - 64, childHeight), ImGuiChildFlags.Borders);
                    {
                        ImGui.Text($"Server Chat (Status: {m_serverNetStatus})");
                        ImGui.Separator();
                        foreach (var chat in m_recievedServerMessages)
                        {
                            ImGui.TextUnformatted(chat);
                        }
                    }
                    ImGui.EndChild();

                    ImGui.SameLine();

                    // Connected Clients
                    ImGui.BeginChild("ConnectedClients", new Vector2(childWidth - 124, childHeight), ImGuiChildFlags.Borders);
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
                        // Handle chat input if needed
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
                            var messageBytes = NetHelper.StringToBytes(msg);
                            var protocolMessage = new ProtocolMessage
                            {
                                Type = m_protocolDataType,
                                Length = messageBytes.Data.Length,
                                Payload = messageBytes.Data
                            };

                            var dataToSend = protocolMessage.ToBytes();

                            m_networkService.Client.SendDataAsync(dataToSend);
                            m_chatBuffer = "";
                        }
                    }

                    ImGui.End();
                }
            }
        }

        /// <summary>
        /// Processes incoming messages from the concurrent queues.
        /// </summary>
        private void ProcessIncomingMessages()
        {
            // Dequeue client messages
            while (m_clientMessageQueue.TryDequeue(out var clientMsg))
            {
                m_recievedClientMessages.Add(clientMsg);
            }

            // Dequeue server messages
            while (m_serverMessageQueue.TryDequeue(out var serverMsg))
            {
                m_recievedServerMessages.Add(serverMsg);
            }
        }

        // Event Handlers

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
            // Enqueue to server message queue
            m_serverMessageQueue.Enqueue("[Message] " + message.Message);
        }

        private void OnClientMessageRecieved(object? sender, OnClientMessageRecieved e)
        {
            var message = NetHelper.BytesToString(e.Data.RawBytes);
            // Enqueue to client message queue
            m_clientMessageQueue.Enqueue("[Message] " + message.Message);
        }

        private void OnClientStreamDataReceived(object? sender, OnClientStreamDataReceived e)
        {
            var message = NetHelper.BytesToString(e.Data.RawBytes);
            if (int.TryParse(message.Message, out var result))
            {
                m_clockStream = result;
            }
            // var message = NetHelper.BytesToString(e.Data.RawBytes);
            // // Enqueue to client message queue
            // m_clientMessageQueue.Enqueue("[STREAM] " + message.Message);
        }

        private void OnServerStreamDataReceived(object? sender, OnServerStreamDataReceived e)
        {
            // var message = NetHelper.BytesToString(e.Data.RawBytes);
            // // Enqueue to server message queue
            // m_serverMessageQueue.Enqueue("[STREAM] " + message.Message);
        }
    }
}

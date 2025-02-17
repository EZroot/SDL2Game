using ImGuiNET;
using SDL2;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Utils;
using SDL2Game.Core.Gui.Gui;
using SDL2Game.Core.Networking.Gui;

namespace SDL2Game.Core.Gui;

public class GuiExample
{
    private bool m_showDebugConsole;
    private ISysInfo m_sysInfo;
    private IGuiRenderService m_guiRenderService;
    private IGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_variableBinder;

    private ImGuiDockData m_guiDockerData;
    private int m_PinkboyCount;
    
    private GuiCellTableQuery m_guiPhysicsDebugCellTable;
    private GuiCellTableQuery m_guiAudioSynthCellTable;
    private GuiStringQuery<int> m_guiStringQuery_PinkboyCount;
    private GuiStringQuery<string> m_guiStringQuery_MouseOverLabel;
    
    public GuiExample(IGuiRenderService guiRenderService, IGuiWindowBuilder guiWindowBuilder, IVariableBinder guiVarBinder, ISysInfo sysInfo)
    {
        m_sysInfo = sysInfo;
        m_guiRenderService = guiRenderService;
        m_guiWindowBuilder = guiWindowBuilder;
        m_variableBinder = guiVarBinder;
        
        m_guiDockerData = new ImGuiDockData(
            new DockPanelData("Main Dock", true),
            new DockPanelData("Left Dock", false),
            new DockPanelData("Top Dock", false),
            new DockPanelData("Right Dock", true),
            new DockPanelData("Bottom Dock", false),
            hasFileMenu: true);

        m_guiPhysicsDebugCellTable = new GuiCellTableQuery("CellTableOne",m_variableBinder);
        m_guiAudioSynthCellTable = new GuiCellTableQuery("CellTableAudioSynth",m_variableBinder);
        m_guiStringQuery_PinkboyCount = new GuiStringQuery<int>("Pinkboy Count", m_variableBinder);
        m_guiStringQuery_MouseOverLabel = new GuiStringQuery<string>("##MouseLabel", m_variableBinder);
    }

    public void RenderGui()
    {
        if (m_guiDockerData.IsDockInitialized == false)
        {
            m_guiDockerData = m_guiRenderService.InitializeDockSpace(m_guiDockerData);
        }

        m_guiRenderService.RenderFullScreenDockSpace(m_guiDockerData);
        RenderFileMenu();

        if (m_showDebugConsole)
            Debug.RenderDebugConsole(ref m_showDebugConsole);

        m_guiWindowBuilder.BeginWindow("Physics Debugger");//m_guiDockerData.BottomDock.Name);
        m_guiStringQuery_PinkboyCount.DrawQuery(m_guiWindowBuilder);
        m_guiStringQuery_MouseOverLabel.DrawQuery(m_guiWindowBuilder);
        // Todo: Fix this trash causing crashes
        // m_guiPhysicsDebugCellTable.DrawDebugQuery(m_guiWindowBuilder);
        m_guiWindowBuilder.EndWindow();
    }

    public void UpdateDebugQuery(bool isHovering, Dictionary<string, List<(string Property, string Value)>> debugQuery)
    {
        var label = isHovering ? "MouseQuery: Hovering over physicsObject" : "MouseQuery: N/A";
        m_guiStringQuery_MouseOverLabel.UpdateQuery(label);
        if (InputManager.IsMouseButtonPressed(SDL.SDL_BUTTON_RIGHT))
        {
            m_guiPhysicsDebugCellTable.UpdateDebugQuery(debugQuery);
        }
    }

    public void UpdateAudioSynthQuery(Dictionary<string, List<(string Property, string Value)>> debugQuery)
    {
        m_guiAudioSynthCellTable.UpdateDebugQuery(debugQuery);
    }
    
    public void UpdatePinkboyCount(int addPinkboyCount)
    {
        m_guiStringQuery_PinkboyCount.UpdateQuery(addPinkboyCount);
    }
    
    private void RenderFileMenu()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.SmallButton("File"))
            {
            }
            ImGui.Separator();
            
            if (ImGui.SmallButton("Edit"))
            {
            }
            ImGui.Separator();

            if (ImGui.SmallButton("Help"))
            {
            }
            ImGui.Separator();

            if (ImGui.SmallButton("Debug Console"))
            {
                m_showDebugConsole = !m_showDebugConsole;
            }
            ImGui.Separator();

            if (ImGui.SmallButton("Network Chat"))
            {
                NetGuiWindowBindings.IsShowingNetWindow = !NetGuiWindowBindings.IsShowingNetWindow;
            }
            ImGui.Separator();

            var fps = $"Fps: {Time.Fps:F2} (delta: {Time.DeltaTime:F2})";
            var fullHeader = $"Mouse: {InputManager.MouseX}x{InputManager.MouseY} Driver: {m_sysInfo.SDLRenderInfo.CurrentRenderDriver} {fps}";
            var windowWidth = ImGui.GetWindowWidth();
            var textWidth = ImGui.CalcTextSize(fullHeader).X;
            ImGui.SameLine(windowWidth - textWidth - ImGui.GetStyle().ItemSpacing.X * 2);
            ImGui.Text(fullHeader);
            ImGui.EndMainMenuBar();
        }
    }
}
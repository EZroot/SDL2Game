using ImGuiNET;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Game.Core.Gui;

public class GuiTest
{
    private bool m_showDebugConsole;
    private IServiceSysInfo m_sysInfo;
    private IServiceGuiRenderService m_guiRenderService;
    private IServiceGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_variableBinder;

    private ImGuiDockData m_guiDockerData;
    private int m_pokemonCount;
    
    public GuiTest(IServiceGuiRenderService guiRenderService, IServiceGuiWindowBuilder guiWindowBuilder, IVariableBinder guiVarBinder, IServiceSysInfo sysInfo)
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
    }
    
    public void RenderGui()
    {
        if (m_guiDockerData.IsDockInitialized == false)
        {
            m_guiDockerData = m_guiRenderService.InitializeDockSpace(m_guiDockerData);
            Initialize();
        }

        m_guiRenderService.RenderFullScreenDockSpace(m_guiDockerData);
        
        RenderFileMenu();
        
        if (m_showDebugConsole)
            Debug.RenderDebugConsole(ref m_showDebugConsole);

        m_guiWindowBuilder.BeginWindow(m_guiDockerData.BottomDock.Name);
            m_guiWindowBuilder.Draw("PokemonCount");
        m_guiWindowBuilder.EndWindow();
    }
    
    private void Initialize()
    {
        
        m_variableBinder.BindVariable("PokemonCount", m_pokemonCount);
    }

    public void UpdatePokemonCount(int addPokemonCount)
    {
        m_pokemonCount += addPokemonCount;
        m_variableBinder.BindVariable("PokemonCount", m_pokemonCount);
    }
    private void RenderFileMenu()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.Button("File"))
            {
            }

            if (ImGui.Button("Edit"))
            {
            }

            if (ImGui.Button("Help"))
            {
            }

            if (ImGui.Button("Debug"))
            {
                ImGui.OpenPopup("DebugWindowPopup");
            }

            if (ImGui.BeginPopup("DebugWindowPopup"))
            {
                if (ImGui.Button("Show Console Output"))
                {
                    m_showDebugConsole = !m_showDebugConsole;
                }

                ImGui.EndPopup();
            }

            var fps = $"Fps: {Time.Fps:F2} (delta: {Time.DeltaTime:F2})";
            var fullHeader = $"Driver: {m_sysInfo.SDLRenderInfo.CurrentRenderDriver} {fps}";
            var windowWidth = ImGui.GetWindowWidth();
            var textWidth = ImGui.CalcTextSize(fullHeader).X;
            ImGui.SameLine(windowWidth - textWidth - ImGui.GetStyle().ItemSpacing.X * 2);
            ImGui.Text(fullHeader);
            ImGui.EndMainMenuBar();
        }
    }
}
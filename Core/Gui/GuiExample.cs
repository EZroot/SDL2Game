using ImGuiNET;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Utils;

namespace SDL2Game.Core.Gui;

public class GuiExample
{
    private bool m_showDebugConsole;
    private IServiceSysInfo m_sysInfo;
    private IServiceGuiRenderService m_guiRenderService;
    private IServiceGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_variableBinder;

    private ImGuiDockData m_guiDockerData;
    
    private string[] m_debugQueryData;
    private int m_pokemonCount;
    
    private ImGuiTableData m_tableData;
    private ImGuiCellTableData m_cellTableData;
    
    public GuiExample(IServiceGuiRenderService guiRenderService, IServiceGuiWindowBuilder guiWindowBuilder, IVariableBinder guiVarBinder, IServiceSysInfo sysInfo)
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
            for (var i = 0; i < m_debugQueryData.Length; i++)
                m_guiWindowBuilder.Draw($"{m_debugQueryData[i]}_{i}");
            m_guiWindowBuilder.Draw("Table");
            m_guiWindowBuilder.Draw("CellTableData");
            m_guiWindowBuilder.EndWindow();
    }
    
    private void Initialize()
    {
        InitializeTable();
        m_variableBinder.BindVariable<ImGuiTableData>("Table", m_tableData);
        m_variableBinder.BindVariable("CellTableData", m_cellTableData);
        m_variableBinder.BindVariable("PokemonCount", m_pokemonCount);
        BindDebugQuery();
    }

    private void BindDebugQuery()
    {
        for (var i = 0; i < m_debugQueryData.Length; i++)
        {
            var str = m_debugQueryData[i];
            m_variableBinder.BindVariable($"{str}_{i}", str);
        }
    }

    public void UpdateDebugQuery(string[] debugQuery)
    {
        m_debugQueryData = debugQuery;
        BindDebugQuery();
    }
    
    public void UpdatePokemonCount(int addPokemonCount)
    {
        m_pokemonCount += addPokemonCount;
        m_variableBinder.BindVariable("PokemonCount", m_pokemonCount);
    }

    private void InitializeTable()
    {
        // Table
        var tableInputData = new ImGuiInputData("Alice", "Alice");
        var tableInputData1 = new ImGuiInputData("Bob", "Bob");
        var tableInputData2 = new ImGuiInputData("Charlie", "Charlie");
        var tableInputData3 = new ImGuiInputData("30", "30", true);
        var tableInputData4 = new ImGuiInputData("25", "25", true);
        var tableInputData5 = new ImGuiInputData("22", "22", true);
        var tableInputData6 = new ImGuiInputData("Engineer", "Engineer");
        var tableInputData7 = new ImGuiInputData("Designer", "Designer", true);
        var tableInputData8 = new ImGuiInputData("Manager", "Manager");
        var tableFlags = ImGuiTableFlags.None;
        var labelOnRight = true;
        m_tableData = new ImGuiTableData(
            tableFlags,
            labelOnRight,
            new ImGuiColumnData("Name", tableInputData, tableInputData1, tableInputData2),
            new ImGuiColumnData("Age", tableInputData3, tableInputData4, tableInputData5),
            new ImGuiColumnData("Ocupation", tableInputData6, tableInputData7, tableInputData8)
        );
        ImGuiCellData someCell = new ImGuiCellData("First","a","b","c");
        ImGuiCellData someCell1 = new ImGuiCellData("Second", "d","e","f");
        ImGuiCellData someCell2 = new ImGuiCellData("Third", "g","h","i");
        m_cellTableData = new ImGuiCellTableData(someCell, someCell1, someCell2);
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
            var fullHeader = $"Mouse: {InputManager.MouseX}x{InputManager.MouseY} Driver: {m_sysInfo.SDLRenderInfo.CurrentRenderDriver} {fps}";
            var windowWidth = ImGui.GetWindowWidth();
            var textWidth = ImGui.CalcTextSize(fullHeader).X;
            ImGui.SameLine(windowWidth - textWidth - ImGui.GetStyle().ItemSpacing.X * 2);
            ImGui.Text(fullHeader);
            ImGui.EndMainMenuBar();
        }
    }
}
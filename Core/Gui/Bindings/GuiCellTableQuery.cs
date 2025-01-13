using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;

namespace SDL2Game.Core.Gui.Gui;

public class GuiCellTableQuery : GuiBase
{
    private string CELL_TABLE = "CellTable";
        
    private  Dictionary<string, List<(string Property, string Value)>> m_debugQueryData = new()
    {
        {
            "General State", new List<(string, string)>
            {
                ("NA", "NA")
            }
        },
    };
    
    private ImGuiCellTableData m_cellTableData;


    public GuiCellTableQuery(string cellTableName, IVariableBinder variableBinder) : base(variableBinder)
    {
        CELL_TABLE = cellTableName;
        BindDebugQuery();
    }

    public void UpdateDebugQuery(Dictionary<string, List<(string Property, string Value)>> debugQuery)
    {
        m_debugQueryData = debugQuery;
        BindDebugQuery();
    }

    public void DrawDebugQuery(IServiceGuiWindowBuilder guiWindowBuilder)
    {
        guiWindowBuilder.Draw(CELL_TABLE);
    }

    private void BindDebugQuery()
    {
        var cells = new List<ImGuiCellData>();
        foreach (var item in m_debugQueryData)
        {
            string header = item.Key;
            List<(string Property, string Value)> properties = item.Value;
            List<string> values = new List<string>();
            foreach (var prop in properties)
            {
                values.Add($"{prop.Property}: {prop.Value}");
            }

            cells.Add(new ImGuiCellData(header, values.ToArray()));
        }

        var cellTableData = new ImGuiCellTableData(cells.ToArray());
        m_variableBinder.BindVariable(CELL_TABLE, cellTableData);
    }
}
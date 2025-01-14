using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;

namespace SDL2Game.Core.Gui.Gui;

public class GuiStringQuery<T> : GuiBase where T : IConvertible
{
    private string GUI_ID = "PokemonCount";
    private T m_value;

    public GuiStringQuery(string id, IVariableBinder variableBinder) : base(variableBinder)
    {
        GUI_ID = id;
        m_value = default(T);
        BindQuery();
    }

    public void UpdateQuery(T queryVal) 
    {
        if (typeof(T) == typeof(int))
        {
            m_value = (T)Convert.ChangeType(
                Convert.ToInt32(m_value) + Convert.ToInt32(queryVal), 
                typeof(T)
            );
        }
        else
        {
            m_value = queryVal;
        }
        
        BindQuery();
    }

    public void DrawQuery(IGuiWindowBuilder guiWindowBuilder)
    {
        guiWindowBuilder.Draw(GUI_ID);
    }
    
    private void BindQuery()
    {
        m_variableBinder.BindVariable(GUI_ID, m_value);
    }

}
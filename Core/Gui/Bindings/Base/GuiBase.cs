using SDL2Engine.Core.GuiRenderer;

namespace SDL2Game.Core.Gui.Gui;

public class GuiBase
{
    protected IVariableBinder m_variableBinder;

    public GuiBase(IVariableBinder variableBinder)
    {
        m_variableBinder = variableBinder;
    }
}
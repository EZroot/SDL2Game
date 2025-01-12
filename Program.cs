using SDL2Engine.Core;
using SDL2Game;

public static class Program
{
    public static void Main()
    {
        var app = new GameApp();
        app.Run(new MyGame());
    }
}
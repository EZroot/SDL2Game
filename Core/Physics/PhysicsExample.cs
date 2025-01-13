using System.Numerics;
using Box2DSharp.Dynamics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Game.Core.Gui;
using SDL2Game.Core.Utils;

namespace SDL2Game.Core.Physics;

public class PhysicsExample
{
    private IServicePhysicsService m_physicsService;
    private IServiceRenderService m_renderService;
    private IServiceAssetManager m_assetManager;
    private IServiceWindowConfig m_windowConfig;

    // Physics gameobject
    private GameObject m_boxObject;
    private List<GameObject> m_boxes = new List<GameObject>();
    
    private int m_prevWindowX, m_prevWindowY;
    private bool isDragging = false;
    private int dragStartX, dragStartY, prevMouseX, prevMouseY;

    public PhysicsExample(IServicePhysicsService physicsService,
        IServiceRenderService renderService, 
        IServiceAssetManager assetManager,
        IServiceWindowConfig windowConfig)
    {
        m_renderService = renderService;
        m_assetManager = assetManager;
        m_physicsService = physicsService;
        m_windowConfig = windowConfig;
    }
    
    public void Update(float deltaTime, GuiExample guiExample)
    {
        HandleMouseDragAndApplyForce();
        SpawnBoxes(guiExample);
        MouseQueryCollision(InputManager.MouseX, InputManager.MouseY, guiExample);

        foreach (var box in m_boxes)
        {
            box.Update(deltaTime);
        }
    }

    public void Render(nint renderPtr)
    {
        foreach (var box in m_boxes)
        {
            box.Render(renderPtr, m_assetManager);
        }
    }

    private void MouseQueryCollision(int mouseX, int mouseY, GuiExample example)
    {
        var body = m_physicsService.CollisionDetector.GetBodyUnderPoint(mouseX, mouseY);
        if (body != null)
        {
            var bodyInfo = new string[,]
            {
                { "IsEnabled", body.IsEnabled.ToString() },
                { "IsAwake", body.IsAwake.ToString() },
                { "IsBullet", body.IsBullet.ToString() },
                { "BodyType", body.BodyType.ToString() },
                { "Mass", body.Mass.ToString() },
                { "Inertia", body.Inertia.ToString() },
                { "AngularDamping", body.AngularDamping.ToString() },
                { "AngularVelocity", body.AngularVelocity.ToString() },
                { "LinearDamping", body.LinearDamping.ToString() },
                { "LinearVelocity", body.LinearVelocity.ToString() }
            };
            example.UpdateDebugQuery(bodyInfo);
        }
        else
        {
            example.UpdateDebugQuery(new string[,] { {"None","N/A"}});
        }
    }

    private void SpawnBoxes(GuiExample guiExample)
    {
        // If space is pressed, spawn multiple boxes
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_SPACE))
        {
            Debug.Log("<color=yellow>SPACE pressed! Spawning 10 'jigglypuff' boxes...</color>");

            var boxTexture = m_assetManager.LoadTexture(m_renderService.RenderPtr, GameHelper.RESOURCES_FOLDER + "/jigglypuff.png");
            Debug.Log($"Loaded Texture Id: {boxTexture.Id}, Size: {boxTexture.Width}x{boxTexture.Height}");

            var rnd = new Random();
            int numberOfBoxes = 10;
            for (int i = 0; i < numberOfBoxes; i++)
            {
                float xPos = rnd.Next(50, m_windowConfig.Settings.Width - 50);
                float yPos = 100; // start them near the top

                var boxObject = new GameObject
                {
                    TextureId = boxTexture.Id,
                    Position = new Vector2(xPos, yPos),
                    OriginalWidth = boxTexture.Width,
                    OriginalHeight = boxTexture.Height,
                    Scale = Vector2.One
                };

                float widthMeters = boxObject.OriginalWidth / 3;
                float heightMeters = boxObject.OriginalHeight / 3;

                Debug.Log($"Box #{i} -> Pos({xPos}, {yPos}), Size({widthMeters}x{heightMeters}), Registering as DynamicBody...");

                m_physicsService.RegisterGameObject(
                    boxObject,
                    widthMeters,
                    heightMeters,
                    BodyType.DynamicBody
                );
                guiExample.UpdatePokemonCount(1);
                m_boxes.Add(boxObject);
                Debug.Log($"Box #{i} registered successfully!");
            }
            Debug.Log("<color=yellow>Done spawning boxes!</color>");
        }

    }
    
    private void HandleMouseDragAndApplyForce()
    {
        var mouseX = InputManager.MouseX;
        var mouseY = InputManager.MouseY;

        if (InputManager.IsMouseButtonPressed(SDL.SDL_BUTTON_LEFT) && !isDragging)
        {
            isDragging = true;
            dragStartX = mouseX;
            dragStartY = mouseY;
            prevMouseX = mouseX;
            prevMouseY = mouseY;
        }

        if (isDragging)
        {
            int deltaX = mouseX - prevMouseX;
            int deltaY = mouseY - prevMouseY;

            for (var index = 0; index < m_boxes.Count; index++)
            {
                if (index > 20) break;
                var box = m_boxes[index];
                Vector2 force = new Vector2(deltaX, deltaY);
                box.PhysicsBody.ApplyForce(force, box.PhysicsBody.GetWorldCenter(), true);
            }

            prevMouseX = mouseX;
            prevMouseY = mouseY;
        }

        if (!InputManager.IsMouseButtonPressed(SDL.SDL_BUTTON_LEFT) && isDragging)
        {
            isDragging = false;
        }
    }

}
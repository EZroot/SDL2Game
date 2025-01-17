using System.Numerics;
using Box2DSharp.Dynamics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
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
    private IPhysicsService m_physicsService;
    private IRenderService m_renderService;
    private IImageService m_imageService;
    private IWindowConfig m_windowConfig;

    // Physics gameobject
    private GameObject m_boxObject;
    private List<GameObject> m_boxes = new List<GameObject>();
    
    private int m_prevWindowX, m_prevWindowY;
    private bool isDragging = false;
    private int dragStartX, dragStartY, prevMouseX, prevMouseY;

    public PhysicsExample(IPhysicsService physicsService,
        IRenderService renderService, 
        IImageService assetManager,
        IWindowConfig windowConfig)
    {
        m_renderService = renderService;
        m_imageService = assetManager;
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
            box.Render(renderPtr);
        }
    }

    private void MouseQueryCollision(int mouseX, int mouseY, GuiExample example)
    {
        var bodyInfo = new Dictionary<string, List<(string Property, string Value)>>()
        {
            {
                "General State", new List<(string, string)>
                {
                    ("NA", "NA")
                }
            },
        };

        var body = m_physicsService.CollisionDetector.GetBodyUnderPoint(mouseX, mouseY);
        if (body != null)
        {
            bodyInfo = new Dictionary<string, List<(string Property, string Value)>>()
            {
                {
                    "General State", new List<(string, string)>
                    {
                        ("IsEnabled", body.IsEnabled.ToString()),
                        ("IsAwake", body.IsAwake.ToString()),
                        ("IsBullet", body.IsBullet.ToString()),
                        ("BodyType", body.BodyType.ToString())
                    }
                },
                {
                    "Physical Properties", new List<(string, string)>
                    {
                        ("Mass", body.Mass.ToString()),
                        ("Inertia", body.Inertia.ToString()),
                    }
                },
                {
                    "Dynamic Properties", new List<(string, string)>
                    {
                        ("AngularDamping", body.AngularDamping.ToString()),
                        ("AngularVelocity", body.AngularVelocity.ToString()),
                        ("LinearDamping", body.LinearDamping.ToString()),
                        ("LinearVelocity", body.LinearVelocity.ToString())
                    }
                },
                // {
                //     "World", new List<(string, string)>
                //     {
                //         ("BodyCount", body.World.BodyCount.ToString()),
                //         ("Gravity", body.World.Gravity.ToString()),
                //         ("ContactCount", body.World.ContactCount.ToString()),
                //     }
                // }
            };
        }
        example.UpdateDebugQuery(body != null, bodyInfo);
    }

    private void SpawnBoxes(GuiExample guiExample)
    {
        // If space is pressed, spawn multiple boxes
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_SPACE))
        {
            Debug.Log("<color=yellow>SPACE pressed! Spawning 10 'pinkyboy' boxes...</color>");

            var boxTexture = m_imageService.LoadTexture(m_renderService.RenderPtr, GameHelper.RESOURCES_FOLDER + "/pinkboy.png");
            Debug.Log($"Loaded Texture Id: {boxTexture.Id}, Size: {boxTexture.Width}x{boxTexture.Height}");

            // var sprite = new StaticSprite(boxTexture.Texture, boxTexture.Width, boxTexture.Height);
            var sprite = new AnimatedSprite(boxTexture.Texture, 32, 32, 4, 0.5f);
            
            var rnd = new Random();
            int numberOfBoxes = 1;
            for (int i = 0; i < numberOfBoxes; i++)
            {
                float xPos = rnd.Next(150, m_windowConfig.Settings.Width - 150);
                float yPos = 100; // start them near the top

                var boxObject = new GameObject(sprite: sprite, position: new Vector2(xPos, yPos), scale: Vector2.One);

                float widthMeters = boxObject.Sprite.Width * 0.75f;
                float heightMeters = boxObject.Sprite.Height * 0.75f;

                Debug.Log($"Box #{i} -> Pos({xPos}, {yPos}), Size({widthMeters}x{heightMeters}), Registering as DynamicBody...");

                m_physicsService.RegisterGameObject(
                    boxObject,
                    widthMeters,
                    heightMeters,
                    BodyType.DynamicBody
                );
                guiExample.UpdatePinkboyCount(1);
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
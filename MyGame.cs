using System.Numerics;
using Box2DSharp.Dynamics;
using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Core.Windowing.Interfaces;

namespace SDL2Game;

public class MyGame : IGame
{
    private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL_Engine/SDL2Engine/resources";
    private const string SOUND_FOLDER = "/home/anon/Music";

    private const int ENGINE_VOLUME = 24; // 0 - 128

    private IServiceRenderService m_renderService;
    private IServiceAssetManager m_assetManager;
    private IServiceAudioLoader m_audioLoader;
    private IServiceWindowConfig m_windowConfig;
    private IServiceCameraService m_cameraService;
    private IServicePhysicsService m_physicsService;

    private nint m_renderer;
    private PokemonHandler m_pokemonHandler;
    private AudioSynthesizer m_audioSynthesizer;
    private float minHue = 0.7f, maxHue = 0.85f;
    private float maxHueSeperation = 0.25f;
    
    // Physics gameobject
    private GameObject m_boxObject;
    private List<GameObject> m_boxes = new List<GameObject>();
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        m_renderService = serviceProvider.GetService<IServiceRenderService>() 
                          ?? throw new InvalidOperationException("IServiceRenderService is required but not registered.");
        m_assetManager = serviceProvider.GetService<IServiceAssetManager>() 
                         ?? throw new InvalidOperationException("IServiceAssetManager is required but not registered.");
        m_audioLoader = serviceProvider.GetService<IServiceAudioLoader>() 
                        ?? throw new InvalidOperationException("IServiceAudioLoader is required but not registered.");
        m_cameraService = serviceProvider.GetService<IServiceCameraService>() 
                          ?? throw new InvalidOperationException("IServiceCameraService is required but not registered.");
        m_windowConfig = serviceProvider.GetService<IServiceWindowConfig>() 
                         ?? throw new InvalidOperationException("IServiceWindowConfig is required but not registered.");
        m_physicsService = serviceProvider.GetService<IServicePhysicsService>() 
                           ?? throw new InvalidOperationException("IServicePhysicsService is required but not registered.");

        Initialize();
        Debug.Log("Initialized MyGame!");
    }

    private void Initialize()
    {
        m_renderer = m_renderService.GetRenderer();
        
        m_pokemonHandler = new PokemonHandler(m_audioLoader, m_assetManager, m_cameraService);
        m_pokemonHandler.Initialize(m_renderer);

        m_audioSynthesizer = new AudioSynthesizer(
            m_windowConfig.Settings.Width,
            m_windowConfig.Settings.Height,
            m_audioLoader);
        
        m_audioSynthesizer.Initialize(rectWidth: 4, rectMaxHeight: 75, rectSpacing: 4, bandIntensity: 3f);

        
        var songPath = SOUND_FOLDER + "/pokemon.wav"; //"/skidrow-portal.wav"; 

        // Music Test
        // var song = m_assetManager.LoadSound(songPath, Addressables.AudioLoader.AudioType.Music);
        // m_assetManager.PlaySound(song, 16, true);

        // Soundfx Test
        var song = m_assetManager.LoadSound(songPath);
        m_assetManager.PlaySound(song, ENGINE_VOLUME);
        
    }

    public void Update(float deltaTime)
    {
        // If space is pressed, spawn multiple boxes
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_SPACE))
        {
            Debug.Log("<color=yellow>SPACE pressed! Spawning 10 'jigglypuff' boxes...</color>");

            var boxTexture = m_assetManager.LoadTexture(m_renderer, RESOURCES_FOLDER + "/jigglypuff.png");
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

                m_boxes.Add(boxObject);
                Debug.Log($"Box #{i} registered successfully!");
            }

            Debug.Log("<color=yellow>Done spawning boxes!</color>");
        }

        foreach (var box in m_boxes)
        {
            box.Update(deltaTime);
        }

        m_pokemonHandler.Update();
    }

    public void Render()
    {
        minHue += Time.DeltaTime * 0.01f;
        maxHue += Time.DeltaTime * 0.01f;
        if (minHue >= 1.0f) minHue -= 1.0f;
        if (maxHue >= 1.0f) maxHue -= 1.0f;
        if (maxHue > minHue + maxHueSeperation) maxHue = minHue + maxHueSeperation;
        if (minHue > maxHue - maxHueSeperation) minHue = maxHue - maxHueSeperation;

        m_pokemonHandler.Render(m_renderer);
        m_audioSynthesizer.Render(m_renderer, minHue, maxHue);
        
        foreach (var box in m_boxes)
        {
            box.Render(m_renderer, m_assetManager);
        }
    }

    public void Shutdown()
    {
        m_pokemonHandler.CleanUp();
    }
}
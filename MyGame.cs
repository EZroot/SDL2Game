using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
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

    private nint m_renderer;
    private Pokemanz m_pokemanz;
    private AudioSynthesizer m_audioSynthesizer;
    private float minHue = 0.7f, maxHue = 0.85f;
    private float maxHueSeperation = 0.25f;
    
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

        Initialize();
        Debug.Log("Initialized MyGame!");
    }

    private void Initialize()
    {
        m_renderer = m_renderService.GetRenderer();
        
        m_pokemanz = new Pokemanz(m_audioLoader, m_assetManager, m_cameraService);
        m_pokemanz.Initialize(m_renderer);

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
        m_pokemanz.Update();
    }
    
    public void Render()
    {
        minHue += Time.DeltaTime * 0.01f;
        maxHue += Time.DeltaTime * 0.01f;
        if (minHue >= 1.0f) minHue -= 1.0f;
        if (maxHue >= 1.0f) maxHue -= 1.0f;
        if (maxHue > minHue + maxHueSeperation) maxHue = minHue + maxHueSeperation;
        if (minHue > maxHue - maxHueSeperation) minHue = maxHue - maxHueSeperation;

        m_pokemanz.Render(m_renderer);
        m_audioSynthesizer.Render(m_renderer, minHue, maxHue);
    }

    public void Shutdown()
    {
        m_pokemanz.CleanUp();
    }
}
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
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

    private int m_windowWidth, m_windowHeight;

    private IServiceWindowService m_windowService;
    private IServiceRenderService m_renderService;
    private IServiceGuiRenderService m_guiRenderService;
    private IServiceGuiWindowService m_guiWindowBuilder;
    private IVariableBinder m_guiVariableBinder;
    private IServiceAssetManager m_assetManager;
    private IServiceAudioLoader m_audioLoader;
    private IServiceCameraService m_cameraService;

    private Pokemanz m_pokemanz;
    private AudioSynthesizer m_audioSynthesizer;
    private nint m_renderer;
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        // Resolve services from the service provider
        m_windowService = serviceProvider.GetService<IServiceWindowService>() 
                          ?? throw new InvalidOperationException("IServiceWindowService is required but not registered.");
        m_renderService = serviceProvider.GetService<IServiceRenderService>() 
                          ?? throw new InvalidOperationException("IServiceRenderService is required but not registered.");
        m_guiRenderService = serviceProvider.GetService<IServiceGuiRenderService>() 
                             ?? throw new InvalidOperationException("IServiceGuiRenderService is required but not registered.");
        m_guiWindowBuilder = serviceProvider.GetService<IServiceGuiWindowService>() 
                             ?? throw new InvalidOperationException("IServiceGuiWindowService is required but not registered.");
        m_guiVariableBinder = serviceProvider.GetService<IVariableBinder>() 
                              ?? throw new InvalidOperationException("IVariableBinder is required but not registered.");
        m_assetManager = serviceProvider.GetService<IServiceAssetManager>() 
                         ?? throw new InvalidOperationException("IServiceAssetManager is required but not registered.");
        m_audioLoader = serviceProvider.GetService<IServiceAudioLoader>() 
                        ?? throw new InvalidOperationException("IServiceAudioLoader is required but not registered.");
        m_cameraService = serviceProvider.GetService<IServiceCameraService>() 
                          ?? throw new InvalidOperationException("IServiceCameraService is required but not registered.");

        Initialize();
        // Log that initialization is complete
        Debug.Log("Initialized MyGame!");
    }

    private void Initialize()
    {
        m_renderer = m_renderService.GetRenderer();
        
        m_pokemanz = new Pokemanz(m_audioLoader, m_assetManager, m_cameraService);
        m_pokemanz.Initialize(m_renderer);

        m_audioSynthesizer = new AudioSynthesizer(m_audioLoader);
        m_audioSynthesizer.Initialize();

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
        m_pokemanz.Render(m_renderer);
        m_audioSynthesizer.Render(m_renderer);
    }

    public void Shutdown()
    {
        m_pokemanz.CleanUp();
    }
}
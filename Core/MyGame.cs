using System.Numerics;
using Box2DSharp.Dynamics;
using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Game.Core.AudioSynth;
using SDL2Game.Core.Gui;
using SDL2Game.Core.Physics;
using SDL2Game.Core.Pokemans;
using SDL2Game.Core.Utils;

namespace SDL2Game.Core;

public class MyGame : IGame
{

    #region Services

    private IServiceWindowService m_windowService;
    private IServiceRenderService m_renderService;
    private IServiceGuiRenderService m_guiRenderService;
    private IServiceGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_guiVarBinder;
    private IServiceAssetManager m_assetManager;
    private IServiceAudioLoader m_audioLoader;
    private IServiceWindowConfig m_windowConfig;
    private IServiceCameraService m_cameraService;
    private IServicePhysicsService m_physicsService;
    private IServiceSysInfo m_sysInfo;

    #endregion

    private PokemonHandler m_pokemonHandler;
    private AudioSynthesizer m_audioSynthesizer;
    private GuiExample m_guiExample;
    private PhysicsExample m_physicsExample;

    private float minHue = 0.7f, maxHue = 0.85f;
    private float maxHueSeperation = 0.25f;

    public void Initialize(IServiceProvider serviceProvider)
    {
        #region Setup Services
        m_windowService = serviceProvider.GetService<IServiceWindowService>() ?? throw new InvalidOperationException("IServiceWindowService is required but not registered.");
        m_renderService = serviceProvider.GetService<IServiceRenderService>() ?? throw new InvalidOperationException("IServiceRenderService is required but not registered.");
        m_guiRenderService = serviceProvider.GetService<IServiceGuiRenderService>() ?? throw new InvalidOperationException("IServiceGuiRenderService is required but not registered.");
        m_assetManager = serviceProvider.GetService<IServiceAssetManager>() ?? throw new InvalidOperationException("IServiceAssetManager is required but not registered.");
        m_audioLoader = serviceProvider.GetService<IServiceAudioLoader>() ?? throw new InvalidOperationException("IServiceAudioLoader is required but not registered.");
        m_cameraService = serviceProvider.GetService<IServiceCameraService>() ?? throw new InvalidOperationException("IServiceCameraService is required but not registered.");
        m_windowConfig = serviceProvider.GetService<IServiceWindowConfig>() ?? throw new InvalidOperationException("IServiceWindowConfig is required but not registered.");
        m_physicsService = serviceProvider.GetService<IServicePhysicsService>() ?? throw new InvalidOperationException("IServicePhysicsService is required but not registered.");
        m_sysInfo = serviceProvider.GetService<IServiceSysInfo>() ?? throw new InvalidOperationException("IServiceSysInfo is required but not registered.");
        m_guiWindowBuilder = serviceProvider.GetService<IServiceGuiWindowBuilder>() ?? throw new InvalidOperationException("IServiceGuiWindowBuilder is required but not registered.");
        m_guiVarBinder = serviceProvider.GetService<IVariableBinder>() ?? throw new InvalidOperationException("IVariableBinder is required but not registered.");
        #endregion

        InitializeInternal();
        Debug.Log("Initialized MyGame!");
    }

    private void InitializeInternal()
    {
        m_guiExample = new GuiExample(m_guiRenderService, m_guiWindowBuilder, m_guiVarBinder, m_sysInfo);
        m_physicsExample = new PhysicsExample(m_physicsService, m_renderService, m_assetManager, m_windowConfig);
        m_pokemonHandler = new PokemonHandler(m_audioLoader, m_assetManager, m_cameraService);
        m_audioSynthesizer = new AudioSynthesizer(
            m_windowConfig.Settings.Width,
            m_windowConfig.Settings.Height,
            m_audioLoader);

        m_pokemonHandler.Initialize(m_renderService.RenderPtr);
        m_audioSynthesizer.Initialize(rectWidth: 4, rectMaxHeight: 75, rectSpacing: 4, bandIntensity: 3f);

        var songPath = GameHelper.SOUND_FOLDER + "/skidrow.wav";//"/pokemon.wav";  
        var song = m_assetManager.LoadSound(songPath);
        m_assetManager.PlaySound(song, GameHelper.GLOBAL_VOLUME);
    }

    public void Update(float deltaTime)
    {
        m_physicsExample.Update(deltaTime, m_guiExample);
        m_pokemonHandler.Update(deltaTime);
    }

    public void Render()
    {
        minHue += Time.DeltaTime * 0.01f;
        maxHue += Time.DeltaTime * 0.01f;
        if (minHue >= 1.0f) minHue -= 1.0f;
        if (maxHue >= 1.0f) maxHue -= 1.0f;
        if (maxHue > minHue + maxHueSeperation) maxHue = minHue + maxHueSeperation;
        if (minHue > maxHue - maxHueSeperation) minHue = maxHue - maxHueSeperation;

        m_pokemonHandler.Render(m_renderService.RenderPtr);
        m_physicsExample.Render(m_renderService.RenderPtr);
        m_audioSynthesizer.Render(m_renderService.RenderPtr, minHue, maxHue);
    }

    public void RenderGui()
    {
        m_guiExample.RenderGui();
    }

    public void Shutdown()
    {
        m_pokemonHandler.CleanUp();
    }
}
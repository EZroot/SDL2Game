using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Addressables.Fonts;
using SDL2Engine.Core.Addressables.Fonts.Interfaces;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.AI;
using SDL2Engine.Core.AI.Data;
using SDL2Engine.Core.CoreSystem.Configuration;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Interfaces;
using SDL2Engine.Core.Networking.Interfaces;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Partitions.Interfaces;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Game.Core.AudioSynth;
using SDL2Game.Core.Gui;
using SDL2Game.Core.Networking;
using SDL2Game.Core.Physics;
using SDL2Game.Core.Pinkboyz;
using SDL2Game.Core.Utils;

namespace SDL2Game.Core;

public class MyGame : IGame
{
    #region Services

    private IWindowService m_windowService;
    private IRenderService m_renderService;
    private IGuiRenderService m_guiRenderService;
    private IGuiWindowBuilder m_guiWindowBuilder;
    private IVariableBinder m_guiVarBinder;
    private IAudioService m_audioService;
    private IImageService m_imageService;
    private IWindowConfig m_windowConfig;
    private ICameraService m_cameraService;
    private IPhysicsService m_physicsService;
    private ISysInfo m_sysInfo;
    private INetworkService m_networkService;
    private IFontService m_fontService;

    #endregion

    private PinkBoysHandler m_pinkBoysHandler;
    private AudioSynthesizer m_audioSynthesizer;
    private GuiExample m_guiExample;
    private PhysicsExample m_physicsExample;
    private NetworkExample m_networkExample;
    
    private IPartitioner m_partitioner;
    private BoidManager m_boidManager;

    private nint m_fontPointr;
    private FontTexture m_testFontTexture;
    private SpriteFontTexture m_testFontSpriteTexture;

    private const int BoidCount = 10000;
    private const float WorldSize = 1024;
    private const float BoidSpeed = 40f;
    private const int SpatialPartitionerSize = 32;
    
    private float minHue = 0.7f, maxHue = 0.85f;
    private float maxHueSeperation = 0.25f;

    public void Initialize(IServiceProvider serviceProvider)
    {
        #region Setup Services
        m_windowService = serviceProvider.GetService<IWindowService>() ?? throw new InvalidOperationException("IServiceWindowService is required but not registered.");
        m_renderService = serviceProvider.GetService<IRenderService>() ?? throw new InvalidOperationException("IServiceRenderService is required but not registered.");
        m_guiRenderService = serviceProvider.GetService<IGuiRenderService>() ?? throw new InvalidOperationException("IServiceGuiRenderService is required but not registered.");
        m_audioService = serviceProvider.GetService<IAudioService>() ?? throw new InvalidOperationException("IAudioService is required but not registered.");
        m_imageService = serviceProvider.GetService<IImageService>() ?? throw new InvalidOperationException("IImageService is required but not registered.");
        m_cameraService = serviceProvider.GetService<ICameraService>() ?? throw new InvalidOperationException("IServiceCameraService is required but not registered.");
        m_windowConfig = serviceProvider.GetService<IWindowConfig>() ?? throw new InvalidOperationException("IServiceWindowConfig is required but not registered.");
        m_physicsService = serviceProvider.GetService<IPhysicsService>() ?? throw new InvalidOperationException("IServicePhysicsService is required but not registered.");
        m_sysInfo = serviceProvider.GetService<ISysInfo>() ?? throw new InvalidOperationException("IServiceSysInfo is required but not registered.");
        m_guiWindowBuilder = serviceProvider.GetService<IGuiWindowBuilder>() ?? throw new InvalidOperationException("IServiceGuiWindowBuilder is required but not registered.");
        m_guiVarBinder = serviceProvider.GetService<IVariableBinder>() ?? throw new InvalidOperationException("IVariableBinder is required but not registered.");
        m_networkService = serviceProvider.GetService<INetworkService>() ?? throw new InvalidOperationException("INetworkService is required but not registered.");
        m_fontService = serviceProvider.GetService<IFontService>() ?? throw new InvalidOperationException("IFontService is required but not registered.");
        #endregion

        m_partitioner = new SpatialPartitioner(SpatialPartitionerSize);
        InitializeInternal();
        Debug.Log("Initialized MyGame!");
        InitializeBoids();
    }
    
    private void InitializeBoids()
    {
        var renderer = m_renderService.RenderPtr;
        var boidTexture = m_imageService.LoadTexture(renderer, GameHelper.RESOURCES_FOLDER + "/pinkboy.png");

        var boidGroupData = new List<BoidGroupData>();
        var random = new Random();

        for (int i = 0; i < BoidCount; i++)
        {
            var boidSprite = new AnimatedSprite(boidTexture.Texture, 32, 32, 4, 0.5f);

            var position = new Vector2(
                (float)(random.NextDouble() * WorldSize),
                (float)(random.NextDouble() * WorldSize)
            );

            var scale = new Vector2(1, 1);

            boidGroupData.Add(new BoidGroupData(sprite: boidSprite, position: position, scale: scale));
        }

        m_boidManager = new BoidManager(m_partitioner as SpatialPartitioner, WorldSize, BoidSpeed);
        m_boidManager.InitializeBoids(boidGroupData.ToArray());
    }

    
    private void InitializeInternal()
    {
        m_guiExample = new GuiExample(m_guiRenderService, m_guiWindowBuilder, m_guiVarBinder, m_sysInfo);
        m_physicsExample = new PhysicsExample(m_physicsService, m_renderService, m_imageService, m_windowConfig);
        m_pinkBoysHandler = new PinkBoysHandler(m_audioService, m_imageService, m_cameraService, m_partitioner);
        m_audioSynthesizer = new AudioSynthesizer(
            m_windowConfig.Settings.Width,
            m_windowConfig.Settings.Height,
            m_audioService);
        m_networkExample = new NetworkExample(m_networkService, m_guiRenderService, m_guiWindowBuilder, m_guiVarBinder);
        m_networkExample.Initialize();

        m_pinkBoysHandler.Initialize(m_renderService.RenderPtr);
        m_audioSynthesizer.Initialize(rectWidth: 4, rectMaxHeight: 75, rectSpacing: 4, bandIntensity: 3f);

        var songPath = GameHelper.SOUND_FOLDER + "/skidrow.wav";//"/pokemon.wav";"
        var song = m_audioService.LoadSound(songPath);
        m_audioService.PlaySound(song, GameHelper.GLOBAL_VOLUME);

        m_fontPointr = m_fontService.LoadFont(GameHelper.RESOURCES_FOLDER + "/fonts/retrogaming.ttf");
        m_testFontSpriteTexture = m_fontService.LoadSpriteFont(
            GameHelper.RESOURCES_FOLDER + "/fonts/pinkyboyfont-sheet.png",
            32,
            32,
            44);
    }

    public void Update(float deltaTime)
    {
        m_physicsExample.Update(deltaTime, m_guiExample);
        m_pinkBoysHandler.Update(deltaTime);
        m_boidManager.UpdateBoids(deltaTime);
        m_testFontTexture = m_fontService.CreateFontTexture(m_fontPointr, $"THIS IS A FONT TEST DeltaTime: ({deltaTime})", new SDL.SDL_Color() { r = 255, g = 255, b = 255 }, (300,200));

    }

    public void Render()
    {
        minHue += Time.DeltaTime * 0.01f;
        maxHue += Time.DeltaTime * 0.01f;
        if (minHue >= 1.0f) minHue -= 1.0f;
        if (maxHue >= 1.0f) maxHue -= 1.0f;
        if (maxHue > minHue + maxHueSeperation) maxHue = minHue + maxHueSeperation;
        if (minHue > maxHue - maxHueSeperation) minHue = maxHue - maxHueSeperation;

        m_pinkBoysHandler.Render(m_renderService.RenderPtr);
        m_physicsExample.Render(m_renderService.RenderPtr);
        m_audioSynthesizer.Render(m_renderService.RenderPtr, minHue, maxHue);
        m_boidManager.RenderBoids(m_renderService.RenderPtr, m_cameraService);
        m_fontService.RenderFont(m_testFontTexture);
        m_fontService.RenderStringSprite(m_testFontSpriteTexture, $"this is a test!!! .,?><+-= {Time.DeltaTime}", (200,50), -10);
        // m_partitioner.RenderDebug(m_renderService.RenderPtr, m_cameraService);
    }

    public void RenderGui()
    {
        m_networkExample.RenderGui();
        m_guiExample.RenderGui();
    }

    public void Shutdown()
    {
        m_networkExample.Shutdown();
        m_pinkBoysHandler.CleanUp();
        m_fontService.CleanupFontTexture(m_testFontTexture);
    }
}
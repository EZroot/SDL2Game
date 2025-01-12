using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Game;

public class Pokemanz
{
    private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL_Engine/SDL2Engine/resources";

    private Vector2 m_position, m_startPosition, m_originalScale;
    private SDL.SDL_Rect m_dstRectAsh;
    private AssetManager.TextureData m_spriteTexture;
    private float m_currentScale;
    private AssetManager.TextureData[] m_spriteTexturePokemans;
    private List<Vector2> m_originalScales = new List<Vector2>();
    private List<float> m_currScales = new List<float>();
    private List<SDL.SDL_Rect> m_dstRects = new List<SDL.SDL_Rect>();

    private IServiceAudioLoader m_audioLoader;
    private IServiceAssetManager m_assetManager;
    private IServiceCameraService m_cameraService;

    public Pokemanz(IServiceAudioLoader audioLoader, IServiceAssetManager assetManager,
        IServiceCameraService cameraService)
    {
        m_audioLoader = audioLoader;
        m_assetManager = assetManager;
        m_cameraService = cameraService;
    }
    
    public void Initialize(nint renderer)
    {
                //Sprite Test
        m_spriteTexture = m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ashh.png");
        m_dstRectAsh = new SDL.SDL_Rect { x = 0, y = 0, w = m_spriteTexture.Width, h = m_spriteTexture.Height };
        m_startPosition = new Vector2(48, 174);
        m_originalScale = new Vector2(m_spriteTexture.Width, m_spriteTexture.Height);
        m_position = m_startPosition;

        //Lil pokemans
        m_spriteTexturePokemans = new AssetManager.TextureData[]
        {
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/charizard.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/gengar.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/jigglypuff.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/moltres.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/squirtle.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ninetales.png"),
            m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/poliwhirl.png"),
        };


        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            var width = m_spriteTexturePokemans[i].Width;
            var height = m_spriteTexturePokemans[i].Height;
            int row = i / 10;
            int col = i % 10;
            var spacing = -20;
            var startPos = new Vector2(330 + (col * (width + spacing)), 0 + (row * (height + spacing)));

            // var startPos = new Vector2(150 + (i*width + width + 64), 60 * j+i);
            var rec = new SDL.SDL_Rect { x = (int)startPos.X, y = (int)startPos.Y, w = width, h = height };
            m_dstRects.Add(rec);
            m_currScales.Add(1.0f);
            m_originalScales.Add(new Vector2(width, height));
        }
    }

    public void Update()
    {
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
            m_position.Y -= 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
            m_position.X -= 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
            m_position.Y += 20f * Time.DeltaTime;
        if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
            m_position.X += 20f * Time.DeltaTime;

        if (m_dstRectAsh.x != (int)m_position.X || m_dstRectAsh.y != (int)m_position.Y)
        {
            // Debug.Log($"X:{position.X} Y:{position.Y}");
            m_dstRectAsh.x = (int)(m_position.X);
            m_dstRectAsh.y = (int)(m_position.Y);
        }
    }
    
    public void Render(nint renderer)
    {
        #region DrawPokemons

        var baseScale = 0.75f;
        var amplitude = 0f;

        // Trying to grab some vocals here
        for (var i = 4; i < 16; i++)
        {
            var index = i.ToString();
            amplitude += m_audioLoader.GetAmplitudeByName(index);
        }

        var scaleFactor = baseScale + amplitude; // A highre freq band, to hopefully grab vocals
        m_currentScale = MathHelper.Lerp(m_currentScale, scaleFactor, 0.1f);
        var maxScale = 3f;
        m_currentScale = Math.Min(m_currentScale, maxScale);
        m_dstRectAsh.w = (int)(m_originalScale.X * m_currentScale);
        m_dstRectAsh.h = (int)(m_originalScale.Y * m_currentScale);

        var pokemansBaseScale = 0.5f;
        var pokemansMaxScale = 5f;

        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            var pulseOffset = (i * 0.5f) % MathHelper.TwoPi;
            // Grab low freq band for the bass
            var dynamicScaleFactor = pokemansBaseScale
                                     + m_audioLoader.GetAmplitudeByName("1")
                                     * (5f + (float)Math.Sin(Time.TotalTime + pulseOffset));

            m_currScales[i] = MathHelper.Lerp(m_currScales[i], dynamicScaleFactor, 0.1f);
            m_currScales[i] = Math.Min(m_currScales[i], pokemansMaxScale);
            var ogScale = m_originalScales[i];
            var bounceX = 10f * (float)Math.Sin(Time.TotalTime * 2f + i * 0.5f);
            var bounceY = 5f * (float)Math.Cos(Time.TotalTime * 3f + i * 0.3f);
            var rec = m_dstRects[i];
            rec.w = (int)(ogScale.X * m_currScales[i]);
            rec.h = (int)(ogScale.Y * m_currScales[i]);
            rec.x += (int)bounceX;
            rec.y += (int)bounceY;
            var camera = m_cameraService.GetActiveCamera();
            m_assetManager.DrawTexture(renderer, m_spriteTexturePokemans[i].Id, ref rec, camera);
        }

        m_assetManager.DrawTexture(renderer, m_spriteTexture.Id, ref m_dstRectAsh);

        #endregion
    }

    public void CleanUp()
    {
        m_assetManager.UnloadTexture(m_spriteTexture.Id);
        for (var i = 0; i < m_spriteTexturePokemans.Length; i++)
        {
            m_assetManager.UnloadTexture(m_spriteTexturePokemans[i].Id);
        }
    }
}
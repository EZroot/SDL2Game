using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Game.Core.Pokemans.GameObjects;

namespace SDL2Game.Core.Pokemans
{
    public class PokemonHandler
    {
        private const string RESOURCES_FOLDER = "/home/anon/Repos/SDL_Engine/SDL2Engine/resources";
        private const int POKEMON_MULTIPLIER = 20;
        
        private readonly IServiceAudioLoader m_audioLoader;
        private readonly IServiceAssetManager m_assetManager;
        private readonly IServiceCameraService m_cameraService;

        private Pokemon m_ash;
        private List<Pokemon> m_pokemonList = new List<Pokemon>();

        private float m_currentScaleAsh;

        public PokemonHandler(IServiceAudioLoader audioLoader,
                        IServiceAssetManager assetManager,
                        IServiceCameraService cameraService)
        {
            m_audioLoader = audioLoader;
            m_assetManager = assetManager;
            m_cameraService = cameraService;
        }
        
        public void Initialize(nint renderer)
        {
            var ashTexture = m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + "/ashh.png");
            m_ash = new Pokemon(
                name: "Ash",
                textureId: ashTexture.Id,
                originalWidth: ashTexture.Width,
                originalHeight: ashTexture.Height,
                initialPosition: new Vector2(48, 174)
            );

            var texturePaths = new[]
            {
                "/charizard.png", "/gengar.png", "/jigglypuff.png", "/moltres.png",
                "/squirtle.png",  "/ninetales.png","/poliwhirl.png"
            };

            var allTextures = new List<AssetManager.TextureData>();
            for (int r = 0; r < POKEMON_MULTIPLIER; r++) 
            {
                foreach (var path in texturePaths)
                {
                    var tex = m_assetManager.LoadTexture(renderer, RESOURCES_FOLDER + path);
                    allTextures.Add(tex);
                }
            }

            for (int i = 0; i < allTextures.Count; i++)
            {
                var texData = allTextures[i];
                int row = i / 10;
                int col = i % 10;
                var spacing = -20;

                var startPos = new Vector2(
                    330 + (col * (texData.Width + spacing)), 
                    0 + (row * (texData.Height + spacing))
                );

                var pokeObj = new Pokemon(
                    name: $"Pokemon_{i}",
                    textureId: texData.Id,
                    originalWidth: texData.Width,
                    originalHeight: texData.Height,
                    initialPosition: startPos
                );

                m_pokemonList.Add(pokeObj);
            }
        }

        public void Update(float deltaTime)
        {
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
                m_ash.Position = new Vector2(m_ash.Position.X, m_ash.Position.Y - 10f * deltaTime);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
                m_ash.Position = new Vector2(m_ash.Position.X - 10f * deltaTime, m_ash.Position.Y);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
                m_ash.Position = new Vector2(m_ash.Position.X, m_ash.Position.Y + 10f * deltaTime);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
                m_ash.Position = new Vector2(m_ash.Position.X + 10f * deltaTime, m_ash.Position.Y);

            m_ash.Update(deltaTime);

            foreach (var pokemon in m_pokemonList)
            {
                pokemon.Update(deltaTime);
            }
        }
        
        public void Render(nint renderer)
        {
            float baseScale = 0.75f;
            float amplitude = 0f;
            
            for (int i = 4; i < 16; i++)
            {
                amplitude += m_audioLoader.GetAmplitudeByName(i.ToString());
            }

            float pokemansBaseScale = 0.5f;
            float pokemansMaxScale = 5f;

            for (int i = 0; i < m_pokemonList.Count; i++)
            {
                var pokemon = m_pokemonList[i];

                // For “bass” amplitude
                float bassAmplitude = m_audioLoader.GetAmplitudeByName("0") + m_audioLoader.GetAmplitudeByName("1");

                float pulseOffset = (i * 0.5f) % MathHelper.TwoPi;
                float dynamicScaleFactor = pokemansBaseScale 
                                           + bassAmplitude 
                                           * (5f + (float)System.Math.Sin(Time.TotalTime + pulseOffset));

                Vector2 targetScale = new Vector2(dynamicScaleFactor, dynamicScaleFactor);
                pokemon.Scale = Vector2.Lerp(pokemon.Scale, targetScale, 0.1f);

                float bounceX = 10f * (float)System.Math.Sin(Time.TotalTime * 2f + i * 0.5f);
                float bounceY = 5f  * (float)System.Math.Cos(Time.TotalTime * 3f + i * 0.3f);

                Vector2 originalPos = pokemon.Position;
                pokemon.Position = new Vector2(originalPos.X + bounceX, originalPos.Y + bounceY);

                pokemon.Render(renderer, m_assetManager, m_cameraService);

                // pokemon.Position = originalPos;
            }
            
            float scaleFactor = baseScale + amplitude;
            m_currentScaleAsh = MathHelper.Lerp(m_currentScaleAsh, scaleFactor, 0.1f);
            m_currentScaleAsh = Math.Min(m_currentScaleAsh, 3f);

            float normalizedAshScale = m_currentScaleAsh; 
            m_ash.Scale = new Vector2(normalizedAshScale, normalizedAshScale);
            m_ash.Render(renderer, m_assetManager, m_cameraService);
        }

        /// <summary>
        /// Cleanup textures when done.
        /// </summary>
        public void CleanUp()
        {
            if (m_ash.TextureId != 0)
            {
                m_assetManager.UnloadTexture(m_ash.TextureId);
            }

            foreach (var poke in m_pokemonList)
            {
                m_assetManager.UnloadTexture(poke.TextureId);
            }
        }
    }
}

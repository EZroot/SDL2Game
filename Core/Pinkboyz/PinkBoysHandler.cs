using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Partitions.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Game.Core.Utils;

namespace SDL2Game.Core.Pinkboyz
{
    public class PinkBoysHandler
    {
        private const int PINKBOYS_MULTIPLIER = 1;
        
        private readonly IAudioService m_audioService;
        private readonly IImageService m_imageService;
        private readonly ICameraService m_cameraService;
        private readonly IPartitioner m_partitioner;
        
        private GameObject m_pinkboy;
        private List<GameObject> m_pinkboysList = new List<GameObject>();

        private float m_currentScalepinkboy;

        public PinkBoysHandler(IAudioService audioLoader, IImageService assetManager, ICameraService cameraService, IPartitioner partitioner)
        {
            m_audioService = audioLoader;
            m_imageService = assetManager;
            m_cameraService = cameraService;
            m_partitioner = partitioner;
        }
        
        public void Initialize(nint renderer)
        {
            var pinkboyTexture = m_imageService.LoadTexture(renderer, GameHelper.RESOURCES_FOLDER + "/pinkboy.png");
            // var pinkboySprite = new StaticSprite(pinkboyTexture.Texture, pinkboyTexture.Width, pinkboyTexture.Height);
            var pinkboySprite = new AnimatedSprite(pinkboyTexture.Texture, 32, 32, 4, 0.5f);//pinkboyTexture.Width, pinkboyTexture.Height);
            m_pinkboy = new GameObject(
                sprite: pinkboySprite,
                scale: new Vector2(32, 32),
                position: new Vector3(48, 174, 0),
                partitioner: m_partitioner
            );

            var texturePaths = new[]
            {
                "/pinkboy.png", "/pinkboy.png", "/pinkboy.png", "/pinkboy.png",
                "/pinkboy.png",  "/pinkboy.png","/pinkboy.png"
            };

            var allTextures = new List<TextureData>();
            for (int r = 0; r < PINKBOYS_MULTIPLIER; r++) 
            {
                foreach (var path in texturePaths)
                {
                    var tex = m_imageService.LoadTexture(renderer, GameHelper.RESOURCES_FOLDER + path);
                    allTextures.Add(tex);
                }
            }

            for (int i = 0; i < allTextures.Count; i++)
            {
                var texData = allTextures[i];
                
                int row = i / 10;
                int col = i % 10;
                var spacing = -20;

                var startPos = new Vector3(
                    330 + (col * (texData.Width + spacing)), 
                    0 + (row * (texData.Height + spacing)),
                    0
                );
                
                // var sprite = new StaticSprite(texData.Texture, texData.Width, texData.Height);
                var sprite = new AnimatedSprite(texData.Texture, 32, 32, 4, 0.25f);//pinkboyTexture.Width, pinkboyTexture.Height);
                var pinkObj = new GameObject(
                    sprite: sprite,
                    position: startPos,
                    scale: new Vector2(texData.Width,texData.Height),
                    partitioner: m_partitioner
                );

                m_pinkboysList.Add(pinkObj);
            }
        }

        public void Update(float deltaTime)
        {
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_w))
                m_pinkboy.Position = new Vector3(m_pinkboy.Position.X, m_pinkboy.Position.Y - 10f * deltaTime, 0f);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_a))
                m_pinkboy.Position = new Vector3(m_pinkboy.Position.X - 10f * deltaTime, m_pinkboy.Position.Y, 0f);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_s))
                m_pinkboy.Position = new Vector3(m_pinkboy.Position.X, m_pinkboy.Position.Y + 10f * deltaTime, 0f);
            if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_d))
                m_pinkboy.Position = new Vector3(m_pinkboy.Position.X + 10f * deltaTime, m_pinkboy.Position.Y, 0f);

            m_pinkboy.Update(deltaTime);

            foreach (var pinky in m_pinkboysList)
            {
                pinky.Update(deltaTime);
            }
        }
        
        public void Render(nint renderer)
        {
            float baseScale = 1f;
            float amplitude = 0f;
            
            for (int i = 4; i < 16; i++)
            {
                amplitude += m_audioService.GetAmplitudeByName(i.ToString());
            }

            float pinkymansBaseScale = 0.5f;
            float pinkymansMaxScale = 5f;

            for (int i = 0; i < m_pinkboysList.Count; i++)
            {
                var pinky = m_pinkboysList[i];

                // For “bass” amplitude
                float bassAmplitude = m_audioService.GetAmplitudeByName("0") + m_audioService.GetAmplitudeByName("1");

                float pulseOffset = (i * 0.5f) % MathHelper.TwoPi;
                float dynamicScaleFactor = pinkymansBaseScale 
                                           + bassAmplitude 
                                           * (5f + (float)System.Math.Sin(Time.TotalTime + pulseOffset));

                Vector2 targetScale = new Vector2(dynamicScaleFactor, dynamicScaleFactor);
                pinky.Scale = Vector2.Lerp(pinky.Scale, targetScale, 0.1f);

                float bounceX = 10f * (float)System.Math.Sin(Time.TotalTime * 2f + i * 0.5f);
                float bounceY = 5f  * (float)System.Math.Cos(Time.TotalTime * 3f + i * 0.3f);

                var originalPos = pinky.Position;
                pinky.Position = new Vector3(originalPos.X + bounceX, originalPos.Y + bounceY, 0);
                pinky.Rotation += bounceX * MathHelper.TwoPi * Time.DeltaTime;
                pinky.Render(renderer, m_cameraService);

                // pinky.Position = originalPos;
            }
            
            float scaleFactor = baseScale + amplitude;
            m_currentScalepinkboy = MathHelper.Lerp(m_currentScalepinkboy, scaleFactor, 0.1f);
            m_currentScalepinkboy = Math.Min(m_currentScalepinkboy, 3f);

            float normalizedpinkboyScale = m_currentScalepinkboy; 
            m_pinkboy.Scale = new Vector2(normalizedpinkboyScale, normalizedpinkboyScale);
            m_pinkboy.Render(renderer, m_cameraService);
        }

        /// <summary>
        /// Cleanup textures when done.
        /// </summary>
        public void CleanUp()
        {
            // if (m_pinkboy.TextureId != 0)
            // {
            //     m_imageService.UnloadTexture(m_pinkboy.TextureId);
            // }
            //
            // foreach (var poke in m_pinkyList)
            // {
            //     m_imageService.UnloadTexture(poke.TextureId);
            // }
        }
    }
}

using System.Numerics;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Game.GameObjects
{
    /// <summary>
    /// A specialized GameObject representing a Pokémon.
    /// </summary>
    public class Pokemon : GameObject
    {
        public string Name { get; set; }

        /// <summary>
        /// Optionally store the original spawn position for later reference.
        /// </summary>
        public Vector2 OriginalPosition { get; private set; }

        /// <summary>
        /// You could store an "audio amplitude factor" for special scaling effects.
        /// (e.g., responding to music or sound data)
        /// </summary>
        public float AudioAmplitudeFactor { get; set; } = 1.0f;

        /// <summary>
        /// Custom constructor to set up texture info, position, etc.
        /// </summary>
        /// <param name="name">Name of the Pokémon.</param>
        /// <param name="textureId">The ID of the texture in your AssetManager.</param>
        /// <param name="originalWidth">The texture's original width in pixels.</param>
        /// <param name="originalHeight">The texture's original height in pixels.</param>
        /// <param name="initialPosition">Initial spawn position in pixels.</param>
        public Pokemon(string name, int textureId, int originalWidth, int originalHeight, Vector2 initialPosition)
        {
            Name = name;
            TextureId = textureId;
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            Position = initialPosition;    // inherited from GameObject
            OriginalPosition = initialPosition;
        }

        /// <summary>
        /// Update this Pokémon's logic, animations, or other custom behavior.
        /// </summary>
        public override void Update(float deltaTime)
        {
            // First, call base to sync physics (if PhysicsBody is not null)
            base.Update(deltaTime);

            // Example: custom logic here. For instance, you could do a "bounce" or "idle" animation.
            // e.g., Position = OriginalPosition + new Vector2(0, (float)Math.Sin(Time.TotalTime * 2f) * 5f);
            
            // Or handle input if this is a controllable Pokémon (less likely):
            // if (InputManager.IsKeyPressed(SDL.SDL_Keycode.SDLK_UP)) { ... }

            // Or do amplitude-based scaling if you store an audio amplitude in AudioAmplitudeFactor.
            // Scale = new Vector2(1.0f + AudioAmplitudeFactor, 1.0f + AudioAmplitudeFactor);
        }

        /// <summary>
        /// Optionally override the render to apply special effects 
        /// before calling the base render logic.
        /// </summary>
        public override void Render
        (
            nint renderer,
            IServiceAssetManager assetManager,
            IServiceCameraService cameraService = null
        )
        {
            // Example: do some pre-render adjustments
            // e.g., apply a "pulse" scale based on AudioAmplitudeFactor:
            // float pulse = (float)Math.Sin(Time.TotalTime * 4f) * 0.2f;
            // Scale = new Vector2(1.0f + pulse * AudioAmplitudeFactor);

            // Then call the base render to actually draw
            base.Render(renderer, assetManager, cameraService);
        }
    }
}

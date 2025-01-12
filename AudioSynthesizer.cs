using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;

namespace SDL2Game;

public class AudioSynthesizer
{
    private float m_bandIntensityMod = 1.75f;
    private int m_rectSpacing = 1;
    private int m_maxRectHeight = 40;
    private int m_rectWidth = 3;

    private float m_maxAmplitude;
    private float[] m_previousHeights;
    private float m_smoothingFactor; // Smoothing factor between 0.1 and 0.3
    private int m_initialRectStartY;

    private IServiceAudioLoader m_audioLoader;
    
    public AudioSynthesizer(IServiceAudioLoader audioLoader)
    {
        m_audioLoader = audioLoader;
    }
    
    public void Initialize()
    {
        // Audio player rects
        m_bandIntensityMod = 1.75f;
        m_rectSpacing = 1;
        m_maxRectHeight = 40;
        m_rectWidth = 3;

        m_maxAmplitude = 0f;
        m_previousHeights = new float[m_audioLoader.FrequencyBands.Count];
        m_smoothingFactor = .3f; // Smoothing factor between 0.1 and 0.3
        m_initialRectStartY = 200; //(int)(m_maxRectHeight / 1.75f + m_maxRectHeight / 2);
    }

    public void Render(nint renderer)
    {
        
        #region DrawAudioSynth

        // Audio synthesizer
        var frequencyBands = m_audioLoader.FrequencyBands;
        var bandRectSize = (m_rectWidth + m_rectSpacing) * frequencyBands.Count;
        var initialRectStartX = 400;
        // initialRectStartX -= m_windowWidth - bandRectSize / 2;

        foreach (var bandPair in m_audioLoader.FrequencyBands)
        {
            var bandAmplitude = m_audioLoader.GetAmplitudeByName(bandPair.Key);
            if (bandAmplitude > m_maxAmplitude)
            {
                m_maxAmplitude = bandAmplitude;
            }
        }

        foreach (var bandPair in frequencyBands)
        {
            var amp = m_audioLoader.GetAmplitudeByName(bandPair.Key);
            var bandIndex = int.Parse(bandPair.Key);
            var bandAmplitude = amp * m_bandIntensityMod;
            var targetHeight = (int)((bandAmplitude / m_maxAmplitude) * m_maxRectHeight);
            targetHeight = Math.Clamp(targetHeight, 0, m_maxRectHeight);

            var smoothedHeight = (int)(m_previousHeights[bandIndex] * (1 - m_smoothingFactor) +
                                       targetHeight * m_smoothingFactor);
            m_previousHeights[bandIndex] = smoothedHeight;
            var currentRectX = initialRectStartX + (bandIndex * (m_rectWidth + m_rectSpacing));

            var ratio = smoothedHeight / (float)m_maxRectHeight;
            var minHue = 0.7f; //0.45f;
            var maxHue = 0.85f; //0.7f;
            var hue = (minHue + (maxHue - minHue) * ratio) * 360;
            var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_Rect backgroundRect = new SDL.SDL_Rect
            {
                x = currentRectX - m_rectSpacing / 2,
                y = m_initialRectStartY - (smoothedHeight / 2) -
                    (m_rectSpacing / 2),
                w = m_rectWidth + m_rectSpacing,
                h = smoothedHeight + m_rectSpacing
            };
            SDL.SDL_RenderFillRect(renderer, ref backgroundRect);

            SDL.SDL_Rect rect = new SDL.SDL_Rect
            {
                x = currentRectX,
                y = m_initialRectStartY - (smoothedHeight / 2),
                w = m_rectWidth,
                h = smoothedHeight
            };
            SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }

        #endregion

    }
}
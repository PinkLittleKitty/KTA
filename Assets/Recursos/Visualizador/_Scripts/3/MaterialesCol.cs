using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialesCol : MonoBehaviour {

    public Material matColor;
    public Material matParticulas;
    public Material matAltos;
    public Material matColorEmision;
    public Material matColorEmision2X;

    public Color[] palette = new Color[] {
        new Color(0.56f, 0.0f, 1.0f),
        new Color(0.0f, 0.85f, 1.0f),
        new Color(1.0f, 0.08f, 0.58f),
        new Color(1.0f, 0.45f, 0.1f),
        new Color(0.15f, 0.35f, 1.0f)
    };

    public float paletteTransitionDuration = 12.0f;

    [Range(0.5f, 3.0f)]
    public float beatSensitivity = 1.5f;

    [Range(0.0f, 1.0f)]
    public float minEmission = 0.2f;

    [Range(0.5f, 2.5f)]
    public float maxEmission = 1.1f;

    public float attackSpeed = 16.0f;

    public float decaySpeed = 4.0f;

    private int currentColorIndex = 0;
    private int nextColorIndex = 1;
    private float paletteProgress = 0.0f;
    private Color activeBaseColor;
    private float smoothedGlow = 0.2f;
    private float smoothedHighs = 0.0f;

    private readonly int baseColorProp = Shader.PropertyToID("_BaseColor");
    private readonly int emissionColorProp = Shader.PropertyToID("_EmissionColor");
    private readonly int colorProp = Shader.PropertyToID("_Color");
    private readonly int wColorProp = Shader.PropertyToID("_WColor");

    void Start()
    {
        if (palette == null || palette.Length == 0)
        {
            palette = new Color[] {
                new Color(0.56f, 0.0f, 1.0f),
                new Color(0.0f, 0.85f, 1.0f),
                new Color(1.0f, 0.08f, 0.58f)
            };
        }

        currentColorIndex = 0;
        nextColorIndex = palette.Length > 1 ? 1 : 0;
        activeBaseColor = palette[0];
        smoothedGlow = minEmission;
    }

    void Update()
    {
        UpdatePaletteDrift();
        UpdateAudioGlow();
        ApplyMaterialColors();
    }

    void UpdatePaletteDrift()
    {
        if (palette.Length < 2) return;

        float duration = Mathf.Max(paletteTransitionDuration, 1.0f);
        paletteProgress += UnityEngine.Time.deltaTime / duration;

        if (paletteProgress >= 1.0f)
        {
            paletteProgress -= 1.0f;
            currentColorIndex = nextColorIndex;
            nextColorIndex = (nextColorIndex + 1) % palette.Length;
        }

        float smoothT = 0.5f - 0.5f * Mathf.Cos(paletteProgress * Mathf.PI);
        activeBaseColor = Color.Lerp(palette[currentColorIndex], palette[nextColorIndex], smoothT);
    }

    void UpdateAudioGlow()
    {
        float rawEnergy = 0.0f;
        if (AudioSpectrum.bandBuffer != null && AudioSpectrum.bandBuffer.Length > 0)
        {
            float bassKick = Mathf.Clamp01(AudioSpectrum.bandBuffer[0] / 6.0f);
            rawEnergy = Mathf.Clamp01(Mathf.Max(AudioSpectrum.amplitudeBuffer, bassKick) * beatSensitivity);
        }
        else
        {
            rawEnergy = Mathf.Clamp01(AudioSpectrum.amplitudeBuffer * beatSensitivity);
        }

        float targetGlow = Mathf.Lerp(minEmission, maxEmission, rawEnergy);

        if (targetGlow > smoothedGlow)
        {
            smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, UnityEngine.Time.deltaTime * attackSpeed);
        }
        else
        {
            smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, UnityEngine.Time.deltaTime * decaySpeed);
        }

        if (AudioSpectrum.bandBuffer != null && AudioSpectrum.bandBuffer.Length > 6)
        {
            float targetHighs = Mathf.Clamp01(AudioSpectrum.bandBuffer[6] / 7.0f);
            smoothedHighs = Mathf.Lerp(smoothedHighs, targetHighs, UnityEngine.Time.deltaTime * 8.0f);
        }
    }

    void ApplyMaterialColors()
    {
        Color surfaceColor = activeBaseColor * 0.65f;
        surfaceColor.a = 1.0f;

        Color glowColor = activeBaseColor * smoothedGlow;

        if (matColorEmision != null)
        {
            matColorEmision.SetColor(baseColorProp, surfaceColor);
            matColorEmision.SetColor(colorProp, surfaceColor);
            matColorEmision.SetColor(emissionColorProp, glowColor);
            if (matColorEmision.HasProperty(wColorProp))
            {
                matColorEmision.SetColor(wColorProp, activeBaseColor);
            }
        }

        if (matColorEmision2X != null)
        {
            matColorEmision2X.SetColor(baseColorProp, activeBaseColor);
            matColorEmision2X.SetColor(emissionColorProp, activeBaseColor * (smoothedGlow * 1.3f));
        }

        if (matColor != null)
        {
            matColor.SetColor(baseColorProp, activeBaseColor);
            if (matColor.HasProperty(colorProp))
            {
                matColor.SetColor(colorProp, activeBaseColor);
            }
        }

        if (matParticulas != null)
        {
            matParticulas.SetColor(baseColorProp, activeBaseColor);
            matParticulas.SetColor(emissionColorProp, glowColor);
        }

        if (matAltos != null)
        {
            Color altosBase = Color.Lerp(activeBaseColor * 0.3f, Color.white, smoothedHighs * 0.8f);
            Color altosEmission = Color.Lerp(Color.black, Color.white * 0.9f, smoothedHighs);
            matAltos.SetColor(baseColorProp, altosBase);
            matAltos.SetColor(emissionColorProp, altosEmission);
        }
    }
}
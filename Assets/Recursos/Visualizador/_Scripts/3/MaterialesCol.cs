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

    public float minThickness = 0.05f;
    public float maxThickness = 0.09f;
    public float thicknessAttack = 20.0f;
    public float thicknessDecay = 6.0f;

    public bool syncStars = true;
    [Range(0.05f, 0.6f)]
    public float starBrightness = 0.25f;
    public bool starAudioPulse = true;

    public bool enableFog = true;
    public FogMode fogMode = FogMode.Linear;
    [Range(0.0f, 0.5f)]
    public float fogBrightness = 0.12f;
    public float fogStartDistance = 60.0f;
    public float fogEndDistance = 180.0f;
    public float fogDensity = 0.015f;
    public float fogLerpSpeed = 2.5f;

    private int currentColorIndex = 0;
    private int nextColorIndex = 1;
    private float paletteProgress = 0.0f;
    private Color activeBaseColor;
    private float smoothedGlow = 0.2f;
    private float smoothedHighs = 0.0f;
    private float smoothedThickness = 0.06f;
    private Color initialSkyboxColor2;
    private bool hasInitialSkyboxColor = false;

    private readonly int baseColorProp = Shader.PropertyToID("_BaseColor");
    private readonly int emissionColorProp = Shader.PropertyToID("_EmissionColor");
    private readonly int colorProp = Shader.PropertyToID("_Color");
    private readonly int wColorProp = Shader.PropertyToID("_WColor");
    private readonly int wThicknessProp = Shader.PropertyToID("_WThickness");
    private readonly int wEmissionProp = Shader.PropertyToID("_WEmission");
    private readonly int skyboxColor2Prop = Shader.PropertyToID("_Color2");

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
        smoothedThickness = minThickness;

        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Color1"))
        {
            RenderSettings.skybox.SetColor("_Color1", Color.black);
        }

        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty(skyboxColor2Prop))
        {
            initialSkyboxColor2 = RenderSettings.skybox.GetColor(skyboxColor2Prop);
            hasInitialSkyboxColor = true;
        }
        
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = fogMode;
            if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            }
            else
            {
                RenderSettings.fogDensity = fogDensity;
            }
            RenderSettings.fogColor = activeBaseColor * fogBrightness;
        }
    }

    void Update()
    {
        UpdatePaletteDrift();
        UpdateMultiBandAudio();
        ApplyMaterialColors();
        UpdateAtmosphericFog();
        UpdateSkyboxStars();
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

    void UpdateMultiBandAudio()
    {
        float bassEnergy = 0.0f;
        float highsEnergy = 0.0f;

        if (AudioSpectrum.bandBuffer != null && AudioSpectrum.bandBuffer.Length > 0)
        {
            float subBass = AudioSpectrum.bandBuffer[0] / 6.0f;
            float lowBass = AudioSpectrum.bandBuffer.Length > 1 ? (AudioSpectrum.bandBuffer[1] / 6.5f) : 0f;
            bassEnergy = Mathf.Clamp01(Mathf.Max(AudioSpectrum.amplitudeBuffer, Mathf.Max(subBass, lowBass)) * beatSensitivity);

            if (AudioSpectrum.bandBuffer.Length > 6)
            {
                float band6 = AudioSpectrum.bandBuffer[6] / 6.5f;
                float band7 = AudioSpectrum.bandBuffer.Length > 7 ? (AudioSpectrum.bandBuffer[7] / 7.0f) : 0f;
                highsEnergy = Mathf.Clamp01(Mathf.Max(band6, band7));
            }
        }
        else
        {
            bassEnergy = Mathf.Clamp01(AudioSpectrum.amplitudeBuffer * beatSensitivity);
            highsEnergy = AudioSpectrum.amplitudeBuffer;
        }

        float targetGlow = Mathf.Lerp(minEmission, maxEmission, bassEnergy);
        float glowLerpSpeed = (targetGlow > smoothedGlow) ? attackSpeed : decaySpeed;
        smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, UnityEngine.Time.deltaTime * glowLerpSpeed);

        float targetThickness = Mathf.Lerp(minThickness, maxThickness, highsEnergy);
        float thicknessLerp = (targetThickness > smoothedThickness) ? thicknessAttack : thicknessDecay;
        smoothedThickness = Mathf.Lerp(smoothedThickness, targetThickness, UnityEngine.Time.deltaTime * thicknessLerp);

        smoothedHighs = Mathf.Lerp(smoothedHighs, highsEnergy, UnityEngine.Time.deltaTime * 8.0f);
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
            if (matColorEmision.HasProperty(wThicknessProp))
            {
                matColorEmision.SetFloat(wThicknessProp, smoothedThickness);
            }
            if (matColorEmision.HasProperty(wEmissionProp))
            {
                matColorEmision.SetFloat(wEmissionProp, smoothedGlow * 3.0f);
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

    void UpdateAtmosphericFog()
    {
        if (!enableFog)
        {
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;

        if (fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
        else
        {
            RenderSettings.fogDensity = fogDensity;
        }

        Color targetFog = activeBaseColor * fogBrightness;
        targetFog.a = 1.0f;
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFog, UnityEngine.Time.deltaTime * fogLerpSpeed);
    }

    void UpdateSkyboxStars()
    {
        if (!syncStars || RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty(skyboxColor2Prop))
            return;

        float pulse = starAudioPulse ? (smoothedHighs * 0.12f) : 0.0f;
        Color starColor = activeBaseColor * (starBrightness + pulse);
        starColor.a = 0.0f;

        Color currentColor = RenderSettings.skybox.GetColor(skyboxColor2Prop);
        RenderSettings.skybox.SetColor(skyboxColor2Prop, Color.Lerp(currentColor, starColor, UnityEngine.Time.deltaTime * 3.0f));
    }

    void OnDestroy()
    {
        if (hasInitialSkyboxColor && RenderSettings.skybox != null && RenderSettings.skybox.HasProperty(skyboxColor2Prop))
        {
            RenderSettings.skybox.SetColor(skyboxColor2Prop, initialSkyboxColor2);
        }
    }
}
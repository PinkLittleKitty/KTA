using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {

    public Material Terrain;

    public Color[] PossibleColors = new Color[] {
        new Color(0.56f, 0.0f, 1.0f),
        new Color(0.0f, 0.85f, 1.0f),
        new Color(1.0f, 0.08f, 0.58f),
        new Color(1.0f, 0.45f, 0.1f)
    };

    public Color CurrentColor;

    public float TiempoCambio = 16.0f; 

    public float MaxThickness = 0.09f, MinThickness = 0.05f;

    public bool syncWithAudio = true;
    public float minEmission = 0.2f;
    public float maxEmission = 1.0f;

    public bool syncStars = true;
    [Range(0.05f, 0.6f)]
    public float starBrightness = 0.25f;
    public bool starAudioPulse = true;

    [Header("Atmospheric Fog")]
    public bool enableFog = true;
    public FogMode fogMode = FogMode.Linear;
    [Range(0.0f, 0.5f)]
    public float fogBrightness = 0.12f;
    public float fogStartDistance = 60.0f;
    public float fogEndDistance = 180.0f;
    public float fogDensity = 0.015f;
    public float fogLerpSpeed = 2.5f;

    public float Tiempo;

    private float Grosor;
    private int currentIndex = 0;
    private int nextIndex = 1;
    private float transitionProgress = 0.0f;
    private float smoothedGlow = 0.2f;
    private float smoothedThickness = 0.06f;
    private Color initialSkyboxColor2;
    private bool hasInitialSkyboxColor = false;

    private readonly int wThicknessProp = Shader.PropertyToID("_WThickness");
    private readonly int wEmissionProp = Shader.PropertyToID("_WEmission");
    private readonly int skyboxColor2Prop = Shader.PropertyToID("_Color2");

    void Start() {
        Grosor = MinThickness;
        smoothedThickness = MinThickness;

        if (PossibleColors != null && PossibleColors.Length > 0)
        {
            CurrentColor = PossibleColors[0];
            nextIndex = PossibleColors.Length > 1 ? 1 : 0;
        }

        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Color1")) {
            RenderSettings.skybox.SetColor("_Color1", Color.black);
        }

        if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty(skyboxColor2Prop)) {
            initialSkyboxColor2 = RenderSettings.skybox.GetColor(skyboxColor2Prop);
            hasInitialSkyboxColor = true;
        }

        if (enableFog) {
            RenderSettings.fog = true;
            RenderSettings.fogMode = fogMode;
            if (fogMode == FogMode.Linear) {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            } else {
                RenderSettings.fogDensity = fogDensity;
            }
            RenderSettings.fogColor = CurrentColor * fogBrightness;
        }
    }

    void Update() {
        this.Tiempo += UnityEngine.Time.deltaTime;

        UpdateColorTransition();
        UpdateMultiBandAudio();
        ApplyToTerrain();
        UpdateAtmosphericFog();
        UpdateSkyboxStars();
    }

    void UpdateColorTransition() {
        if (PossibleColors == null || PossibleColors.Length < 2) return;

        float duration = Mathf.Max(TiempoCambio, 1.0f);
        transitionProgress += UnityEngine.Time.deltaTime / duration;

        if (transitionProgress >= 1.0f) {
            transitionProgress -= 1.0f;
            currentIndex = nextIndex;
            nextIndex = (nextIndex + 1) % PossibleColors.Length;
        }

        float smoothT = 0.5f - 0.5f * Mathf.Cos(transitionProgress * Mathf.PI);
        CurrentColor = Color.Lerp(PossibleColors[currentIndex], PossibleColors[nextIndex], smoothT);
    }

    void UpdateMultiBandAudio() {
        if (!syncWithAudio) {
            smoothedGlow = minEmission;
            smoothedThickness = MinThickness;
            return;
        }

        float bassEnergy = 0.0f;
        float highsEnergy = 0.0f;

        if (AudioSpectrum.bandBuffer != null && AudioSpectrum.bandBuffer.Length > 0) {
            float subBass = AudioSpectrum.bandBuffer[0] / 6.0f;
            float lowBass = AudioSpectrum.bandBuffer.Length > 1 ? (AudioSpectrum.bandBuffer[1] / 6.5f) : 0f;
            bassEnergy = Mathf.Clamp01(Mathf.Max(AudioSpectrum.amplitudeBuffer, Mathf.Max(subBass, lowBass)));

            if (AudioSpectrum.bandBuffer.Length > 6) {
                float band6 = AudioSpectrum.bandBuffer[6] / 6.5f;
                float band7 = AudioSpectrum.bandBuffer.Length > 7 ? (AudioSpectrum.bandBuffer[7] / 7.0f) : 0f;
                highsEnergy = Mathf.Clamp01(Mathf.Max(band6, band7));
            }
        } else {
            bassEnergy = AudioSpectrum.amplitudeBuffer;
            highsEnergy = AudioSpectrum.amplitudeBuffer;
        }

        float targetGlow = Mathf.Lerp(minEmission, maxEmission, bassEnergy);
        float glowSpeed = (targetGlow > smoothedGlow) ? 14.0f : 4.0f;
        smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, UnityEngine.Time.deltaTime * glowSpeed);

        float targetThickness = Mathf.Lerp(MinThickness, MaxThickness, highsEnergy);
        float thickSpeed = (targetThickness > smoothedThickness) ? 18.0f : 6.0f;
        smoothedThickness = Mathf.Lerp(smoothedThickness, targetThickness, UnityEngine.Time.deltaTime * thickSpeed);
    }

    void ApplyToTerrain() {
        if (Terrain == null) return;

        Color baseShade = CurrentColor * 0.65f;
        baseShade.a = 1.0f;
        Color glow = CurrentColor * smoothedGlow;

        Terrain.SetColor("_Color", baseShade);
        Terrain.SetColor("_BaseColor", baseShade);

        Terrain.SetColor("_GColor", CurrentColor);
        Terrain.SetColor("_WColor", CurrentColor);
        Terrain.SetColor("_EmissionColor", glow);

        if (Terrain.HasProperty(wThicknessProp)) {
            Terrain.SetFloat(wThicknessProp, smoothedThickness);
        }
        if (Terrain.HasProperty(wEmissionProp)) {
            Terrain.SetFloat(wEmissionProp, smoothedGlow * 3.0f);
        }

        Terrain.SetFloat("_Grosor", smoothedThickness);
    }

    void UpdateAtmosphericFog() {
        if (!enableFog) {
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;

        if (fogMode == FogMode.Linear) {
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        } else {
            RenderSettings.fogDensity = fogDensity;
        }

        Color targetFog = CurrentColor * fogBrightness;
        targetFog.a = 1.0f;
        RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFog, UnityEngine.Time.deltaTime * fogLerpSpeed);
    }

    void UpdateSkyboxStars() {
        if (!syncStars || RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty(skyboxColor2Prop))
            return;

        Color starColor = CurrentColor * starBrightness;
        starColor.a = 0.0f;

        Color currentColor = RenderSettings.skybox.GetColor(skyboxColor2Prop);
        RenderSettings.skybox.SetColor(skyboxColor2Prop, Color.Lerp(currentColor, starColor, UnityEngine.Time.deltaTime * 3.0f));
    }

    void OnDestroy() {
        if (hasInitialSkyboxColor && RenderSettings.skybox != null && RenderSettings.skybox.HasProperty(skyboxColor2Prop)) {
            RenderSettings.skybox.SetColor(skyboxColor2Prop, initialSkyboxColor2);
        }
    }
}

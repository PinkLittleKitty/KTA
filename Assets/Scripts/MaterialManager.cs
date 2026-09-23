using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {

    public Material Terrain;

    public Color[] PossibleColors = new Color[] {
        new Color(0.56f, 0.0f, 1.0f),    // Purple
        new Color(0.0f, 0.85f, 1.0f),    // Cyan
        new Color(1.0f, 0.08f, 0.58f),   // Magenta
        new Color(1.0f, 0.45f, 0.1f)     // Amber
    };

    public Color CurrentColor;

    public float TiempoCambio = 16.0f; 

    public float MaxThickness = 0.3f, MinThickness = 0.15f;

    public bool syncWithAudio = true;
    public float minEmission = 0.2f;
    public float maxEmission = 1.0f;

    public float Tiempo;

    private float Grosor;

    private int currentIndex = 0;
    private int nextIndex = 1;
    private float transitionProgress = 0.0f;
    private float smoothedGlow = 0.2f;

    void Start() {
        Grosor = MaxThickness;

        if (PossibleColors != null && PossibleColors.Length > 0)
        {
            CurrentColor = PossibleColors[0];
            nextIndex = PossibleColors.Length > 1 ? 1 : 0;
        }
    }

    void Update() {
        this.Tiempo += UnityEngine.Time.deltaTime;

        UpdateColorTransition();
        UpdateAudioGlow();
        ApplyToTerrain();
        UpdateThickness();
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

    void UpdateAudioGlow() {
        if (!syncWithAudio) {
            smoothedGlow = minEmission;
            return;
        }

        float targetGlow = minEmission;
        if (AudioSpectrum.bandBuffer != null && AudioSpectrum.bandBuffer.Length > 0) {
            float bass = Mathf.Clamp01(AudioSpectrum.bandBuffer[0] / 6.0f);
            float energy = Mathf.Max(AudioSpectrum.amplitudeBuffer, bass);
            targetGlow = Mathf.Lerp(minEmission, maxEmission, energy);
        } else {
            targetGlow = Mathf.Lerp(minEmission, maxEmission, AudioSpectrum.amplitudeBuffer);
        }

        float lerpSpeed = (targetGlow > smoothedGlow) ? 14.0f : 4.0f;
        smoothedGlow = Mathf.Lerp(smoothedGlow, targetGlow, UnityEngine.Time.deltaTime * lerpSpeed);
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
    }

    void UpdateThickness() {
        if (Terrain == null) return;

        float t = Mathf.PingPong(Tiempo / Mathf.Max(TiempoCambio, 1.0f), 1.0f);
        Grosor = Mathf.Lerp(MinThickness, MaxThickness, t);
        Terrain.SetFloat("_Grosor", Grosor);
    }
}

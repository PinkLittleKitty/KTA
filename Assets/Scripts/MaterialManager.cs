using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour {

    // Referencia al material del terreno
    public Material Terrain;

    // Arreglo de colores posibles
    public Color[] PossibleColors;

    // Color actual
    public Color CurrentColor;

    // Grosor máximo y mínimo
    public float MaxThickness = 0.3f, MinThickness = 0.15f;

    // Tiempo actual
    public float Tiempo;

    // Tiempo para cambiar de color
    public float TiempoCambio = 32; 

    // Grosor actual
    private float Grosor;

    void Start() {
        // Inicializa el grosor al valor máximo
        Grosor = MaxThickness;
    }

    void Update(){

        // Actualiza el tiempo con el tiempo delta de Unity
        this.Tiempo += UnityEngine.Time.deltaTime;

        // Aplica una transición suave de color al material del terreno
        Terrain.SetColor("_Color", Lerp(Terrain.GetColor("_Color"), CurrentColor, UnityEngine.Time.deltaTime * 2f));

        // Actualiza otros colores relacionados con el terreno
        Terrain.SetColor("_GColor", Terrain.GetColor("_Color"));
        Terrain.SetColor("_WColor", Terrain.GetColor("_Color"));
        Terrain.SetColor("_EmissionColor", Terrain.GetColor("_Color"));

        // Actualiza el grosor del material
        UpdateThickness();
    }

    // Función para realizar una interpolación lineal entre dos colores
    Color Lerp(Color c1, Color c2, float delta){
        return new Color (Mathf.Lerp (c1.r, c2.r, delta), Mathf.Lerp (c1.g, c2.g, delta), Mathf.Lerp (c1.b, c2.b, delta), Mathf.Lerp (c1.a, c2.a, delta));
    }

    // Función para actualizar el grosor del material de terreno
    void UpdateThickness() {
        // Calcula el grosor en función del tiempo actual
        Grosor = Mathf.Lerp(MinThickness, MaxThickness, Tiempo / TiempoCambio);

        // Aplica el grosor al material
        Terrain.SetFloat("_Grosor", Grosor);
    }
}

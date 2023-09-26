using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiskPoints : MonoBehaviour
{
    // Referencia al colisionador asociado a este objeto
    public Collider Colision;

    // Cantidad de puntos de riesgo
    public int RiskPointsNumber;

    // Referencia al objeto "World"
    public GameObject World;

    void Start()
    {
        // Busca el objeto con la etiqueta "World" y lo asigna a la variable "World"
        World = GameObject.FindGameObjectWithTag("World");
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray1 = new Ray();
        ray1.origin = transform.position;
        ray1.direction = Vector3.left;

        // Realiza un raycast hacia la izquierda desde la posición de este objeto
        if (Physics.Raycast(ray1, out hit))
        {
            Debug.DrawLine(ray1.origin, hit.point);
        }
    }

    // Se ejecuta cuando este objeto sale de un colisionador
    void OnTriggerExit(Collider other)
    {
        // Incrementa la puntuación máxima del jugador utilizando PlayerPrefs
        PlayerPrefs.SetInt("HighScore", PlayerPrefs.GetInt("HighScore") + RiskPointsNumber);
    }
}

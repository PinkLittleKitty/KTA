using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CargarEscenaInicial : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("Launch") == 0)
        {
            //Primer Lanzamiento
            PlayerPrefs.SetInt("Launch", 1);
            PlayerPrefs.SetFloat("Chunks", 9);
            SceneManager.LoadScene("Seleccion");
        }
        else
        {
            //Cargar el juego si no es el primer lanzamiento
            SceneManager.LoadScene("Game");
        }
    }

}
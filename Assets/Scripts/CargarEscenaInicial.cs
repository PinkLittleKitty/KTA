using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CargarEscenaInicial : MonoBehaviour
{
    //public GoogleIntegration googleIntegration; AL DESCOMENTAR ESTO TAMBIÉN AÑADÍ EL OBJETO EN ESCENA O NO VA A FUNCAR

    async void Start()
    {
        //await googleIntegration.Authenticate();
        
        if (PlayerPrefs.GetInt("Launch") == 0)
        {
            //Primer Lanzamiento
            PlayerPrefs.SetInt("Launch", 1);
            PlayerPrefs.SetFloat("Chunks", 9);
            SceneManager.LoadScene("Selección");
        }
        else
        {
            //Cargar el juego si no es el primer lanzamiento
            SceneManager.LoadScene("Game");
        }
    }

}
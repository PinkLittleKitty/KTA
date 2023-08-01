using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CargarEscenaInicial : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        if (PlayerPrefs.GetInt("BananaLaunch") == 0)
        {
            //First launch
            PlayerPrefs.SetInt("BananaLaunch", 1);
            PlayerPrefs.SetFloat("Chunks", 9);
            SceneManager.LoadScene("Selección");
        }
        else
        {
            //Load scene_02 if its not the first launch
            SceneManager.LoadScene("Game");
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Opciones : MonoBehaviour
{

    public Canvas CanvasOpciones;
    public Canvas CanvasInfo;

    public void AbrirOpciones()
    {
        if (CanvasOpciones.gameObject.activeInHierarchy)
        {
            CanvasOpciones.gameObject.SetActive(false);
            CanvasInfo.gameObject.SetActive(false);
        }
        else
        {
            CanvasOpciones.gameObject.SetActive(true);
            CanvasInfo.gameObject.SetActive(false);
        }
    }

    public void AbrirInfo()
    {
        if (CanvasInfo.gameObject.activeInHierarchy)
        {
            CanvasInfo.gameObject.SetActive(false);
        }
        else
        {
            CanvasInfo.gameObject.SetActive(true);
        }
    }

    public void ControlIzquierdo()
    {
        PlayerPrefs.SetInt("Control", 1);
        CanvasOpciones.gameObject.SetActive(false);
        PlayerPrefs.Save();
    }

    public void ControlDerecho()
    {
        PlayerPrefs.SetInt("Control", 2);
        CanvasOpciones.gameObject.SetActive(false);
        PlayerPrefs.Save();
    }

    public void ControlGyro()
    {
        PlayerPrefs.SetInt("Control", 3);
        CanvasOpciones.gameObject.SetActive(false);
        PlayerPrefs.Save();
    }

    public void ReiniciarHighScore()
    {
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.Save();
    }

    public void ControlIzqPrimera()
    {
        PlayerPrefs.SetInt("Control", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("0");

    }

    public void ControlDerPrimera()
    {
        PlayerPrefs.SetInt("Control", 2);
        PlayerPrefs.Save();
        SceneManager.LoadScene("0");
    }

    public void ControlGyroPrimera()
    {
        PlayerPrefs.SetInt("Control", 3);
        PlayerPrefs.Save();
        SceneManager.LoadScene("0");
    }
}

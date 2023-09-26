using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyPressTimer : MonoBehaviour
{
    public float timer; // El temporizador actual
    public float keyPressDuration; // La duración necesaria para activar la función
    public bool isKeyPressActive; // Indica si se ha activado la tecla
    public Image keyIndicator; // La imagen que indica la tecla
    public GameObject canvas; // La referencia al objeto Canvas
    public GameObject player; // La referencia al objeto jugador

    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Presionar inicialmente la tecla
        if (Input.touchCount <= 1 && !isKeyPressActive)
        {
            // Obtener la marca de tiempo
            isKeyPressActive = true;
        }

        // Mantener presionada la tecla
        if (Input.touchCount <= 1 && isKeyPressActive)
        {
            // Verificar si ha transcurrido más tiempo del necesario y si hay energía suficiente
            if (timer > keyPressDuration && canvas.GetComponent<TimeControl>().energyLeft > 0)
            {
                keyIndicator.gameObject.SetActive(true);
            }
        }

        // Detectar si se presionan dos dedos y se había activado la tecla
        if (Input.touchCount >= 2 && isKeyPressActive)
        {
            keyIndicator.gameObject.SetActive(false);
            timer = 0;
            isKeyPressActive = false;
            Destroy(this);
        }
    }
}

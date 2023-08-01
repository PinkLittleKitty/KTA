using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimerTuto : MonoBehaviour
{
    public float keyTimer;
    public float keyLength; //The duration needed to trigger the function
    public bool isKeyActive;
    public Image taptap;
    public GameObject CanvasTc;
    public GameObject timerTutowo;

void Start()
    {
       CanvasTc = GameObject.FindGameObjectWithTag("Canvas");
       timerTutowo = GameObject.FindGameObjectWithTag("Player");
    }

 void Update()
    {

            keyTimer += Time.deltaTime;

        //Initial key press
        if (Input.touchCount <= 1 && !isKeyActive)
        {

            //Get the timestamp

            isKeyActive = true;

        }

        //Key currently being held down
        if (Input.touchCount <= 1 && isKeyActive)
        {

            //Check is time elapsed is greater than keyLength
            if (keyTimer > keyLength && CanvasTc.GetComponent<TimeControl>().EnergyLeft > 0)
            {   
                taptap.gameObject.SetActive(true);
            }

        }

        if (Input.touchCount >= 2 && isKeyActive)
        {
            taptap.gameObject.SetActive(false);
            keyTimer = 0;
            isKeyActive = false;
            Destroy(this);

        }

    }
}

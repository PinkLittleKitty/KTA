using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiskPoints : MonoBehaviour
{
    public Collider Colision;
    public int RiskPointsNumber;
    public GameObject World;
    

	void Start()
        {
            World = GameObject.FindGameObjectWithTag ("World");
	    }

    void Update()
    {
        RaycastHit hit;
        Ray ray1 = new Ray();
        ray1.origin = transform.position;
        ray1.direction = Vector3.left;

         if (Physics.Raycast(ray1, out hit))
         {
            Debug.DrawLine(ray1.origin, hit.point);
            //target = hit.collider.transform.parent.gameObject;
         }
        //void OnTriggerEnter(Collider other)
        //{
        //    if(other.transform.parent.gameObject.CompareTag("World"))
        //    {
        //        RiskPointsNumber = 3;
        //    }
        //}

        void OnTriggerExit(Collider other)
        {
            PlayerPrefs.SetInt("HighScore", (int) PlayerPrefs.GetInt("HighScore") + RiskPointsNumber);
        }         
    }
}

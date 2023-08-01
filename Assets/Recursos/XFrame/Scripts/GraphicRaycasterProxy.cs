using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace XFrameEffect
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class GraphicRaycasterProxy : MonoBehaviour
    {

        [HideInInspector]
        GraphicRaycaster
                        gr;
        Canvas canvas;
        GameObject lastHitGO;
        List<RaycastResult> hitResults;

        void OnEnable()
        {
            canvas = this.GetComponent<Canvas>();
        }

        void OnDestroy()
        {
            GraphicRaycaster gr = this.GetComponent<GraphicRaycaster>();
            if (gr != null)
                gr.enabled = true;
        }

        public void Update()
        {
            {
                if (canvas == null || (canvas.renderMode != RenderMode.ScreenSpaceCamera && canvas.renderMode != RenderMode.WorldSpace))
                {
                    DestroyImmediate(this);
                    return;
                }

                if (gr == null)
                {
                    gr = this.GetComponent<GraphicRaycaster>();
                    if (gr == null)
                        return;
                }
                gr.enabled = false;

                bool pressed = Input.GetMouseButtonDown(0);
                bool released = Input.GetMouseButtonUp(0);

                if (pressed || released)
                {
                    PointerEventData ped = new PointerEventData(EventSystem.current);
                    float downsampling = XFrameManager.instance.activeDownsampling;
                    ped.position = new Vector3(Input.mousePosition.x * downsampling, Input.mousePosition.y * downsampling);
                    if (hitResults == null)
                    {
                        hitResults = new List<RaycastResult>();
                    }
                    else
                    {
                        hitResults.Clear();
                    }
                    gr.Raycast(ped, hitResults);
                    int count = hitResults.Count;
                    if (count > 0)
                    {
                        ped.Use();
                        if (pressed)
                        {
                            lastHitGO = hitResults[0].gameObject;
                        }
                        else if (released)
                        {
                            if (hitResults[0].gameObject == lastHitGO)
                            {
                                hitResults[0].gameObject.BroadcastMessage("OnPointerClick", ped, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        for (int k = 0; k < count; k++)
                        {
                            if (pressed)
                            {
                                hitResults[k].gameObject.BroadcastMessage("OnPointerDown", ped, SendMessageOptions.DontRequireReceiver);
                            }
                            else if (released)
                            {
                                hitResults[k].gameObject.BroadcastMessage("OnPointerUp", ped, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                }
            }
        }
    }
}
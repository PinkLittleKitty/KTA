using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFrameEffect
{

    [ExecuteInEditMode]
    public class XFrameFPSInfo : MonoBehaviour
    {

        static GUIStyle boxStyle;

        void OnGUI()
        {
            Rect rect;
            XFrameManager xframe = XFrameManager.instance;
            if (xframe == null)
                return;
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle();
                boxStyle.normal.textColor = Color.yellow;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.wordWrap = false;
                boxStyle.richText = false;
            }
            switch (xframe.fpsLocation)
            {
                default:
                    rect = new Rect(5, 5, 50, 30);
                    boxStyle.alignment = TextAnchor.MiddleLeft;
                    break;
                case XFRAME_FPS_LOCATION.TopRightCorner:
                    rect = new Rect(Screen.width - 55, 5, 50, 30);
                    boxStyle.alignment = TextAnchor.MiddleRight;
                    break;
                case XFRAME_FPS_LOCATION.BottomLeftCorner:
                    rect = new Rect(5, Screen.height - 35, 50, 30);
                    boxStyle.alignment = TextAnchor.MiddleLeft;
                    break;
                case XFRAME_FPS_LOCATION.BottomRightCorner:
                    rect = new Rect(Screen.width - 55, Screen.height - 35, 50, 30);
                    boxStyle.alignment = TextAnchor.MiddleRight;
                    break;
            }

            float scale = 1080 / Screen.height;
            boxStyle.fontSize = (int)(xframe.fpsFontSize * scale);
            GUI.Box(rect, xframe.currentFPS.ToString(), boxStyle);

        }
    }
}
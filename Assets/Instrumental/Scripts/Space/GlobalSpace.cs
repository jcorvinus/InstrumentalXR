using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.GraphicalRenderer;

namespace Instrumental.Space
{
    public class GlobalSpace : MonoBehaviour
    {
        LeapGraphicRenderer graphicRenderer;
        UICommonElements uiCommon;

        public LeapGraphicRenderer GraphicRenderer { get { return graphicRenderer; } }
        public UICommonElements UICommon { get { return uiCommon; } }

        private static GlobalSpace instance;
        public static GlobalSpace Instance { get { return instance; } }

        private void Awake()
        {
            graphicRenderer = GetComponent<LeapGraphicRenderer>();
            uiCommon = Resources.Load<UICommonElements>("UICommon");

            if(uiCommon == null)
            {
                Debug.LogError("could not load UI common");
            }

            instance = this;
        }
    }
}
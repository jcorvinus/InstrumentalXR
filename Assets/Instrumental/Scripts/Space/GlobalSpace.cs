using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Space
{
    public class GlobalSpace : MonoBehaviour
    {
        UICommonElements uiCommon;

        public UICommonElements UICommon { get { return uiCommon; } }

        private static GlobalSpace instance;
        public static GlobalSpace Instance { get { return instance; } }

        private void Awake()
        {
            uiCommon = Resources.Load<UICommonElements>("UICommon");

            if(uiCommon == null)
            {
                Debug.LogError("could not load UI common");
            }

            instance = this;
        }
    }
}
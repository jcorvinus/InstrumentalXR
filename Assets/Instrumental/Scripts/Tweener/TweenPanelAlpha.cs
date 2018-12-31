using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.GraphicalRenderer;

namespace Instrumental.Tweening
{
    public class TweenPanelAlpha : Tweener
    {
        LeapPanelGraphic panelGraphic;

        public Color StartColor;
        public Color GoalColor;

        private void Awake()
        {
            panelGraphic = GetComponent<LeapPanelGraphic>();
        }

        // Use this for initialization
        void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
            base.Update();
            if (TweenerState == TweenState.Play) panelGraphic.SetRuntimeTint(Color.Lerp(StartColor, GoalColor, TValue));
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Tweening
{
    public class TweenPanelAlpha : Tweener
    {
        public Color StartColor;
        public Color GoalColor;

        private void Awake()
        {

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
            //if (TweenerState == TweenState.Play) panelGraphic.SetRuntimeTint(Color.Lerp(StartColor, GoalColor, TValue));
        }
    }
}
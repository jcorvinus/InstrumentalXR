using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing.Tools
{
    public class UniformColorZone : ColorZone
    {
        [SerializeField] Color color;
        public Color Color { get { return color; } set { color = value; } }

        public override Color GetColorAtPoint(Vector3 point)
        {
            return color;
        }

        private void Start()
        {
            OnColorSet += UniformColorZone_OnColorSet;
        }

        private void UniformColorZone_OnColorSet(ColorZone zone, Color color, Vector3 point)
        {
            this.color = color;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Modeling;

namespace Instrumental.Editing.Tools
{
    public class GradientColorZone : ColorZone
    {
        public delegate void GradientUpdateHandler(GradientColorZone sender);
        public event GradientUpdateHandler OnGradientChanged;

        public GradientInfo GradientInfo;
        public Gradient Gradient;
        BoxCollider gradientZoneCollider;

        private void Awake()
        {
            gradientZoneCollider = GetComponent<BoxCollider>();

            OnColorSet += GradientColorZone_OnColorSet;
        }

        private void GradientColorZone_OnColorSet(ColorZone zone, Color color, Vector3 point)
        {
            // figure out which side of the gradient to change
            // for simplicity's sake, we only allow 2 color gradients at the moment

            Vector3 localPoint = transform.InverseTransformPoint(point);
            float gradientValue = 0;
            if (GradientInfo.Type == GradientType.Horizontal)
            {
                gradientValue = Mathf.InverseLerp(-gradientZoneCollider.size.x * 0.5f, gradientZoneCollider.size.x * 0.5f,
                    localPoint.x);
            }
            else if (GradientInfo.Type == GradientType.Vertical)
            {
                gradientValue = Mathf.InverseLerp(-gradientZoneCollider.size.y * 0.5f, gradientZoneCollider.size.y * 0.5f,
                    localPoint.y);
            }
            else if (GradientInfo.Type == GradientType.Radial)
            {
                gradientValue = ((Vector2)localPoint).magnitude;
            }

            if (GradientInfo.Invert) gradientValue = 1 - gradientValue;

            if(gradientValue > 0.5)
            {
                GradientColorKey[] newKeys = new GradientColorKey[2];
                newKeys[0] = Gradient.colorKeys[0];
                newKeys[1] = new GradientColorKey(color, 1);

                Gradient.colorKeys = newKeys;
            }
            else
            {
                Gradient.colorKeys[0] = new GradientColorKey(color, 0);

                GradientColorKey[] newKeys = new GradientColorKey[2];
                newKeys[0] = new GradientColorKey(color, 0);
                newKeys[1] = Gradient.colorKeys[1];

                Gradient.colorKeys = newKeys;
            }

            if(OnGradientChanged != null)
            {
                OnGradientChanged(this);
            }
        }

        public override Color GetColorAtPoint(Vector3 point)
        {
            Vector3 localPoint = transform.InverseTransformPoint(point);
            float gradientValue = 0;
            if (GradientInfo.Type == GradientType.Horizontal)
            {
                gradientValue = Mathf.InverseLerp(-gradientZoneCollider.size.x * 0.5f, gradientZoneCollider.size.x * 0.5f,
                    localPoint.x);
            }
            else if (GradientInfo.Type == GradientType.Vertical)
            {
                gradientValue = Mathf.InverseLerp(-gradientZoneCollider.size.y * 0.5f, gradientZoneCollider.size.y * 0.5f,
                    localPoint.y);
            }
            else if (GradientInfo.Type == GradientType.Radial)
            {
                gradientValue = ((Vector2)localPoint).magnitude;
            }

            if (GradientInfo.Invert) gradientValue = 1 - gradientValue;
            return Gradient.Evaluate(gradientValue);
        }
    }
}
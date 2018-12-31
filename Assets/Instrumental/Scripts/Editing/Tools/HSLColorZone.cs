using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing.Tools
{
    public class HSLColorZone : ColorZone
    {
        [SerializeField]
        BoxCollider boxCollider;

        public override Color GetColorAtPoint(Vector3 point)
        {
            Vector3 localPoint = transform.InverseTransformPoint(point);
            localPoint += boxCollider.size * 0.5f;

            //Bounds shiftedBounds = new Bounds(boxCollider.size * 0.5f, boxCollider.size); // move everything into the positive quadrant

            float x = Mathf.InverseLerp(0, boxCollider.size.x, localPoint.x);
            float y = Mathf.InverseLerp(0, boxCollider.size.y, localPoint.y);

            float h = x;
            float s = 1 - Mathf.InverseLerp(0.5f, 1, y);
            float l = 1 - Mathf.InverseLerp(0, 0.5f, y);

            return Color.HSVToRGB(h, s, l);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing.Tools
{
    public class ColorZoneRelay : MonoBehaviour
    {
        ColorZone colorZone;
        public ColorZone Zone { get { return colorZone; } }

        private void Awake()
        {
            colorZone = GetComponentInParent<ColorZone>();
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    ColorDropper dropperCandidate = other.GetComponentInParent<ColorDropper>();

        //    if(dropperCandidate != null)
        //    {
        //        colorZone.RegisterDropper(dropperCandidate);
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    ColorDropper dropperCandidate = other.GetComponentInParent<ColorDropper>();

        //    if(dropperCandidate != null)
        //    {
        //        colorZone.UnregisterDropper(dropperCandidate);
        //    }
        //}
    }
}
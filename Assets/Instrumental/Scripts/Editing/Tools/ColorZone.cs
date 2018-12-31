using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing.Tools
{
    public abstract class ColorZone : MonoBehaviour
    {
        public delegate void ColorHandler(ColorZone zone, Color color, Vector3 point);
        public event ColorHandler OnColorSet;

        [SerializeField] protected bool canDrop;
        public bool CanDrop { get { return canDrop; } }

        int dropperColliderRefCount = 0;
        ColorDropper dropper;

        public abstract Color GetColorAtPoint(Vector3 point);
        public void SetColorAtPoint(Color color, Vector3 point)
        {
            if(OnColorSet != null)
            {
                ColorHandler dispatch = OnColorSet;

                dispatch(this, color, point);
            }
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    ColorDropper dropperCandidate = other.GetComponentInParent<ColorDropper>();

        //    Debug.Log(name + " collider " + other.name + " entered.");

        //    if (dropperCandidate == null) return;

        //    if(dropper == null || dropperCandidate == dropper)
        //    {
        //        dropper = dropperCandidate;

        //        IncrementDropper();
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    ColorDropper dropperCandidate = other.GetComponentInParent<ColorDropper>();

        //    Debug.Log(name + " collider " + other.name + " exited.");

        //    if (dropperCandidate == null) return;

        //    if (dropperCandidate == dropper)
        //    {
        //        DecrementDropper();
        //    }
        //}

        //private void IncrementDropper()
        //{
        //    if (dropperColliderRefCount == 0)
        //    {
        //        dropper.SetColorZone(this);
        //    }

        //    dropperColliderRefCount++;
        //}

        //private void DecrementDropper()
        //{
        //    dropperColliderRefCount--;

        //    if (dropperColliderRefCount == 0)
        //    {
        //        dropper.ClearColorZone();
        //        dropper = null;
        //    }
        //}

        ///// <summary>
        ///// Use Register and Unregister dropper when
        ///// you have a multi-collider scenario that prevents proper
        ///// unity messaging
        ///// </summary>
        ///// <param name="dropper"></param>
        //public void RegisterDropper(ColorDropper dropperCandidate)
        //{
        //    if (dropperCandidate == null || !enabled) return;

        //    if (dropper == null || dropperCandidate == dropper)
        //    {
        //        dropper = dropperCandidate;

        //        IncrementDropper();
        //    }
        //}

        //public void UnregisterDropper(ColorDropper dropperCandidate)
        //{
        //    if (!enabled) return;

        //    if (dropperCandidate == dropper)
        //    {
        //        DecrementDropper();
        //    }
        //}
    }
}
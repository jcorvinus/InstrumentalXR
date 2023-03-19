using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class FingerPinchVisualization : PinchVisualization
    {
        [SerializeField] Finger pinchFinger = Finger.Index;

		protected override PinchInfo GetPinchInfo()
		{
			return hand.GetPinchInfo(pinchFinger);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class GrabPinchVisualization : PinchVisualization
    {
        protected override PinchInfo GetPinchInfo()
		{
            return hand.GraspPinch;
        }
    }
}
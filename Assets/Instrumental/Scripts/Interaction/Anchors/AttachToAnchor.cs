using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class AttachToAnchor : MonoBehaviour
    {
        [SerializeField] Handedness handToAttach;
        [SerializeField] AnchorPoint pointToAttach;

		private void Update()
		{
			DoAttach();
		}

		private void LateUpdate()
		{
			DoAttach();
		}

		void DoAttach()
		{
			if (handToAttach != Handedness.None)
			{
				InstrumentalHand hand = (handToAttach == Handedness.Left) ?
					InstrumentalHand.LeftHand : InstrumentalHand.RightHand;

				Pose attachPose = hand.GetAnchorPose(pointToAttach);
				transform.SetPositionAndRotation(attachPose.position, attachPose.rotation);
			}
		}
	}
}
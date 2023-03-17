using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Triggers
{
	public enum ExtensionState
	{
		None = 0,
		Extended = 1,
		Unextended = 2
	}

	public class FingerExtensionTrigger : PoseTrigger
    {
		[SerializeField] Handedness handedness;
		InstrumentalHand hand;

		[SerializeField] ExtensionState thumbExtension;
		[SerializeField] ExtensionState indexExtension;
		[SerializeField] ExtensionState middleExtension;
		[SerializeField] ExtensionState ringExtension;
		[SerializeField] ExtensionState pinkyExtension;

		bool thumbPasses = false;
		bool indexPasses = false;
		bool middlePasses = false;
		bool ringPasses = false;
		bool pinkyPasses = false;

		const float dotCutoff=0.45f;
		const float thumbDotCutoff=0.31f;
			
		const float lineDist=0.05f;

		Vector3 palmDirection;
		Vector3 palmThumbRef;

		void GetHand()
		{
			if (handedness == Handedness.Left) hand = InstrumentalHand.LeftHand;
			else if (handedness == Handedness.Right) hand = InstrumentalHand.RightHand;

			//if (hand) head = hand.transform.parent;
		}

		// Start is called before the first frame update
		void Start()
        {
			GetHand();
        }

        // Update is called once per frame
        void Update()
        {
			if (hand)
			{
				// get the palm direction
				Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);
				palmDirection = palmPose.rotation * Vector3.up;
				palmThumbRef = palmPose.rotation * Vector3.right;

				// do dot products
				// remember to try coming up with a continuous value
				// for signification
				// just like I mentioned in the docs

				// thumb dot product might need to be different, not sure.

				// index dot product
				Pose thumbPose = hand.GetAnchorPose(AnchorPoint.ThumbTip);
				Pose indexPose = hand.GetAnchorPose(AnchorPoint.IndexTip);
				Pose middlePose = hand.GetAnchorPose(AnchorPoint.MiddleTip);
				Pose ringPose = hand.GetAnchorPose(AnchorPoint.RingTip);
				Pose pinkyPose = hand.GetAnchorPose(AnchorPoint.PinkyTip);

				Vector2 thumbForward = thumbPose.rotation * Vector3.forward;
				Vector3 indexForward = indexPose.rotation * Vector3.forward;
				Vector3 middleForward = middlePose.rotation * Vector3.forward;
				Vector3 ringForward = ringPose.rotation * Vector3.forward;
				Vector3 pinkyForward = pinkyPose.rotation * Vector3.forward;

				float thumbDot = Vector3.Dot(thumbForward, palmDirection);
				float indexDot = Vector3.Dot(indexForward, palmDirection);
				float middleDot = Vector3.Dot(middleForward, palmDirection);
				float ringDot = Vector3.Dot(ringForward, palmDirection);
				float pinkyDot = Vector3.Dot(pinkyForward, palmDirection);

				indexPasses = indexDot > dotCutoff;
				middlePasses = middleDot > dotCutoff;
				ringPasses = ringDot > dotCutoff;
				pinkyPasses = pinkyDot > dotCutoff;
				thumbPasses = thumbDot > thumbDotCutoff; // it does look like the thumb is not extended when it's lower than 0.4 to 0.2 or so
			}
			else GetHand();
        }

		void DrawFingerState(bool passes, AnchorPoint anchorPoint)
		{
			Vector3 position = hand.GetAnchorPose(anchorPoint).position;
			Quaternion rotation = hand.GetAnchorPose(anchorPoint).rotation;
			Vector3 forward = rotation * Vector3.forward;

			Gizmos.color = (passes) ? Color.green : Color.red;
			Gizmos.DrawLine(position, position + (forward * lineDist));
		}

		private void OnDrawGizmos()
		{
			if(hand)
			{
				Pose palmPose = hand.GetAnchorPose(AnchorPoint.Palm);
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(palmPose.position, palmPose.position + (palmDirection * lineDist));
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(palmPose.position, palmPose.position + palmThumbRef);
				DrawFingerState(thumbPasses, AnchorPoint.ThumbTip);
				DrawFingerState(indexPasses, AnchorPoint.IndexTip);
				DrawFingerState(middlePasses, AnchorPoint.MiddleTip);
				DrawFingerState(ringPasses, AnchorPoint.RingTip);
				DrawFingerState(pinkyPasses, AnchorPoint.PinkyTip);
			}
		}
	}
}
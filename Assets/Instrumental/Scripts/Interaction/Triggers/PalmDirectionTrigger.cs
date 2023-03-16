using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental.Interaction.Triggers
{
    public class PalmDirectionTrigger : Trigger
	{
		public enum DirectionToCheck
        {
            UserUp,
            HeadForward
        }

		// want to support checking against the up direction.
		// eventually this should be a stand-in for the hip-up
		// direction, but for now just up is fine.
		[SerializeField] Handedness handedness;
        InstrumentalHand hand;
        Transform head;

        [SerializeField] DirectionToCheck directionToCheck;
        [Range(0, 90)]
        [SerializeField] float angle = 25;

        [SerializeField] float measuredAngle;

        Vector3 comparisonDirection;
        Vector3 palmDirection;

        // Start is called before the first frame update
        void Start()
        {
            GetHand();
        }

        void GetHand()
		{
            if (handedness == Handedness.Left) hand = InstrumentalHand.LeftHand;
            else if (handedness == Handedness.Right) hand = InstrumentalHand.RightHand;

            if (hand) head = hand.transform.parent;
        }

        Vector3 GetDirectionToCheck()
		{
			switch (directionToCheck)
			{
				case DirectionToCheck.UserUp:
                    return Vector3.up; // todo: replace this with a smoothly rotating tracked plane
				case DirectionToCheck.HeadForward:
                    return head.forward;
				default:
                    return Vector3.up;
			}
		}

        // Update is called once per frame
        void Update()
        {
            if(!hand)
			{
                GetHand();
			}
            else
			{
                comparisonDirection = GetDirectionToCheck();
                palmDirection = hand.GetAnchorPose(AnchorPoint.Palm).rotation * Vector3.forward;
                measuredAngle = Vector3.Angle(palmDirection, comparisonDirection);

                if (Vector3.Dot(palmDirection, comparisonDirection) < 0)
				{
                    measuredAngle = -1;
				}

                if (measuredAngle >= 0 && measuredAngle < angle)
                {
                    Activate();
                }
                else Deactivate();
			}
        }

		private void OnDrawGizmos()
		{
		    if(hand)
			{
                Gizmos.color = Color.blue;
                Vector3 rayOrigin = hand.GetAnchorPose(AnchorPoint.Palm).position;
                Gizmos.DrawRay(rayOrigin,
                    palmDirection);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(rayOrigin, comparisonDirection);
			}
		}
	}
}
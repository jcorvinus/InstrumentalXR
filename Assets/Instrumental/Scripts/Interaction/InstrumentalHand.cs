using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Instrumental.Interaction
{
	[System.Serializable]
    public enum Handedness { None=0, Left = 1, Right = 2 }
	[System.Serializable]
    public enum AnchorPoint 
    { 
        None=0,
        Palm=1,
		ThumbTip=2,
        IndexTip=3,
        MiddleTip=4,
		RingTip=5,
		PinkyTip=6
    }

	public class InstrumentalHand : MonoBehaviour
	{
		SteamVR_Behaviour_Pose handPose;
		[SerializeField] SteamVR_Behaviour_Skeleton dataHand;
		private Handedness hand;
		public Handedness Hand { get { return hand; } }

		const float basisDrawDist = 0.02f;

		const float palmForwardOffset = 0.0153f;
		const float palmUpOffset = 0.06f;
		const float palmRightOffset = 0.0074f;

		private static InstrumentalHand leftHand;
		private static InstrumentalHand rightHand;

		public static InstrumentalHand LeftHand { get { return leftHand; } }
		public static InstrumentalHand RightHand { get { return rightHand; } }

		private void Awake()
		{
            handPose = GetComponent<SteamVR_Behaviour_Pose>();

			if (handPose.inputSource == SteamVR_Input_Sources.LeftHand)
			{
				hand = Handedness.Left;
				leftHand = this;
			}
			else if (handPose.inputSource == SteamVR_Input_Sources.RightHand)
			{
				hand = Handedness.Right;
				rightHand = this;
			}
			else
			{
				Debug.LogError("Hand's pose did not have a valid input source");
			}
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public Pose GetAnchorPose(AnchorPoint anchorPoint)
		{
			bool flip = hand == Handedness.Left;
			Vector3 position = Vector3.zero;
			Vector3 forward = Vector3.right;
			Vector3 up = Vector3.up;

			Transform transform=null;

			switch (anchorPoint)
			{
				case AnchorPoint.None:
					return Pose.identity;

				case AnchorPoint.Palm:
					transform = dataHand.wrist;
					position = Vector3.zero;
					forward = Vector3.right * -1;
					up = Vector3.forward;

					position = new Vector3(
						-palmForwardOffset, 
						palmRightOffset /** ((flip) ? -1 : 1)*/,
						palmUpOffset);
					break;

				case AnchorPoint.IndexTip:
					transform = dataHand.indexTip;
					break;

				case AnchorPoint.MiddleTip:
					transform = dataHand.middleTip;
					break;

				case AnchorPoint.ThumbTip:
					transform = dataHand.thumbTip;
					break;

				case AnchorPoint.RingTip:
					transform = dataHand.ringTip;
					break;

				case AnchorPoint.PinkyTip:
					transform = dataHand.pinkyTip;
					break;

				default:
					break;
			}

			up = transform.TransformDirection(up);
			forward = transform.TransformDirection(forward);

			return new Pose(transform.TransformPoint(position), 
				Quaternion.LookRotation(forward, up));
		}

		void DrawBasis(Pose pose)
		{
			Vector3 up, forward, right;
			up = pose.rotation * Vector3.up;
			forward = pose.rotation * Vector3.forward;
			right = pose.rotation * Vector3.right;

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(pose.position, pose.position + 
				(up * basisDrawDist));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(pose.position, pose.position + 
				(forward * basisDrawDist));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(pose.position, pose.position + 
				(right * basisDrawDist));
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				// draw the palm
				DrawBasis(GetAnchorPose(AnchorPoint.Palm));

				// draw the index finger
				DrawBasis(GetAnchorPose(AnchorPoint.IndexTip));

				// draw middle finger
				DrawBasis(GetAnchorPose(AnchorPoint.MiddleTip));

				// draw the thumb tip
				DrawBasis(GetAnchorPose(AnchorPoint.ThumbTip));
			}
		}
	}
}
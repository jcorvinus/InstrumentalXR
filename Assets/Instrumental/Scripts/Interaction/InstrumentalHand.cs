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
		PinkyTip= 6
	}

	public enum Finger
	{ 
		Thumb=0,
		Index=1,
		Middle=2,
		Ring=3,
		Pinky=4
	}

	public struct PinchInfo
	{
		public Finger Finger;
		public Vector3 PinchCenter;
		public float PinchDistance;
		public float PinchAmount;
	}

	public class InstrumentalHand : MonoBehaviour
	{
		InstrumentalBody body;

		[SerializeField] SteamVR_Behaviour_Skeleton[] handAvatars;
		[SerializeField] SteamVR_Behaviour_Skeleton dataHand;
		private Handedness hand;
		public Handedness Hand { get { return hand; } }

		const float basisDrawDist = 0.02f;

		const float palmForwardOffset = 0.0153f;
		const float palmUpOffset = 0.06f;
		const float palmRightOffset = 0.0074f;

		#region finger extension and curls
		Pose palmPose;
		Pose thumbPose;
		Pose indexPose;
		Pose middlePose;
		Pose ringPose;
		Pose pinkyPose;

		bool thumbIsExtended = false;
		bool indexIsExtended = false;
		bool middleIsExtended = false;
		bool ringIsExtended = false;
		bool pinkyIsExtended = false;

		float thumbCurl = 0;
		float indexCurl = 0;
		float middleCurl = 0;
		float ringCurl = 0;
		float pinkyCurl = 0;
		#endregion

		#region Pinch stuff
		PinchInfo[] pinches;

		[Range(0,0.1f)]
		[SerializeField] float pinchMaxDistance=0.09f;
		[Range(0,0.1f)]
		[SerializeField] float pinchMinDistance = 0.01f;
		#endregion

		#region Finger Extension and curl accessors
		public bool ThumbIsExtended { get { return thumbIsExtended; } }
		public bool IndexIsExtended { get { return indexIsExtended; } }
		public bool MiddleIsExtended { get { return middleIsExtended; } }
		public bool RingIsExtended { get { return ringIsExtended; } }
		public bool PinkyIsExtended { get { return pinkyIsExtended; } }

		public float ThumbCurl { get { return thumbCurl; } }
		public float IndexCurl { get { return indexCurl; } }
		public float MiddleCurl { get { return middleCurl; } }

		public float RingCurl { get { return ringCurl; } }

		public float PinkyCurl { get { return pinkyCurl; } }
		#endregion

		#region Pinch Accessors
		public bool HasPinchInfo(Finger finger)
		{
			return (finger != Finger.Thumb);
		}

		public PinchInfo GetPinchInfo(Finger finger)
		{
			if(finger != Finger.Thumb)
			{
				return pinches[(int)finger];
			}
			else
			{
				Debug.LogError("No pinch for thumb");
				return new PinchInfo();
			}
		}

		/// <summary>
		/// The distance used for a 'fully open' pinch gesture, useful for
		/// pinch based visualizations. Does not tell you that the pinch is active
		/// </summary>
		public float PinchMaxDistance { get { return pinchMaxDistance; } }
		public float PinchMinDistance { get { return pinchMinDistance; } }
		#endregion

		const float curlCutoff = 0.3f;
		const float thumbCurlCutoff = 0.31f;

		private static InstrumentalHand leftHand;
		private static InstrumentalHand rightHand;

		public static InstrumentalHand LeftHand { get { return leftHand; } }
		public static InstrumentalHand RightHand { get { return rightHand; } }

		private void Awake()
		{
			body = GetComponentInParent<InstrumentalBody>();

			pinches = new PinchInfo[5];

			if (dataHand.inputSource == SteamVR_Input_Sources.LeftHand)
			{
				hand = Handedness.Left;
				leftHand = this;
			}
			else if (dataHand.inputSource == SteamVR_Input_Sources.RightHand)
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
			UpdateHandAvatars();
        }

		void UpdateHandAvatars()
		{
			for (int i = 0; i < handAvatars.Length; i++)
			{
				handAvatars[i].gameObject.SetActive(i == (int)body.Avatar);
			}
		}

		void GetAnchorPoses()
		{
			palmPose = GetAnchorPose(AnchorPoint.Palm);
			thumbPose = GetAnchorPose(AnchorPoint.ThumbTip);
			indexPose = GetAnchorPose(AnchorPoint.IndexTip);
			middlePose = GetAnchorPose(AnchorPoint.MiddleTip);
			ringPose = GetAnchorPose(AnchorPoint.RingTip);
			pinkyPose = GetAnchorPose(AnchorPoint.PinkyTip);
		}

		float GetFingerAngle(Vector3 baseDirection,
			Vector3 forward, Vector3 axis)
		{
			float signedAngle = Vector3.SignedAngle(baseDirection, forward, axis);

			if (signedAngle < 0 && signedAngle > -60) signedAngle = 0;
			else if (signedAngle < 0 /*&& signedAngle > -130*/)
			{
				float absExtra = Mathf.Abs(signedAngle);
				signedAngle = 180 + absExtra;
			}

			return Mathf.Clamp(signedAngle, 0, 340);
		}

		float GetFingerCurl(float angle)
		{
			return angle / 340;
		}

		void CalculateExtension()
		{
			Vector3 palmDirection = palmPose.rotation * Vector3.up;
			Vector3 palmThumbRef = palmPose.rotation * Vector3.right;

			Vector2 thumbForward = thumbPose.rotation * Vector3.forward;
			Vector3 indexForward = indexPose.rotation * Vector3.forward;
			Vector3 middleForward = middlePose.rotation * Vector3.forward;
			Vector3 ringForward = ringPose.rotation * Vector3.forward;
			Vector3 pinkyForward = pinkyPose.rotation * Vector3.forward;

			thumbCurl = GetFingerCurl(GetFingerAngle(palmThumbRef, thumbForward, palmDirection));
			indexCurl = GetFingerCurl(GetFingerAngle(palmDirection, indexForward, palmThumbRef));
			middleCurl = GetFingerCurl(GetFingerAngle(palmDirection, middleForward, palmThumbRef));
			ringCurl = GetFingerCurl(GetFingerAngle(palmDirection, ringForward, palmThumbRef));
			pinkyCurl = GetFingerCurl(GetFingerAngle(palmDirection, pinkyForward, palmThumbRef));

			indexIsExtended = (indexCurl < curlCutoff);
			middleIsExtended = (middleCurl < curlCutoff);
			ringIsExtended = (ringCurl < curlCutoff);
			pinkyIsExtended = (pinkyCurl < curlCutoff);
			thumbIsExtended = (thumbCurl < thumbCurlCutoff); // it does look like the thumb is not extended when it's lower than 0.4 to 0.2 or so
		}

        // Update is called once per frame
        void Update()
        {
			UpdateHandAvatars();
			GetAnchorPoses();
			CalculateExtension();
			ProcessPinches();
		}

		void ProcessPinches()
		{
			pinches[(int)Finger.Index] = ProcessPinch(Finger.Index);
			pinches[(int)Finger.Middle] = ProcessPinch(Finger.Middle);
			pinches[(int)Finger.Ring] = ProcessPinch(Finger.Ring);
			pinches[(int)Finger.Pinky] = ProcessPinch(Finger.Pinky);
		}

		PinchInfo ProcessPinch(Finger finger)
		{
			Vector3 thumbTip, fingerTip = Vector3.zero;
			Vector3 pinchCenter = Vector3.zero;
			float pinchDistance = 0, pinchAmount = 0;
			thumbTip = thumbPose.position;

			switch (finger)
			{
				case Finger.Thumb:
					break;
				case Finger.Index:
					fingerTip = indexPose.position;
					break;
				case Finger.Middle:
					fingerTip = middlePose.position;
					break;
				case Finger.Ring:
					fingerTip = ringPose.position;
					break;
				case Finger.Pinky:
					fingerTip = pinkyPose.position;
					break;
				default:
					break;
			}

			pinchCenter = (fingerTip + thumbTip) * 0.5f;

			Vector3 offset = (fingerTip - thumbTip);
			pinchDistance = offset.magnitude;
			pinchAmount = 1 - Mathf.InverseLerp(pinchMinDistance, pinchMaxDistance, pinchDistance);

			return new PinchInfo()
			{
				Finger = finger,
				PinchAmount = pinchAmount,
				PinchDistance = pinchDistance,
				PinchCenter = pinchCenter
			};
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
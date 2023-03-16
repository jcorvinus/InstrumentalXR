/* The MIT License (MIT)

Copyright (c) 2016 Joshua Corvinus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Instrumental;
using Instrumental.Interaction;
using Instrumental.Space;

namespace Instrumental.Controls
{
	public class ButtonRuntime : MonoBehaviour
	{
		#region Events
		public delegate void ButtonEventHandler(ButtonRuntime sender);
		public event ButtonEventHandler ButtonActivated;
		public event ButtonEventHandler ButtonHovered;
		public event ButtonEventHandler ButtonHoverEnded;
		#endregion

		Button button;

		#region Fingertip Tracking Variables
		[Header("Fingertip Tracking Variables")]
		//public List<GameObject> FingertipsInCollisionBounds;
		Collider boundsCollider;
		bool isLeftInBounds;
		bool isRightInBounds;

		[SerializeField]
		private float furthestPushPoint;
		public float FurthestPushPoint { get { return furthestPushPoint; } }
		public List<float> fingerDots;

		public bool WaitingForReactivation = false;

		InstrumentalHand leftHand;
		InstrumentalHand rightHand;
		#endregion

		#region Button Variables
		[Header("Button Variables")]
		public float ButtonFaceDistance;
		public float ButtonThrowDistance;
		public float CurrentThrowValue;

		bool isTouching;
		bool isHovering;
		bool isPressed;

		public bool IsTouching { get { return isTouching; } }
		public bool IsHovering { get { return isHovering; } }
		public bool IsPressed { get { return isPressed; } }

		public bool CanHighlight = true;
		#endregion

		#region Visual Variables
		public Transform ButtonFace;
		#endregion

		#region Audio Variables
		AudioClip hoverClip { get { return GlobalSpace.Instance.UICommon.HoverClip; } }
		AudioClip activateClip { get { return GlobalSpace.Instance.UICommon.ActivateClip; } }
		[SerializeField] AudioSource ThrowSource;
		[Range(0, 1)]
		public float VolumeModifier = 1;
		#endregion

		#region Debug Variables
		public bool EnableDebugLogging = false;
		#endregion

		// Use this for initialization
		void Awake()
		{
			boundsCollider = GetComponentInChildren<Collider>();
			button = GetComponent<Button>();
		}

		private void Start()
		{
			leftHand = InstrumentalHand.LeftHand;
			rightHand = InstrumentalHand.RightHand;
		}

		bool IsInBounds(Vector3 point)
		{
			Vector3 closestPointOnBounds = boundsCollider.ClosestPoint(point);

			return closestPointOnBounds == point;
		}

		Vector3 GetTipPosition(InstrumentalHand hand)
		{
			return hand.GetAnchorPose(AnchorPoint.IndexTip).position;
		}

		// Update is called once per frame
		void Update()
		{
			// check to see if there are any fingers in this button's region
			bool oldIsInBounds = isLeftInBounds || isRightInBounds;

			isLeftInBounds = leftHand != null && IsInBounds(GetTipPosition(leftHand));
			isRightInBounds = rightHand != null && IsInBounds(GetTipPosition(rightHand));

			bool isInBoundsThisFrame = isLeftInBounds || isRightInBounds;

			if (isInBoundsThisFrame != oldIsInBounds)
			{
				if (isInBoundsThisFrame) Hover();
				else CancelHover();
			}

			isHovering = isInBoundsThisFrame;

			if (isInBoundsThisFrame)
			{
				furthestPushPoint = ButtonFaceDistance;

				if (isLeftInBounds)
				{
					Vector3 leftTipPosition = GetTipPosition(leftHand);
					leftTipPosition = transform.InverseTransformPoint(leftTipPosition);

					if (furthestPushPoint > leftTipPosition.z) furthestPushPoint = leftTipPosition.z;
				}

				if (isRightInBounds)
				{
					Vector3 rightTipPosition = GetTipPosition(rightHand);
					rightTipPosition = transform.InverseTransformPoint(rightTipPosition);

					if (furthestPushPoint > rightTipPosition.z) furthestPushPoint = rightTipPosition.z;
				}

				isTouching = furthestPushPoint < ButtonFaceDistance;
				isPressed = (furthestPushPoint < ButtonThrowDistance);

				if (!WaitingForReactivation)
				{
					CurrentThrowValue = Mathf.InverseLerp(ButtonThrowDistance, ButtonFaceDistance, furthestPushPoint);
					ThrowSource.volume = Mathf.Lerp(0f, 1f, 1 - CurrentThrowValue * VolumeModifier);
					ThrowSource.pitch = Mathf.Lerp(1f, 1.84f, 1 - CurrentThrowValue); // used to be MathSupplement.UnitReciprocal
					if (furthestPushPoint < ButtonThrowDistance)
					{ 
						Activate();
					}
				}
				else
				{
					if (furthestPushPoint >= ButtonThrowDistance)
					{
						WaitingForReactivation = false;
						if (EnableDebugLogging) Debug.Log("Re-activation allowed.");
					}
				}
			}
			else
			{
				isTouching = false;
				isPressed = false;
				isHovering = false;
				ThrowSource.volume = 0;
				ThrowSource.pitch = 1;
			}

			if (isTouching)
			{
				float pushBackDistance = -(ButtonFaceDistance - furthestPushPoint);
				float minValue = button.Depth - (ButtonThrowDistance * 0.25f);
				// need to constrain the amount this can be pushed in by.
				ButtonFace.transform.localPosition = new Vector3(0, 0, //Mathf.Max(pushBackDistance, -ButtonThrowDistance)
					Mathf.Max(pushBackDistance, -minValue)); // I think I need to take the button's total
																		// physical depth into account here.
			}
			else
			{
				// can we spring this back into position? With overshoot?
				ButtonFace.transform.localPosition = Vector3.Lerp(ButtonFace.transform.localPosition, Vector3.zero, Time.deltaTime * 5);
			}
		}

		#region Functions
		public void Activate()
		{
			AudioSource.PlayClipAtPoint(activateClip, transform.position);
			WaitingForReactivation = true;
			if (ButtonActivated != null) ButtonActivated(this);
			if (EnableDebugLogging) Debug.Log("FingerButton: " + name + " activated.");
		}

		public void Hover()
		{
			AudioSource.PlayClipAtPoint(hoverClip, transform.position);

			isHovering = true;

			if (ButtonHovered != null) ButtonHovered(this);
			if (EnableDebugLogging)
			{
				Debug.Log("FingerButton: " + name + " hovered.");
				Debug.Log("Re-activation disallowed.");
			}
		}

		public void CancelHover()
		{
			isHovering = false;
			if (ButtonHoverEnded != null) ButtonHoverEnded(this);
			if (EnableDebugLogging) Debug.Log("FingerButton: " + name + " hover ended.");
		}
		#endregion

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position + (transform.forward * ButtonFaceDistance), 0.008f);

			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(transform.position + (transform.forward * ButtonThrowDistance), 0.008f);
		}
	}
}
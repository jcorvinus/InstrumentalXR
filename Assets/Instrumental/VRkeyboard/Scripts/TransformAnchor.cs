using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRKeyboard
{
	public class TransformAnchor : MonoBehaviour
	{
		public Transform Anchor;
		public Vector3 Offset;

		public Vector3 AnchorForward;
		public Vector3 AnchorUp;

		void ApplyAnchor()
		{
			transform.position = Anchor.TransformPoint(Offset);
			transform.rotation = Quaternion.LookRotation(Anchor.TransformDirection(AnchorForward), Anchor.TransformDirection(AnchorUp));
		}

		// Update is called once per frame
		void Update()
		{
			ApplyAnchor();
		}

		private void LateUpdate()
		{
			ApplyAnchor();
		}
	}
}
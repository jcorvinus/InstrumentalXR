using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction.Triggers;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class LeftMasterJoystick : MonoBehaviour
    {
        InstrumentalHand hand;
        LogicTrigger logicTrigger;
        RingActivator ringActivator;

		private void Awake()
		{
            hand = InstrumentalHand.LeftHand;
            logicTrigger = GetComponent<LogicTrigger>();
            ringActivator = GetComponentInChildren<RingActivator>();
		}

		// Start is called before the first frame update
		void Start()
        {
            ringActivator.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            Pose anchorPose = hand.GetAnchorPose(AnchorPoint.Palm);
            ringActivator.transform.position = anchorPose.position;

            ringActivator.enabled = logicTrigger.IsActive;
        }
    }
}
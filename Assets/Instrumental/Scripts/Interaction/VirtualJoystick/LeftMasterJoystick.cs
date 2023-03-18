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
            logicTrigger = GetComponent<LogicTrigger>();
            ringActivator = GetComponentInChildren<RingActivator>();
            GetHand();
		}

        void GetHand()
		{
            hand = InstrumentalHand.LeftHand;
        }

		// Start is called before the first frame update
		void Start()
        {
            ringActivator.enabled = false;
            GetHand();
        }

        // Update is called once per frame
        void Update()
        {
            if (hand)
            {
                Pose anchorPose = hand.GetAnchorPose(AnchorPoint.Palm);
                ringActivator.transform.position = anchorPose.position;

                ringActivator.enabled = logicTrigger.IsActive;
            }
        }
    }
}
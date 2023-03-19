using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public enum HandAvatar
    {
        Glove = 0,
        Capsule = 1
    }

    public class InstrumentalBody : MonoBehaviour
    {
        private static InstrumentalBody instance;
        public static InstrumentalBody Instance { get { return instance; } }

        private InstrumentalHand leftHand, rightHand;
        private Transform head;

        private Vector3 leftShoulder;
        private Vector3 rightShoulder;
        private Vector3 forwardDirection;
        private Quaternion noRollHeadRotation;

        [SerializeField] HandAvatar handAvatar = HandAvatar.Glove;

        public Transform Head { get { return head; } }

        public Vector3 LeftShoulder { get { return leftShoulder; } }
        public Vector3 RightShoulder { get { return rightShoulder; } }

        public HandAvatar Avatar { get { return handAvatar; } }

		private void Awake()
		{
            instance = this;

            for(int i=0; i < transform.childCount; i++)
			{
                InstrumentalHand handCandidate = transform.GetChild(i).GetComponent<InstrumentalHand>();

                if(handCandidate)
				{
                    if (handCandidate.Hand == Handedness.Left) leftHand = handCandidate;
                    else rightHand = handCandidate;
				}

                if (transform.GetChild(i).name == "Head") head = transform.GetChild(i);
			}
		}

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // get shoulder positions
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class PinchVisualization : MonoBehaviour
    {
        [SerializeField] Handedness handedness;
        [SerializeField] Finger pinchFinger = Finger.Index;
        InstrumentalHand hand;
        GameObject visModel;

		private void Awake()
		{
            visModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            SphereCollider visCollider = visModel.GetComponent<SphereCollider>();
            Destroy(visCollider);

            visModel.transform.localScale = Vector3.zero;
            visModel.gameObject.SetActive(false);
		}

		// Start is called before the first frame update
		void Start()
        {
            hand = (handedness == Handedness.Left) ? InstrumentalHand.LeftHand :
                InstrumentalHand.RightHand;
        }

        // Update is called once per frame
        void Update()
        {
            PinchInfo pinchInfo = hand.GetPinchInfo(pinchFinger);
            visModel.transform.position = pinchInfo.PinchCenter;
            visModel.transform.localScale = Vector3.one * (pinchInfo.PinchDistance * 0.5f);
            visModel.SetActive(true);
        }
    }
}
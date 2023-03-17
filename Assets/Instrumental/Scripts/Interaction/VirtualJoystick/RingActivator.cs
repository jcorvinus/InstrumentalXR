using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.VirtualJoystick
{
    public class RingActivator : MonoBehaviour
    {
        [SerializeField] float verticalOffset=0.1f;
        [SerializeField] float outerRingOffset = 0.05f;
        [SerializeField] float dampAmount=6f;
        Vector3 innerRingVelocity;
        GameObject outerTarget;

        [SerializeField] AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Range(0.1f, 0.3f)]
        [SerializeField]
        float introAnimDuration = 0.15f;
        [Range(0, 0.2f)]
        [SerializeField]
        float innerRingTimeOffset = 0.05f;
        float enabledTime = 0;

        // components
        [SerializeField] MeshRenderer innerRing;
        [SerializeField] MeshRenderer outerRing;

        // debug:
        [Range(0, 1)]
        [SerializeField] float debugRadius = 0.1f;

		private void Awake()
		{
            outerTarget = new GameObject("InnerTarget");
            outerTarget.transform.parent = transform;
            outerRing.transform.parent = null;
		}

		// Start is called before the first frame update
		void Start()
        {

        }

		private void OnEnable()
		{
            outerRing.enabled = true;
            outerRing.transform.localScale = Vector3.zero;
            innerRing.enabled = true;
            innerRing.transform.localScale = Vector3.zero;
		}

		private void OnDisable()
		{
            enabledTime = 0;
            if(innerRing) innerRing.enabled = false;
            if(outerRing) outerRing.enabled = false;
		}

		// Update is called once per frame
		void Update()
        {
            // doing local here so we can take advantage of parenting
            // and not get glitchy poses
            Vector3 outerRingPoint = (Vector3.up * (verticalOffset + outerRingOffset));
            Vector3 innerRingPoint = (Vector3.up * verticalOffset);

            if (enabledTime < introAnimDuration)
			{
                enabledTime += Time.deltaTime;

                float outerTValue = Mathf.InverseLerp(0, introAnimDuration, enabledTime);
                float innerTValue = Mathf.InverseLerp(innerRingTimeOffset, introAnimDuration, enabledTime);

                outerRing.transform.localPosition = Vector3.Lerp(Vector3.zero, outerRingPoint, outerTValue);
                outerRing.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scaleCurve.Evaluate(outerTValue));

                innerRing.transform.position = transform.position + Vector3.Lerp(Vector3.zero, innerRingPoint, innerTValue);
                innerRing.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, scaleCurve.Evaluate(outerTValue));
                outerTarget.transform.SetPositionAndRotation(innerRing.transform.position, innerRing.transform.rotation);
            }
            else
			{
                innerRing.transform.localPosition = innerRingPoint;
                outerTarget.transform.position = transform.position + outerRingPoint;
                outerRing.transform.position = Vector3.SmoothDamp(outerRing.transform.position, 
                    outerTarget.transform.position, 
                    ref innerRingVelocity, dampAmount);
			}
        }

		private void OnDrawGizmos()
		{
            Vector3 outerRingPoint = transform.position + (Vector3.up * (verticalOffset + outerRingOffset));
            Vector3 innerRingPoint = transform.position + (Vector3.up * verticalOffset);

            DebugExtension.DrawCircle(outerRingPoint, debugRadius);
            DebugExtension.DrawCircle(innerRingPoint, debugRadius);
        }
	}
}
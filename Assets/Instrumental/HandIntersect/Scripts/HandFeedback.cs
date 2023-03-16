using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Interaction;

namespace Instrumental
{
    public class HandFeedback : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [UnityEngine.Serialization.FormerlySerializedAs("volumeBoost")]
        [SerializeField] float depthVolumeBoost = 1;
        float isGrabbingTime = 0;
        bool grabbedPreviousFrame = false;
        float grabFadeDuration = 1f;

		HandPenetration[] penetrators;
		InstrumentalHand hand;
		/*RigidHand rigidHand;
		RigidFinger[] fingers;*/
	
		// haptic variables
		float frequency = 1; // from 1-320hz
		float oldFrequency;
		float frequencyDiffPeriod;
		float timeTillNextPulse; // if frequency pops above this, bend the curve a little.
		float amplitude = 1; // 0-1 range. Best to keep this high or you won't feel anything.

		public float Frequency { get { return frequency; } set { frequency = value; } } // switch these to lensed values later if you run into collisions - you probably won't though.
		public float Amplitude { get { return amplitude; }  set { amplitude = value; } }

		[SerializeField]
		bool doHaptics = false;

		[SerializeField]
		bool penetrationHaptics = true;
		[SerializeField] float currentDeepestPenetration; // debug only
		[Range(0,1)]
		[SerializeField] float penetrationHapticsMaxout = 0.02f;

		LensedValue<bool> overridePenetrationHaptics = new LensedValue<bool>(false);
		public LensedValue<bool> OverridePenetrationHaptics { get { return overridePenetrationHaptics; } }

		[SerializeField] Renderer[] handMeshes;
        [SerializeField] Color staticIntersectColor;
        [SerializeField] Color grabIntersectColor;
        Color hoverIntersectColor;
        int intersectColorHash;
        Color currentColor;

		// a bit of a hack but it'll prevent me from having to write tons of glue code
		private static HandFeedback leftHandFeedback;
		private static HandFeedback rightHandFeedback;

		public static HandFeedback LeftHand { get { return leftHandFeedback; } }
		public static HandFeedback RightHand { get { return rightHandFeedback; } }

		// UI fingertip glow stuff:
		[Range(0, 0.2f)]
		[SerializeField] float fingerUIDistanceGlow = 0.12f;
		private int leftFingertipHash;
		private int rightFingertipHash;
		private int fingerGlowDistanceHash;
		Vector3 leftGlowPoint;
		Vector3 rightGlowPoint;

		private void Awake()
		{
			/*rigidHand = GetComponent<RigidHand>();
			fingers = GetComponentsInChildren<RigidFinger>(true);*/
			hand = GetComponent<InstrumentalHand>();

			leftFingertipHash = Shader.PropertyToID("_GlobalLeftFingertip");
			rightFingertipHash = Shader.PropertyToID("_GlobalRightFingertip");
			fingerGlowDistanceHash = Shader.PropertyToID("_HandGlowMaxDistance");

			penetrators = transform.GetComponentsInChildren<HandPenetration>(true);

			if (hand.Hand == Handedness.Left) leftHandFeedback = this;
			else rightHandFeedback = this;
		}

		// Use this for initialization
		void Start()
        {
            intersectColorHash = Shader.PropertyToID("_HighlightColor");
            if((handMeshes != null && handMeshes.Length > 1)) hoverIntersectColor = handMeshes[0].material.GetColor(intersectColorHash);
            currentColor = staticIntersectColor;

			oldFrequency = frequency;
        }

		private bool IsGraspingObject()
		{
			bool isGrasping = false;

			// un-comment to restore left/right hand grapsing values
			// you will need to supply proper values though
			/*if(PlatformControllerManager.Instance != null)
			{
				InteractionController[] controllers = (rigidHand.Handedness == Chirality.Left) ? 
					PlatformControllerManager.Instance.LeftControllers : PlatformControllerManager.Instance.RightControllers;

				if (controllers != null)
				{
					foreach (InteractionController controller in controllers)
					{
						if (controller.isGraspingObject) { isGrasping = true; break; }
					}
				}
			}*/

			return isGrasping;
		}

		private bool IsPrimaryHovering()
		{
			bool isPrimaryHovering = false;

			/*if (PlatformControllerManager.Instance != null)
			{
				InteractionController[] controllers = (rigidHand.Handedness == Chirality.Left) ?
					PlatformControllerManager.Instance.LeftControllers : PlatformControllerManager.Instance.RightControllers;

				if (controllers != null)
				{
					foreach (InteractionController controller in controllers)
					{
						if (controller.isPrimaryHovering) { isPrimaryHovering = true; break; }
					}
				}
			}*/

			return isPrimaryHovering;
		}

		void SetHandRendererColors(Renderer handMesh)
		{
			if (handMesh)
			{
				if (IsGraspingObject())
				{
					currentColor = Color.Lerp(currentColor, grabIntersectColor, Time.deltaTime * 6f);
				}
				else if (IsPrimaryHovering())
				{
					currentColor = Color.Lerp(currentColor, hoverIntersectColor, Time.deltaTime * 6f);
				}
				else
				{
					currentColor = Color.Lerp(currentColor, staticIntersectColor, Time.deltaTime * 6f);
				}

				handMesh.material.SetColor(intersectColorHash, currentColor);
			}
		}

		void SetGlobalShaderFingertips()
		{
			//Vector3 centerPoint = ((hand.GetAnchorPose(AnchorPoint.IndexTip).position +
			//	(hand.GetAnchorPose(AnchorPoint.MiddleTip).position)) * 0.5f);

			Vector3 centerPoint = hand.GetAnchorPose(AnchorPoint.IndexTip).position;

			if (hand.Hand == Handedness.Left)
			{
				leftGlowPoint = centerPoint;
				Vector4 handGlowPoint = new Vector4(centerPoint.x, centerPoint.y, centerPoint.z);
				Shader.SetGlobalVector(leftFingertipHash, handGlowPoint);
			}
			else
			{
				rightGlowPoint = centerPoint;
				Vector4 handGlowPoint = new Vector4(centerPoint.x, centerPoint.y, centerPoint.z);
				Shader.SetGlobalVector(rightFingertipHash, handGlowPoint);
			}

			Shader.SetGlobalFloat(fingerGlowDistanceHash, fingerUIDistanceGlow);
		}

        private void Update()
        {
			if (handMeshes != null && handMeshes.Length > 0)
			{
				foreach (Renderer handRenderer in handMeshes) SetHandRendererColors(handRenderer);

				if (IsGraspingObject())
				{
					isGrabbingTime += Time.deltaTime;
					isGrabbingTime = Mathf.Min(isGrabbingTime, grabFadeDuration);
				}
				else
				{
					isGrabbingTime -= Time.deltaTime;
					isGrabbingTime = Mathf.Max(isGrabbingTime, 0);
				}

				grabbedPreviousFrame = IsGraspingObject();
			}

			SetGlobalShaderFingertips();
		}

		private void CalculatePenetrationHaptics(HandPenetration deepestPenetrator)
		{
			if (deepestPenetrator == null)
			{
				doHaptics = false;
				return;
			}

			currentDeepestPenetration = deepestPenetrator.MaxPenetrationDepth;

			float depthTValue = Mathf.InverseLerp(0, penetrationHapticsMaxout, deepestPenetrator.MaxPenetrationDepth);
			amplitude = Mathf.Lerp(0, 1, depthTValue) * GetGrabVolumeMultiplier();
			frequency = Mathf.Lerp(1, 40, depthTValue);
			doHaptics = true;
		}

		private void HapticUpdate()
		{
			timeTillNextPulse -= Time.fixedDeltaTime;

			if (frequency == 0)
			{
				frequency = 0;
				oldFrequency = 0;
				return;
			}
			else if (oldFrequency == 0)
			{
				// un-fuck things
				timeTillNextPulse = PeriodForFreq(frequency);
				oldFrequency = frequency;
			}

			float period = PeriodForFreq(frequency);
			frequencyDiffPeriod = period - PeriodForFreq(oldFrequency);

			timeTillNextPulse += frequencyDiffPeriod; // bend the curve

			if(timeTillNextPulse <= 0)
			{
				//vibration.Execute(0, Time.fixedDeltaTime, frequency, amplitude, inputSource);
				//PlatformManager.Instance.DoHapticsForCurrentPlatform(frequency, amplitude, Time.fixedDeltaTime, rigidHand.Handedness == Chirality.Left);
				timeTillNextPulse = period;
			}

			oldFrequency = frequency;
		}

		float PeriodForFreq(float frequency)
		{
			return (frequency != 0) ? 1 / frequency : float.PositiveInfinity;
		}

		float FrequencyForTime(float time)
		{
			return 1 / time;
		}

        float GetGrabVolumeMultiplier()
        {
            return 1 - Mathf.InverseLerp(0, grabFadeDuration, isGrabbingTime);
        }

		private HandPenetration GetDeepestPenetrator()
		{
			HandPenetration deepestPenetrator = penetrators[0];

			for (int i = 0; i < penetrators.Length; i++)
			{
				if (penetrators[i].MaxPenetrationDepth > deepestPenetrator.MaxPenetrationDepth)
				{
					deepestPenetrator = penetrators[i];
				}
			}

			return deepestPenetrator;
		}

		Vector3 GetPenetratorCenter(out int penetratorCount)
		{
			penetratorCount = 0;
			Vector3 penetratorCenter = Vector3.zero;

			for (int i = 0; i < penetrators.Length; i++)
			{
				if (penetrators[i].MaxPenetrationDepth > 0)
				{
					penetratorCount++;
					penetratorCenter += penetrators[i].GetGlobalCenter();
				}
			}

			if (penetratorCount > 0) penetratorCenter /= penetratorCount;

			return penetratorCenter;
		}

        private void FixedUpdate()
        {
			if (handMeshes != null && handMeshes.Length > 0) // swap this out with a physics check once the colliders are in
			{
				HandPenetration deepestPenetrator = GetDeepestPenetrator();

				audioSource.volume = (deepestPenetrator.MaxPenetrationDepth * depthVolumeBoost) * (IsGraspingObject() ? 0 : 1);
				if (Mathf.Approximately(audioSource.volume, 0) && audioSource.isPlaying) audioSource.Pause(); // don't waste CPU time playing nothing
				else audioSource.UnPause();

				// move audiosource to center of all penetrating bodies
				int penetratorCount = 0;
				Vector3 penetratorCenter = GetPenetratorCenter(out penetratorCount);

				audioSource.transform.position = penetratorCenter;

				if (!overridePenetrationHaptics.GetValue()) CalculatePenetrationHaptics(penetratorCount > 0 ? deepestPenetrator : null);
				if (doHaptics) HapticUpdate();
			}
		}

		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				if (hand.Hand == Handedness.Left)
				{
					Gizmos.DrawWireSphere(leftGlowPoint, fingerUIDistanceGlow);
				}
				else
				{
					Gizmos.DrawWireSphere(rightGlowPoint, fingerUIDistanceGlow);
				}
			}
		}
	}
}
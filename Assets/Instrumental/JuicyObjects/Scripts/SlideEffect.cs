using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental
{
    public class SlideEffect : MonoBehaviour
    {
        Rigidbody rigidBody;
		InteractionSound interactionSound;
        [SerializeField] ParticleSystem particle;
        [SerializeField] float maxMagnitude = 0.1f;
        [SerializeField] float maxSoundMagnitude = 1f;
        [SerializeField] float maxEmissionRate = 55.31f;
        [SerializeField] AnimationCurve slideSoundVolume = AnimationCurve.Linear(0, 0, 1, 1);
        ParticleSystem.EmissionModule emission;
		[SerializeField] bool enableAirTrail=true;
		[SerializeField] bool enableSlideSound = true;
        TrailRenderer trailRenderer;

		LensToken leftIsGrabbedToken;
		bool grabbedLeftPreviousFrame; // these values are only valid if also sliding.
		bool grabbedRightPreviousFrame;
		LensToken rightIsGrabbedToken;
		float maxHapticDist = 0.2f;
		float minHapticMagnitude = 0.0167f;
		float maxHapticMagnitude = 0.77f;

		public bool EnableSlideSound { get { return enableSlideSound; } set { enableSlideSound = value; } }
		public bool EnableAirTrail { get { return enableAirTrail; } set { enableAirTrail = value; } }

        int colliderCount = 0;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			interactionSound = GetComponent<InteractionSound>();
			trailRenderer = GetComponent<TrailRenderer>();
		}

		// Use this for initialization
		void Start()
        {
            emission = particle.emission;
            emission.rateOverTime = 0;
        }

		private void OnDisable()
		{
			emission.rateOverTime = 0;
		}

		// Update is called once per frame
		void Update()
        {
            trailRenderer.enabled = enableAirTrail && colliderCount == 0 && !IsGrasped();
            if (trailRenderer.enabled && HasSoundSource())
            {
				interactionSound.SlideSource.volume = 0;
            }
        }

		// once we've got a grasp system, re-implement this.
		private bool IsGrasped()
		{
			return false;
		}

		private bool HasSoundSource()
		{
			return (interactionSound && interactionSound.SlideSource);
		}

        private void OnCollisionStay(Collision collision)
        {
			if (!enabled) return;

            //if (collision.collider.gameObject.layer == InteractionManager.instance.contactBoneLayer) return; // contact bone layer means anything we have a hand collider on
            if (collision.collider.gameObject.tag == "NoSlideParticles") return; // we are trying to prevent the hand from invoking the slide effect on a grasped object.

            float particleTValue = Mathf.InverseLerp(0.1f, maxMagnitude, rigidBody.velocity.magnitude);
            if (HasSoundSource())
            {
                float soundTValue = Mathf.InverseLerp(0, maxSoundMagnitude, rigidBody.velocity.magnitude);
				interactionSound.SlideSource.volume = slideSoundVolume.Evaluate(soundTValue);
            }
            emission.rateOverTime = Mathf.Lerp(0, maxEmissionRate, particleTValue);
            particle.transform.position = collision.contacts[0].point;

			bool grabbedByLeft = false;
			bool grabbedByRight = false;

			// do grab feedback
			if (IsGrasped())
			{
				// grabbing vibration feedback
				float leftGrabDistance = 0;
				float rightGrabDistance = 0;

				bool isLeftGrabbing = false;
				bool isRightGrabbing = false;


				float hapticTValue = MathSupplement.Coserp(0, 1, Mathf.InverseLerp(minHapticMagnitude, maxHapticMagnitude, rigidBody.velocity.magnitude));

				GetGrabbingControllers(out grabbedByLeft, out grabbedByRight);

				if(isLeftGrabbing)
				{
					leftGrabDistance = 0; //interactionBehaviour.GetHoverDistance(leftGrabbingController.GetGraspPoint());

					HandFeedback.LeftHand.Amplitude = 1 - Mathf.InverseLerp(0, maxHapticDist, leftGrabDistance);
					HandFeedback.LeftHand.Frequency = Mathf.Lerp(0, 320, hapticTValue);
				}

				if(isRightGrabbing)
				{
					rightGrabDistance = 0; //interactionBehaviour.GetHoverDistance(rightGrabbingController.GetGraspPoint());

					HandFeedback.RightHand.Amplitude = 1 - Mathf.InverseLerp(0, maxHapticDist, rightGrabDistance);
					HandFeedback.RightHand.Frequency = Mathf.Lerp(0, 320, hapticTValue);
				}
			}

			if (grabbedByLeft != grabbedLeftPreviousFrame)
			{
				// state change
				if (grabbedByLeft) leftIsGrabbedToken = HandFeedback.LeftHand.OverridePenetrationHaptics.AddLens(new Lens<bool>(0, (grabbed) => true));
				else
				{
					leftIsGrabbedToken.Remove();
					leftIsGrabbedToken = null;
				}
			}

			if (grabbedByRight != grabbedRightPreviousFrame)
			{
				// state change
				if (grabbedByRight) rightIsGrabbedToken = HandFeedback.RightHand.OverridePenetrationHaptics.AddLens(new Lens<bool>(0, (grabbed) => true));
				else
				{
					rightIsGrabbedToken.Remove();
					rightIsGrabbedToken = null;
				}
			}

			// save our state
			grabbedLeftPreviousFrame = grabbedByLeft;
			grabbedRightPreviousFrame = grabbedByRight;
		}

		private void GetGrabbingControllers(out bool grabbedByLeft, out bool grabbedByRight/*, out InteractionController leftGrabbingController, out InteractionController rightGrabbingController*/)
		{
			/*leftGrabbingController = null;
			rightGrabbingController = null;*/

			/*foreach (InteractionController graspingController in interactionBehaviour.graspingControllers)
			{
				if (graspingController.isLeft) leftGrabbingController = graspingController;
				else rightGrabbingController = graspingController;
			}*/

			grabbedByLeft = false; //leftGrabbingController;
			grabbedByRight = false; //rightGrabbingController;
		}

        private void OnCollisionEnter(Collision collision)
        {
			if (!enabled) return;
            //if (collision.collider.gameObject.layer == InteractionManager.instance.contactBoneLayer) return;

            colliderCount++;
        }

        private void OnCollisionExit(Collision collision)
        {
            //if (collision.collider.gameObject.layer == InteractionManager.instance.contactBoneLayer) return;

            colliderCount--;

            if(colliderCount == 0)
            {
                emission = particle.emission;

                emission.rateOverTime = 0;
				if(HasSoundSource() && enableSlideSound) interactionSound.SlideSource.volume = 0;

				// ensure we don't have any remaining grabbing controller overrides
				if (leftIsGrabbedToken != null) { leftIsGrabbedToken.Remove(); grabbedLeftPreviousFrame = false; leftIsGrabbedToken = null; }
				if (rightIsGrabbedToken != null) { rightIsGrabbedToken.Remove(); grabbedRightPreviousFrame = false; rightIsGrabbedToken = null; }
            }
        }
    }
}
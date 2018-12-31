using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Instrumental.Tweening
{
	public enum TweenPlayStyle 
	{ 
		/// <summary>Plays the animation once, then stops until told to play again.</summary>
		Once,
		/// <summary>Loops indefinitely until ordered to cease.</summary>
		Loop,
		/// <summary>Loops indefinitely, but odd-numbered loops play in reverse.</summary>
		PingPong,
		/// <summary>Tweens based off of a boolean value.</summary>
		Switch
	}

	/// <summary>These describe a state of a tweener. They operate similar to the settings on a VCR/DVD player.</summary>
	public enum TweenState
	{
		Play,
		Pause,
		Stopped
	}

	/// <summary>
	/// This is a base class that provides common tweener functionality. It keeps track of an animation's current time index, as well as 
	/// handling the easing curve. The easing curve is baked into the public TValue value. There is presently no way of getting a guaranteed linear
	/// tvalue.
	/// </summary>
	public abstract class Tweener : MonoBehaviour
	{
		/// <summary>C# Event version of the Finished event. Use this if you'd prefer your signalling to be debuggable. Recommended, but increases code complexity.
		/// The SendMessage approach that uses CallWhenFinished and FinishListeners reduces code complexity at a performance and debuggability cost.</summary>
		public UnityEvent Finished;
        public event Action<Tweener> FinishedEvent;

		public TweenPlayStyle Style = TweenPlayStyle.Once;
		public TweenState TweenerState { get { return tweenerState; } }
		/// <summary>Biasing/Easing curve of the animation.</summary>
		public AnimationCurve AnimationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
		/// <summary>Time length of the animation.</summary>
		public float Duration;
		/// <summary>If TRUE the animation will start automatically when the scene begins.</summary>
		public bool StartOnAwake;
        /// <summary>If TRUE the animation will play automatically when the gameobject becomes active.</summary>
        public bool StartOnEnable;

		private TweenState tweenerState = TweenState.Pause;
		private float timeRemaining;

        private float previousTValue;
		private float tValue;
		/// <summary>Time in percentage format, 0-1.</summary>
		public float TValue { get
            {
                if (Style == TweenPlayStyle.Switch) return AnimationCurve.Evaluate(tValue);
                else return AnimationCurve.Evaluate((forward) ? tValue : 1 - tValue);
            }  set { tValue = value; } }
		/// <summary>If TRUE, we're in the reflection phase of a ping-pong loop animation.</summary>
		private bool pingPongReflection;

        [SerializeField]
        private bool forward = true;
		/// <summary>If the tweener is in switch mode, use this to set the tweener's direction. Forward will take you to 1, backwards will take you to 0.</summary>
         
		public bool Forward { get { return (Style == TweenPlayStyle.Switch) ? forward : !pingPongReflection; } set 
		{
			forward = value;
            if(enableDebugLogging) Debug.Log("Forward set to" + forward);
		}
		}

        [SerializeField] bool enableDebugLogging = false;

		// Use this for initialization
		public void Start()
		{
			timeRemaining = Duration;
			if (StartOnAwake) Play();
		}

        public void OnEnable()
        {
            if(StartOnEnable)
            {
                Stop();
                Play();
            }
        }

        #region Interaction Methods
        [CatchCo.ExposeMethodInEditor]
		public void Play()
		{
			if(tweenerState == TweenState.Stopped)
			{
				timeRemaining = Duration;
				tValue = 0;
                if(Style == TweenPlayStyle.PingPong) pingPongReflection = !pingPongReflection;
			}

			tweenerState = TweenState.Play;
		}

        [CatchCo.ExposeMethodInEditor]
        public void Pause()
		{
			tweenerState = TweenState.Pause;
		}

        [CatchCo.ExposeMethodInEditor]
        public void Stop()
		{
			tweenerState = TweenState.Stopped;
            timeRemaining = Duration;
        }
		#endregion

		// Update is called once per frame
		public void Update()
		{
			if (tweenerState == TweenState.Play)
			{
				if (Style != TweenPlayStyle.Switch)
				{
					timeRemaining -= Time.deltaTime;
					//tValue = (!pingPongReflection) ? MathSupplement.UnitReciprocal(Mathf.InverseLerp(0, Duration, timeRemaining)) : Mathf.InverseLerp(0, Duration, timeRemaining);
                    tValue = 1 - Mathf.InverseLerp(0, Duration, timeRemaining);

                    // note: animation curve eval gets done in the public accessor for tValue, in case you're looking for it!

                    if (timeRemaining <= 0) // handle finish cases
					{
						switch (Style)
						{
							case TweenPlayStyle.Once:
								Stop();
								break;

							case TweenPlayStyle.Loop:
								Stop();
								Play();
								break;

							case TweenPlayStyle.PingPong:
								Stop();
								Play();
								break;
						}

                        FinishTweener();
					}
				}
				else
				{
                    float increment = Mathf.InverseLerp(0, Duration, Time.deltaTime);
					tValue = Mathf.Clamp01(tValue + (increment * ((forward) ? 1 : -1)));

                    if(previousTValue != tValue) // we may have a state change on our hands
                    {
                        if(forward) // we check against the endcaps of the 0-1 range instead of looking at play/stop events since those aren't valid for switch type.
                        {
                            if (tValue == 1) FinishTweener();
                        }
                        else
                        {
                            if (tValue == 0) FinishTweener();
                        }
                    }

                    previousTValue = tValue;
				}
			}
		}

        private void FinishTweener()
        {
            if (Finished != null) Finished.Invoke();
            if (FinishedEvent != null) FinishedEvent(this);
        }
	}
}
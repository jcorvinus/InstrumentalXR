using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental
{
    /// <summary>
    /// If you have a boolean signal that can flip faster than
    /// is meaningful, because of jitter or something like that,
    /// use this to filter out those unwanted state changes
    /// </summary>
    public class BoolTimeFilter : MonoBehaviour
    {
        bool inputState = false; // this is the raw input, can contain jitter
        bool state = false; // this is the stored state, the one you can rely on
        float timer;
        float threshold = 0.1f;

		public bool State { get { return state; } }

        public void SetValue(bool newState, bool skipFilter)
		{
			if (skipFilter)
			{
				state = newState;
				inputState = newState;
				timer = 0;
			}
			else
			{
				inputState = newState;
			}
		}

		private void Update()
		{
			if(state != inputState)
			{
                if(timer >= threshold)
				{
					state = inputState;
				}
				else
				{
					timer += Time.deltaTime;
				}
			}
            else
			{
                timer = 0;
			}
		}
	}
}
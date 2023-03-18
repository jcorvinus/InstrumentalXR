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


        public void SetValue(bool newState, bool skipFilter)
		{
            // todo: figure this out
		}

		private void Update()
		{
			
		}
	}
}
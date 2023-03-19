using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Triggers
{
    public class Trigger : MonoBehaviour
    {
        public System.Action OnActivated;
        public System.Action OnDeactivated;

        bool isActive = false;

        /// <summary>
        /// A continuous 0-1 value that triggers can use to specify
        /// how close they are to activation/deactivation. Feedback components
        /// can use this for audio/visual/haptics to explain the trigger's current
        /// state
        /// </summary>
        protected float feedback;

        public bool IsActive { get { return isActive; } }
        public float Feedback { get { return feedback; } }

        public void Activate()
		{
            if(!isActive)
			{
                isActive = true;
                if (OnActivated != null) OnActivated();
			}
		}

        public void Deactivate()
		{
            if(isActive)
			{
                isActive = false;
                if (OnDeactivated != null) OnDeactivated();
			}
		}
    }
}
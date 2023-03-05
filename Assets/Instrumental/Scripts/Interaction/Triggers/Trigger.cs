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

        public bool IsActive { get { return isActive; } }

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
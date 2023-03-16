using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction.Triggers
{
    public class CameraBGVisualizer : MonoBehaviour
    {
        [SerializeField] Camera[] cameras;
        [SerializeField] Color inactiveColor;
        [SerializeField] Color activeColor;
        [SerializeField] Trigger trigger;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(trigger)
			{
                foreach(Camera currentCamera in cameras)
				{
                    currentCamera.backgroundColor = (trigger.IsActive) ? activeColor : inactiveColor;
				}
			}
        }
    }
}
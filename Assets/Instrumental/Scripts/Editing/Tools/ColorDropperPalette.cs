using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.Interaction;

namespace Instrumental.Editing.Tools
{
    public class ColorDropperPalette : MonoBehaviour
    {
        // components
        [SerializeField] ColorDropper dropper;
        [SerializeField] InteractionSlider slider;
        [SerializeField] Transform panel;
        Transform viewCamera;

        // properties
        [Range(0, 0.2f)]
        [SerializeField] float yHeightOffset = 0.2f;
        Vector3 sliderDefaultScale;

        [Range(0.02f, 0.1f)]
        [SerializeField]
        float sideOffset = 0.02f;

        [Range(0,6)]
        [SerializeField]
        float sliderReturnSpeed = 1;

        // state
        bool isEnabled = false;
        bool isOpen = false;

        private void Awake()
        {
            viewCamera = Camera.main.transform;
            sliderDefaultScale = slider.transform.localScale;
            panel.gameObject.SetActive(false);
            panel.transform.localScale = new Vector3(0, 1, 1);
        }

        // Use this for initialization
        void Start()
        {

        }

        Vector3 GetPosition()
        {
            Vector3 offsetDirection = (viewCamera != null) ? viewCamera.transform.right * -1 : Vector3.left;
            return dropper.transform.TransformPoint(Vector3.down * yHeightOffset) + (offsetDirection * sideOffset);
        }

        Quaternion GetOrientation()
        {
            Vector3 directionToHead = (viewCamera.transform.position - transform.position).normalized;
            return Quaternion.LookRotation(-directionToHead, viewCamera.up);
        }

        void SetPanelScaleForSlider()
        {
            panel.transform.localScale = new Vector3(1 - slider.HorizontalSliderPercent, 1, 1);
            panel.gameObject.SetActive(slider.HorizontalSliderPercent < 0.99f);
        }

        // Update is called once per frame
        void Update()
        {
            if(!isOpen) // we can only change enable or disable states while open
            {
                isEnabled = dropper.IsGrasped;

                if (!isEnabled && slider.transform.parent.gameObject.activeSelf) slider.transform.parent.gameObject.SetActive(false);
                else
                {
                    if (isEnabled && !slider.transform.parent.gameObject.activeSelf)
                    {
                        slider.transform.parent.gameObject.SetActive(true);
                    }
                }

                if(isEnabled && !slider.isGrasped)
                {
                    if (slider.contactingControllers.Count == 0)
                    {
                        float hoverNormalizedDistance = 1 - Mathf.InverseLerp(0, InteractionManager.instance.hoverActivationRadius, slider.primaryHoverDistance);
                        slider.transform.localScale = Vector3.Lerp(sliderDefaultScale * 0.25f, sliderDefaultScale, hoverNormalizedDistance);

                        // do our slider return to center
                        bool increase = slider.HorizontalSliderPercent > 0.6f; // increase means return to 'closed' position
                        float sliderAdjustValue = slider.HorizontalSliderPercent + sliderReturnSpeed * Time.deltaTime * ((increase) ? 1 : -1);
                        slider.HorizontalSliderPercent = slider.HorizontalSliderPercent = Mathf.Clamp01(sliderAdjustValue);
                    }
                    else
                    {
                        slider.transform.localScale = sliderDefaultScale;
                    }

                    transform.position = GetPosition();
                    transform.rotation = GetOrientation();
                    if (!panel.gameObject.activeSelf) panel.gameObject.SetActive(true);
                }
                else
                {
                    slider.transform.localScale = sliderDefaultScale;
                }

                SetPanelScaleForSlider();
                
                if(slider.HorizontalSliderPercent < 0.01f)
                {
                    slider.HorizontalSliderPercent = 0;
                    isOpen = true;
                }
            }
            else
            {
                // check to see if we should close
                if (slider.HorizontalSliderPercent > 0.02f)
                {
                    isOpen = false;
                    isEnabled = false;
                    slider.HorizontalSliderPercent = 1;
                    SetPanelScaleForSlider();
                }

                if(isOpen && !slider.isGrasped && slider.contactingControllers.Count == 0)
                {
                    // do our hover stuff
                    float hoverNormalizedDistance = 1 - Mathf.InverseLerp(0, InteractionManager.instance.hoverActivationRadius, slider.primaryHoverDistance);
                    slider.transform.localScale = Vector3.Lerp(sliderDefaultScale * 0.25f, sliderDefaultScale, hoverNormalizedDistance);

                    // return to full open position
                    if(slider.HorizontalSliderPercent > 0.01f)
                    {
                        slider.HorizontalSliderPercent -= sliderReturnSpeed * Time.deltaTime;
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(GetPosition(), 0.02f);
        }
    }
}
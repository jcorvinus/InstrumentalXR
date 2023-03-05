using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Overlay
{
    public class ScreenRendering : MonoBehaviour
    {
        CameraSetup cameraSetup;
        MeshRenderer meshRenderer;

		private void Awake()
		{
            cameraSetup = GetComponentInParent<CameraSetup>();
            meshRenderer = GetComponent<MeshRenderer>();
		}

		// Start is called before the first frame update
		void Start()
        {
            meshRenderer.material.mainTexture = cameraSetup.RenderTexture;

            float distance = cameraSetup.nearPlane;
            float frustumHeight = 2.0f * distance * Mathf.Tan(cameraSetup.FieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * cameraSetup.Aspect;

            transform.localScale = new Vector3(frustumWidth, frustumHeight, 1);
            transform.localPosition = Vector3.forward * (cameraSetup.nearPlane + 0.00001f);
        }
    }
}
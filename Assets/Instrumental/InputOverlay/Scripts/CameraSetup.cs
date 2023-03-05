using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Valve.VR;

namespace Instrumental.Overlay
{
    public class CameraSetup : MonoBehaviour
    {
		public UnityEvent OnSetupComplete;

		[SerializeField] RenderTexture renderTexture;
		public RenderTexture RenderTexture { get { return renderTexture; } }

		Camera leftEyeCamera;
		Camera rightEyeCamera;
		[SerializeField] Camera screenCamera;
		Camera centerEyeCamera;

		[Range(1, 1.5f)]
		[SerializeField] float perCameraFovMultiplier = 1.333f;
		float fieldOfView=90;
		float aspect=1;

		[SerializeField] SteamVR_Overlay debugOverlay;

		public float FieldOfView { get { return fieldOfView; } }
		public float Aspect { get { return aspect; } }
		public float nearPlane { get { return screenCamera.nearClipPlane; } }

		const UnityEngine.Experimental.Rendering.GraphicsFormat renderTextureFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
		const UnityEngine.Experimental.Rendering.GraphicsFormat depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;

		private void Awake()
		{
			Camera[] cameras = GetComponentsInChildren<Camera>();

			for(int i=0; i < cameras.Length; i++)
			{
				if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Left) leftEyeCamera = cameras[i];
				else if (cameras[i].stereoTargetEye == StereoTargetEyeMask.Right) rightEyeCamera = cameras[i];
				else if (screenCamera == null && cameras[i].stereoTargetEye == StereoTargetEyeMask.None) screenCamera = cameras[i];
			}

			// get our target resolution
			int textureWidth = (int)SteamVR.instance.sceneWidth;
			int textureHeight = (int)SteamVR.instance.sceneHeight;

			renderTexture = new RenderTexture(textureWidth, textureHeight,
				renderTextureFormat,
				depthStencilFormat, 
				0);

			Vector2 tanHalfFov;
			CVRSystem hmd = SteamVR.instance.hmd;

			float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
			hmd.GetProjectionRaw(EVREye.Eye_Left, ref l_left, ref l_right, ref l_top, ref l_bottom);

			float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
			hmd.GetProjectionRaw(EVREye.Eye_Right, ref r_left, ref r_right, ref r_top, ref r_bottom);

			tanHalfFov = new Vector2(
				Mathf.Max(-l_left, l_right, -r_left, r_right),
				Mathf.Max(-l_top, l_bottom, -r_top, r_bottom));

			fieldOfView = 2.0f * Mathf.Atan(tanHalfFov.y) * Mathf.Rad2Deg;
			screenCamera.fieldOfView = fieldOfView;
			aspect = screenCamera.aspect;
			Debug.Log("Starting fov: " + fieldOfView);

			if (leftEyeCamera && rightEyeCamera)
			{
				leftEyeCamera.targetTexture = renderTexture;
				rightEyeCamera.targetTexture = renderTexture;

				HmdMatrix34_t leftEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Left);
				leftEyeCamera.transform.localPosition = leftEyeMatrix.GetPosition();
				leftEyeCamera.fieldOfView = fieldOfView * perCameraFovMultiplier;

				HmdMatrix34_t rightEyeMatrix = hmd.GetEyeToHeadTransform(EVREye.Eye_Right);
				rightEyeCamera.transform.localPosition = rightEyeMatrix.GetPosition();
				rightEyeCamera.fieldOfView = fieldOfView * perCameraFovMultiplier;
			}
			else
			{
				centerEyeCamera = GetComponent<Camera>();
				centerEyeCamera.targetTexture = renderTexture;
				debugOverlay.texture = renderTexture;
			}

			if(OnSetupComplete != null)
			{
				OnSetupComplete.Invoke();
			}
		}

		[SerializeField] bool setFieldOfView;
		private void Update()
		{
			if(setFieldOfView)
			{
				leftEyeCamera.fieldOfView = fieldOfView * perCameraFovMultiplier;
				rightEyeCamera.fieldOfView = fieldOfView * perCameraFovMultiplier;
			}
		}
	}
}
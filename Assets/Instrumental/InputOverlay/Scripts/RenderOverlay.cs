using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Instrumental.Overlay
{
    public class RenderOverlay : MonoBehaviour
    {
        [SerializeField] Transform headTransform;
        CameraSetup cameraSetup;

        [SerializeField] Transform targetReference; // this represents our quad

        [Range(0.001f, 1f)]
        [SerializeField] float widthMultiplier = 1;

        [SerializeField] bool useFixedWidth = true;
        [SerializeField] float fixedWidth = 3f;

        static public string key { get { return "unity:" + Application.companyName + "." + Application.productName; } }

        private ulong handle = OpenVR.k_ulOverlayHandleInvalid;

        Coroutine renderLoop;

		private void Awake()
		{
            cameraSetup = headTransform.GetComponent<CameraSetup>();
		}

		private void OnEnable()
		{
            CVROverlay overlay = OpenVR.Overlay;
            EVROverlayError overlayError = OpenVR.Overlay.CreateOverlay(key,
                gameObject.name, ref handle);

            if (overlayError != EVROverlayError.None)
            {
                Debug.Log("<b>[SteamVR]</b> " + overlay.GetOverlayErrorNameFromEnum(overlayError));
                enabled = false;
                return;
            }

            overlayError = OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Parallel, true);
            if (overlayError != EVROverlayError.None)
            {
                Debug.Log("<b>[SteamVR]</b> " + overlay.GetOverlayErrorNameFromEnum(overlayError));
                enabled = false;
                return;
            }

            // start our rendering
            renderLoop = StartCoroutine(RenderLoop());
        }

        private void OnDisable()
		{
            StopCoroutine(renderLoop);
            if (handle != OpenVR.k_ulOverlayHandleInvalid)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    overlay.DestroyOverlay(handle);
                }

                handle = OpenVR.k_ulOverlayHandleInvalid;
            }
        }

        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        private IEnumerator RenderLoop()
        {
            while (Application.isPlaying)
            {
                yield return waitForEndOfFrame;

                UpdateOverlay();
            }
        }

        // Update is called once per frame
        void UpdateOverlay()
        {
            CVROverlay overlay = OpenVR.Overlay;

            if (overlay == null) return;

            if(cameraSetup.RenderTexture != null)
			{
                EVROverlayError error = overlay.ShowOverlay(handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(key, ref handle) != EVROverlayError.None)
                        return;
                }

                Texture_t tex = new Texture_t();
                tex.handle = cameraSetup.RenderTexture.GetNativeTexturePtr();
                tex.eType = SteamVR.instance.textureType;
                tex.eColorSpace = EColorSpace.Auto;
                error = overlay.SetOverlayTexture(handle, ref tex);

                if (error != EVROverlayError.None)
                {
                    Debug.LogError("Overlay error: " + error.ToString());
                    enabled = false;
                    return;
                }

                float distance = targetReference.localPosition.z;
                float frustumHeight = 2.0f * distance * Mathf.Tan(cameraSetup.FieldOfView * 0.5f * Mathf.Deg2Rad);
                float frustumWidth = frustumHeight * cameraSetup.Aspect;

                overlay.SetOverlayAlpha(handle, 1);

                if (!useFixedWidth)
                {
                    overlay.SetOverlayWidthInMeters(handle, frustumWidth * widthMultiplier);
                }
                else
				{
                    overlay.SetOverlayWidthInMeters(handle, fixedWidth * widthMultiplier);
                    targetReference.transform.localPosition = (Vector3.forward * fixedWidth / 3);
				}

                VRTextureBounds_t textureBounds = new VRTextureBounds_t();
                textureBounds.uMin = (0);
                textureBounds.vMin = (1);
                textureBounds.uMax = (1);
                textureBounds.vMax = (0);
                overlay.SetOverlayTextureBounds(handle, ref textureBounds);

                HmdVector2_t vecMouseScale = new HmdVector2_t();
                vecMouseScale.v0 = 1;
                vecMouseScale.v1 = 1;
                overlay.SetOverlayMouseScale(handle, ref vecMouseScale);

                SteamVR_Utils.RigidTransform offset = new SteamVR_Utils.RigidTransform(targetReference);
                //offset.pos.z += viewCamera.nearClipPlane;

                HmdMatrix34_t hmdMatrix = offset.ToHmdMatrix34();
                overlay.SetOverlayTransformTrackedDeviceRelative(handle, 0,
                    ref hmdMatrix);

                //overlay.SetOverlayInputMethod(handle, inputMethod);
            }
            else
            {
                overlay.HideOverlay(handle);
            }
        }

		private void OnDrawGizmos()
		{
            float distance = targetReference.localPosition.z;
            float frustumHeight = 2.0f * distance * Mathf.Tan(cameraSetup.FieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * cameraSetup.Aspect;

            Gizmos.matrix = Matrix4x4.TRS(cameraSetup.transform.position +
                (cameraSetup.transform.forward * cameraSetup.transform.localPosition.z), 
                cameraSetup.transform.rotation,
                Vector3.one);

            Gizmos.DrawWireCube(Vector3.zero, new Vector3(frustumWidth, frustumHeight, 0));

            Gizmos.matrix = Matrix4x4.identity;
        }
	}
}
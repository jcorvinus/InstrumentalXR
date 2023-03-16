using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class ButtonUnityGraphic : MonoBehaviour
    {
        Button button;
        ButtonRuntime buttonRuntime;
        ButtonModel buttonModel;        

        MeshFilter faceMeshFilter;
        MeshRenderer faceMeshRenderer;
        MeshFilter rimMeshFilter;
        MeshRenderer rimMeshRenderer;

        MaterialPropertyBlock faceMeshPropertyBlock;
        MaterialPropertyBlock rimMeshPropertyBlock;

        int glowAmountHash;
        int isPressingHash;
        int isGraspingHash;
        int isTouchingHash;
        int isHoveringHash;
        int useDistanceGlowHash;

        bool hasComponents = false;

        // Start is called before the first frame update
        void Start()
        {
            glowAmountHash = Shader.PropertyToID("_GlowAmount");
            isPressingHash = Shader.PropertyToID("_IsPressing");
            isGraspingHash = Shader.PropertyToID("_IsGrasping");
            isHoveringHash = Shader.PropertyToID("_IsHovering");
            isTouchingHash = Shader.PropertyToID("_IsTouching");
            useDistanceGlowHash = Shader.PropertyToID("_UseDistanceGlow");

            faceMeshPropertyBlock = new MaterialPropertyBlock();
            rimMeshPropertyBlock = new MaterialPropertyBlock();
            AcquireComponents();
        }

        void AcquireComponents()
		{
            if (!hasComponents)
            {
                button = GetComponent<Button>();
                buttonModel = GetComponent<ButtonModel>();
                buttonRuntime = GetComponent<ButtonRuntime>();
                faceMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();
                faceMeshRenderer = faceMeshFilter.GetComponent<MeshRenderer>();
                rimMeshFilter = transform.GetChild(1).GetComponent<MeshFilter>();
                rimMeshRenderer = rimMeshFilter.GetComponent<MeshRenderer>();

                buttonModel.PropertiesChanged += (ButtonModel sender) => { Regenerate(); };

                hasComponents = true;
            }
		}

        void Regenerate()
		{
            AcquireComponents();
            faceMeshFilter.sharedMesh = buttonModel.FaceMesh;
            rimMeshFilter.sharedMesh = buttonModel.RimMesh;
		}

		void OnValidate()
		{
            Regenerate();
		}

        // Update is called once per frame
        void Update()
        {
            // update our graphics
            faceMeshRenderer.GetPropertyBlock(faceMeshPropertyBlock);
            rimMeshRenderer.GetPropertyBlock(rimMeshPropertyBlock);

            // touchAmount should change to 'effect amount' and change behavior depending on 
            // hover vs touch
            float glowAmount = 0;
            if(buttonRuntime.IsHovering)
			{
                if(buttonRuntime.IsTouching)
				{
                    glowAmount = 1 - buttonRuntime.CurrentThrowValue;
				}
                else if (buttonRuntime.IsHovering) // not sure if I even wanna mess with this
				{
                    // todo: normalize this
                    float differenceValue = Mathf.Max(0, buttonRuntime.FurthestPushPoint - buttonRuntime.ButtonFaceDistance);
                    glowAmount = Mathf.InverseLerp(buttonRuntime.ButtonFaceDistance, 
                        buttonRuntime.ButtonFaceDistance + button.HoverHeight, differenceValue);
				}
                else
				{
                    
				}
			}

            // property block versions
            faceMeshPropertyBlock.SetFloat(glowAmountHash, glowAmount);
            faceMeshPropertyBlock.SetInteger(useDistanceGlowHash, 1); // I just realized that if I wanted to get
            faceMeshPropertyBlock.SetInteger(isPressingHash, buttonRuntime.IsPressed ? 1 : 0); // really crazy
            faceMeshPropertyBlock.SetInteger(isHoveringHash, buttonRuntime.IsHovering ? 1 : 0); // I could bitpack
            faceMeshPropertyBlock.SetInteger(isTouchingHash, buttonRuntime.IsTouching ? 1 : 0); // these bools intoa single integer

            faceMeshRenderer.SetPropertyBlock(faceMeshPropertyBlock);
        }
    }
}
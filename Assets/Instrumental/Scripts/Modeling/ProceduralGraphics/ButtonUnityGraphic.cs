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

        public GameObject FaceObject { get { return faceMeshRenderer.gameObject; } }
        public GameObject RimObject { get { return rimMeshRenderer.gameObject; } }

        bool hasComponents = false;

        // Start is called before the first frame update
        void Start()
        {
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

        }
    }
}
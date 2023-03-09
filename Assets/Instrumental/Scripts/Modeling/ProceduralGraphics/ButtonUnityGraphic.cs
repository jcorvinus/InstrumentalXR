using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class ButtonUnityGraphic : MonoBehaviour
    {
        ButtonModel buttonModel;

        MeshFilter faceMeshFilter;
        MeshRenderer faceMeshRenderer;
        MeshFilter rimMeshFilter;
        MeshRenderer rimMeshRenderer;

        [SerializeField] Material material;

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
                buttonModel = GetComponent<ButtonModel>();
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
            faceMeshRenderer.material = material;
            rimMeshFilter.sharedMesh = buttonModel.RimMesh;
            rimMeshRenderer.material = material;
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
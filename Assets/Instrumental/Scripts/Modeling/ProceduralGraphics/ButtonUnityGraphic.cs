using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class ButtonUnityGraphic : MonoBehaviour
    {
        ButtonModel faceButtonModel;
        [SerializeField] MeshFilter faceMeshFilter;
        [SerializeField] MeshRenderer faceMeshRenderer;

        [SerializeField] MeshFilter rimMeshFilter;
        [SerializeField] MeshRenderer rimMeshRenderer;

        [SerializeField] Material material;

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
                faceButtonModel = transform.GetChild(0).GetComponent<ButtonModel>();
                faceMeshFilter = faceButtonModel.GetComponent<MeshFilter>();
                faceMeshRenderer = faceButtonModel.GetComponent<MeshRenderer>();

                faceButtonModel.PropertiesChanged += (ButtonModel sender) => { Regenerate(); };

                hasComponents = true;
            }
		}

        void Regenerate()
		{
            AcquireComponents();
            faceMeshFilter.sharedMesh = faceButtonModel.FaceMesh;
            faceMeshRenderer.material = material;
            rimMeshFilter.sharedMesh = faceButtonModel.RimMesh;
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
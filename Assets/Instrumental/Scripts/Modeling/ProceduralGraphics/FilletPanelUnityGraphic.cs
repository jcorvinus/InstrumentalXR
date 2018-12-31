using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
    /// <summary>
    /// Unity version of the Fillet Panel Graphhic. Use this if for some reason,
    /// you cannot use Leap's Graphic Renderer.
    /// </summary>
    [RequireComponent(typeof(FilletPanel))]
    public class FilletPanelUnityGraphic : MonoBehaviour
    {
        FilletPanel filletPanel;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        [SerializeField] Material material;

        private void Awake()
        {
            GetComponents();
        }

        // Use this for initialization
        void Start()
        {
            filletPanel.GenerateMesh();
            meshFilter.sharedMesh = filletPanel.Mesh;
        }

        private void OnEnable()
        {
            filletPanel.GenerateMesh();
            meshFilter.sharedMesh = filletPanel.Mesh;
        }

        void GetComponents()
        {
            filletPanel = GetComponent<FilletPanel>();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();

            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = material;
            meshFilter.sharedMesh = filletPanel.Mesh;
        }

        private void OnValidate()
        {
            GetComponents();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
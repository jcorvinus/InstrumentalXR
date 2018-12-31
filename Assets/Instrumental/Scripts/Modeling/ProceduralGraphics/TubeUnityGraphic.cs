using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidigital.Modeling.ProceduralGraphics
{
    /// <summary>
    /// The Unity renderer version of the Tube Graphic. Use this if for some reason you cannot use a 
    /// Leap Graphic Renderer.
    /// </summary>
    [RequireComponent(typeof(TubeGraphic))]
    public class TubeUnityGraphic : MonoBehaviour
    {
        TubeGraphic tubeGraphic;

        // components
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        // Use this for initialization
        void Start()
        {
            tubeGraphic = GetComponent<TubeGraphic>();
            meshFilter = GetComponent<MeshFilter>();
            if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();

            meshRenderer = GetComponent<MeshRenderer>();
            if (!meshRenderer) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = tubeGraphic.GetMesh();
        }
    }
}
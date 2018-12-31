using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Attributes;

using CatchCo;

namespace Lucidigital.Modeling.ProceduralGraphics
{
    [RequireComponent(typeof(TubeGraphic))]
    public class TubeLeapGraphic : LeapMeshGraphicBase
    {
        [Tooltip("The mesh that will represent this graphic")]
        [EditTimeOnly]
        [SerializeField]
        private Mesh _mesh;

        TubeGraphic tubeGraphic;

        protected override void Awake()
        {
            base.Awake();
            tubeGraphic = GetComponent<TubeGraphic>();
        }

        private void SetMesh(Mesh mesh)
        {
            if (isAttachedToGroup && !attachedGroup.addRemoveSupported)
            {
                Debug.LogWarning("Changing the representation of the graphic is not supported by this rendering type");
            }

            isRepresentationDirty = true;
            _mesh = mesh;
        }

        public override void RefreshMeshData()
        {
            SetMesh(tubeGraphic.GetMesh());
            mesh = _mesh;
        }
    }
}
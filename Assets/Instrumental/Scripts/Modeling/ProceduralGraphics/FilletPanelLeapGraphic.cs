using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Attributes;

namespace Instrumental.Modeling.ProceduralGraphics
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(FilletPanel))]
    public class FilletPanelLeapGraphic : LeapMeshGraphicBase
    {
        FilletPanel filletPanel;

        protected override void Awake()
        {
            base.Awake();

            filletPanel = GetComponent<FilletPanel>();
            filletPanel.PropertiesChanged += FilletPanel_PropertiesChanged;
        }

        private void FilletPanel_PropertiesChanged(FilletPanel sender)
        {
            RefreshMeshData();
        }

        private void Start()
        {
            RefreshMeshData();
        }

        /*private void SetMesh(Mesh mesh)
        {
            if (isAttachedToGroup && !attachedGroup.addRemoveSupported)
            {
                Debug.LogWarning("Changing the representation of the graphic is not supported by this rendering type");
            }

            isRepresentationDirty = true;
        }*/

        private void Update()
        {
            RefreshMeshData();
        }

        public override void RefreshMeshData()
        {
            if (filletPanel == null) filletPanel = GetComponent<FilletPanel>();
            if (filletPanel.Mesh == null) filletPanel.GenerateMesh();

            //SetMesh(filletPanel.Mesh);
            isRepresentationDirty = true;
            mesh = filletPanel.Mesh;
        }
    }
}
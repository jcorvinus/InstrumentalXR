using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.GraphicalRenderer;
using Leap.Unity.Space;
using Instrumental.Modeling.ProceduralGraphics;
using Instrumental.Editing.Tools;

namespace Instrumental.Controls
{
    public class Panel : MonoBehaviour
    {

        LeapGraphicRenderer graphicRenderer;
        LeapSphericalSpace sphericalSpace;
        LeapCylindricalSpace cylindricalSpace;        

        LeapBoxGraphic boxPanelGraphic;
        LeapPanelGraphic panelGraphic;
        LeapPanelOutlineGraphic panelOutlineGraphic;
        FilletPanel filletPanel;

        BoxCollider rendererSpaceCollider;
        BoxCollider panelCollider;
        BoxCollider borderUpperCollider;
        BoxCollider borderLowerCollider;
        BoxCollider borderLeftCollider;
        BoxCollider borderRightCollider;
        Instrumental.Schema.PanelSchema panelSchema;
        bool isResizing = false;

        public Collider PanelCollider { get { return panelCollider; } }
        public float Radius { get { return panelSchema.SpaceCurveRadius; } set
            {
                panelSchema.SpaceCurveRadius = value;
                if (cylindricalSpace) cylindricalSpace.radius = value;
                if (sphericalSpace) sphericalSpace.radius = value;
            }
        }

        #region PanelHandles
        PanelHandle[] allHandles;
        PanelHandle upperHandle, lowerHandle, leftHandle, rightHandle,
            upperLeftHandle, lowerLeftHandle, upperRightHandle, lowerRightHandle;
        #endregion

        #region ColorZones
        [SerializeField]
        ColorZone outlineColorZone;
        UniformColorZone surfaceUniformColorZone;
        GradientColorZone surfaceGradientColorZone;
        #endregion

        Dictionary<string, UIControl> uiControls = new Dictionary<string, UIControl>();

        // todo: add resizable outside of design mode flag?

        bool hasInitialized = false; // don't allow certain changes when true.
        private bool isDesignerMode = true;
        public bool IsDesignerMode { get { return isDesignerMode; } }
        public bool IsResizing { get { return isResizing; } }

        private void Awake()
        {
            filletPanel = transform.GetChild(0).GetComponent<FilletPanel>();
            boxPanelGraphic = transform.GetChild(1).GetComponent<LeapBoxGraphic>();
            panelGraphic = transform.GetChild(2).GetComponent<LeapPanelGraphic>();
            panelOutlineGraphic = transform.GetChild(3).GetComponent<LeapPanelOutlineGraphic>();
            panelCollider = transform.GetChild(4).GetComponent<BoxCollider>();
            rendererSpaceCollider = GetComponent<BoxCollider>();

            surfaceUniformColorZone = panelCollider.GetComponent<UniformColorZone>();
            surfaceGradientColorZone = panelCollider.GetComponent<GradientColorZone>();

            #region Handles
            allHandles = GetComponentsInChildren<PanelHandle>(true);
            upperHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.UpperRail);
            lowerHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.LowerRail);
            leftHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.LeftRail);
            rightHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.RightRail);
            upperLeftHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.UpperLeftCorner);
            lowerLeftHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.LowerLeftCorner);
            upperRightHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.UpperRightCorner);
            lowerRightHandle = allHandles.First(item => item.Type == PanelHandle.HandleType.LowerRightCorner);
            #endregion

            #region Border Colliders
            borderUpperCollider = outlineColorZone.transform.GetChild(0).GetComponent<BoxCollider>();
            borderLowerCollider = outlineColorZone.transform.GetChild(1).GetComponent<BoxCollider>();
            borderLeftCollider = outlineColorZone.transform.GetChild(2).GetComponent<BoxCollider>();
            borderRightCollider = outlineColorZone.transform.GetChild(3).GetComponent<BoxCollider>();
            #endregion
        }

        // Use this for initialization
        void Start()
        {
            hasInitialized = true;

            foreach(PanelHandle handle in allHandles)
            {
                handle.SetPanel(this);
                handle.OnHandleGrasped += Handle_OnHandleGrasped;
                handle.OnhandleUngrasped += Handle_OnhandleUngrasped;

                //set handle initial position
                handle.transform.position = GetPositionForHandle(handle.Type);
            }

            switch (panelSchema.PanelType)
            {
                case Schema.PanelType.Square:
                    break;

                case Schema.PanelType.Fillet:
                    boxPanelGraphic.gameObject.SetActive(false);
                    panelGraphic.gameObject.SetActive(false);
                    panelOutlineGraphic.gameObject.SetActive(false);

                    // set fillet values properly
                    filletPanel.SetDepth(panelSchema.Depth);
                    filletPanel.SetDimensions(panelSchema.PanelDimensions);
                    filletPanel.SetRadius(panelSchema.Radius);
                    filletPanel.SetBorderInsetPercent(panelSchema.BorderThickness);
                    filletPanel.SetBorderColor(panelSchema.BorderColor);

                    // set surface color zones properly
                    surfaceGradientColorZone.enabled = filletPanel.FaceColorType == Modeling.ColorType.Gradient;
                    surfaceUniformColorZone.enabled = filletPanel.FaceColorType == Modeling.ColorType.FlatColor;

                    // then enable the fillet gameobject
                    filletPanel.gameObject.SetActive(true);
                    filletPanel.GenerateMesh();
                    break;
                default:
                    break;
            }

            SetSpaceType();

            SetBorderColliders();

            outlineColorZone.OnColorSet += (ColorZone sender, Color color, Vector3 point) => { SetBorderColor(color); };
            surfaceGradientColorZone.OnGradientChanged += SurfaceGradientColorZone_OnGradientChanged;

            // set up our controls
        }

        public void InitializeSchema(bool designerMode, Instrumental.Schema.PanelSchema panelSchema)
        {
            this.isDesignerMode = designerMode;
            this.panelSchema = panelSchema;
        }

        public Instrumental.Schema.PanelSchema GetSchema()
        {
            // ensure our schema has all of the UI controls
            Schema.ControlSchema[] controlSchema = new Schema.ControlSchema[uiControls.Count];

            int i = 0;
            foreach(UIControl control in uiControls.Values)
            {
                controlSchema[i] = control.GetSchema();
                i++;
            }

            panelSchema.Controls = controlSchema;

            return panelSchema;
        }

        public Vector2 Dimensions { get { return panelSchema.PanelDimensions; } }

        #region Child Controls Recordkeeping
        /// <summary>
        /// Because UI controls must have unique names, this method will tell you
        /// if the 
        /// </summary>
        /// <param name="controlName">The name of the control you'd like to check.</param>
        /// <param name="uniqueName">The supplied control name, trivially modified to be unique.</param>
        /// <returns>Whether or not the supplied name is unique</returns>
        public bool MustRenameControl(string controlName, out string uniqueName)
        {
            bool mustRename = uiControls.ContainsKey(controlName);

            if (mustRename)
            {
                // find uniquename
                int uniqueBumpCounter = 0;
                string newName = controlName + uniqueBumpCounter;

                while(uiControls.ContainsKey(newName))
                {
                    uniqueBumpCounter++;
                    newName = controlName + uniqueBumpCounter;
                }

                uniqueName = newName;
            }
            else uniqueName = controlName;

            return mustRename;
        }

        public void AddUIControl(UIControl control)
        {
            uiControls.Add(control.name, control);
        }

        public void RemoveUIControl(UIControl control)
        {
            uiControls.Remove(control.Name);
        }
        #endregion

        #region Panel Handle Methods
        private void Handle_OnHandleGrasped(PanelHandle handle)
        {
            // disable grasping for all the handles that aren't grasped
            foreach(PanelHandle currentHandle in allHandles)
            {
                if (currentHandle != handle) currentHandle.SetGrabbable(false);
            }

            isResizing = true;
        }

        private void Handle_OnhandleUngrasped(PanelHandle handle)
        {
            handle.transform.position = GetPositionForHandle(handle.Type);
            // enable grasping for all the handles
            foreach (PanelHandle currentHandle in allHandles)
            {
                if (currentHandle != handle) currentHandle.SetGrabbable(true);
            }

            isResizing = false;
        }

        bool HandleIsCorner(PanelHandle.HandleType handleType)
        {
            switch (handleType)
            {
                case PanelHandle.HandleType.UpperRail:
                case PanelHandle.HandleType.LowerRail:
                case PanelHandle.HandleType.LeftRail:
                case PanelHandle.HandleType.RightRail:
                    return false;

                case PanelHandle.HandleType.UpperLeftCorner:
                case PanelHandle.HandleType.LowerLeftCorner:
                case PanelHandle.HandleType.UpperRightCorner:
                case PanelHandle.HandleType.LowerRightCorner:
                    return true;
                default:
                    return false;
            }
        }

        public void SetDimensionsForPanel(PanelHandle handle)
        {
            Vector3 handleLocalPosition = transform.InverseTransformPoint(handle.transform.position);

            bool isCorner = HandleIsCorner(handle.Type);

            Vector2 newDimensions = panelSchema.PanelDimensions * 0.5f;

            switch (handle.Type)
            {
                case PanelHandle.HandleType.UpperRail:
                case PanelHandle.HandleType.LowerRail:
                    newDimensions = new Vector2(newDimensions.x, Mathf.Abs(handleLocalPosition.y));
                    break;

                case PanelHandle.HandleType.LeftRail:
                case PanelHandle.HandleType.RightRail:
                    newDimensions = new Vector2(Mathf.Abs(handleLocalPosition.x), newDimensions.y);
                    break;

                case PanelHandle.HandleType.UpperLeftCorner:
                case PanelHandle.HandleType.LowerLeftCorner:
                case PanelHandle.HandleType.UpperRightCorner:
                case PanelHandle.HandleType.LowerRightCorner:
                    newDimensions = new Vector2(Mathf.Abs(handleLocalPosition.x),
                        Mathf.Abs(handleLocalPosition.y));
                    break;
                default:
                    break;
            }

            newDimensions *= 2;

            bool didConstrain = ShouldPanelConstrain(newDimensions, out newDimensions);
            panelSchema.PanelDimensions = newDimensions;

            switch (panelSchema.PanelType)
            {
                case Schema.PanelType.Square:
                    break;
                case Schema.PanelType.Fillet:
                    filletPanel.SetDimensions(panelSchema.PanelDimensions);
                    break;
                default:
                    break;
            }

            panelCollider.size = new Vector3(newDimensions.x, newDimensions.y, panelSchema.Depth);
            SetBorderColliders();

            // for all the other handles, just set their position
            foreach (PanelHandle panelHandle in allHandles)
            {
                if(panelHandle != handle)
                {
                    panelHandle.transform.position = GetPositionForHandle(panelHandle.Type);
                }
            }

            SetSpaceColliderBounds();
        }

        bool ShouldPanelConstrain(Vector2 newDimensions, out Vector2 clippedDimensions)
        {
            clippedDimensions = new Vector2(Mathf.Clamp(newDimensions.x, FilletPanel.MIN_DIMENSION_SIZE, FilletPanel.MAX_DIMENSION_WIDTH),
                Mathf.Clamp(newDimensions.y, FilletPanel.MIN_DIMENSION_SIZE, FilletPanel.MAX_DIMENSION_HEIGHT));

            return (newDimensions.x > FilletPanel.MAX_DIMENSION_WIDTH || newDimensions.y > FilletPanel.MAX_DIMENSION_HEIGHT ||
                newDimensions.x < FilletPanel.MIN_DIMENSION_SIZE || newDimensions.y < FilletPanel.MIN_DIMENSION_SIZE);
        }

        public Vector3 GetPositionForHandle(PanelHandle.HandleType handleType)
        {
            switch (handleType)
            {
                case PanelHandle.HandleType.UpperRail:
                    return transform.TransformPoint(Vector2.up * (Dimensions.y * 0.5f));

                case PanelHandle.HandleType.LowerRail:
                    return transform.TransformPoint(Vector2.down * (Dimensions.y * 0.5f));

                case PanelHandle.HandleType.LeftRail:
                    return transform.TransformPoint(Vector2.left * (Dimensions.x * 0.5f));

                case PanelHandle.HandleType.RightRail:
                    return transform.TransformPoint(Vector2.right * (Dimensions.x * 0.5f));

                case PanelHandle.HandleType.UpperLeftCorner:
                    return transform.TransformPoint((Vector2.left * (Dimensions.x * 0.5f)) +
                        (Vector2.up * (Dimensions.y * 0.5f)));

                case PanelHandle.HandleType.LowerLeftCorner:
                    return transform.TransformPoint((Vector2.left * (Dimensions.x * 0.5f)) +
                        (Vector2.down * (Dimensions.y * 0.5f)));

                case PanelHandle.HandleType.UpperRightCorner:
                    return transform.TransformPoint((Vector2.right * (Dimensions.x * 0.5f)) +
                        (Vector2.up * (Dimensions.y * 0.5f)));

                case PanelHandle.HandleType.LowerRightCorner:
                    return transform.TransformPoint((Vector2.right * (Dimensions.x * 0.5f)) +
                        (Vector2.down * (Dimensions.y * 0.5f)));
                default:
                    return transform.position;
            }
        }
        #endregion

        #region Panel Space Methods
        public void SetSpaceType(Schema.SpaceType spaceType)
        {
            panelSchema.SpaceType = spaceType;
            SetSpaceType();
            Radius = panelSchema.SpaceCurveRadius;
        }

        void SetSpaceType()
        {
            switch (panelSchema.SpaceType)
            {
                case Schema.SpaceType.Rectilinear:
                    if (sphericalSpace != null)
                    {
                        sphericalSpace.enabled = false;
                        Destroy(sphericalSpace);
                    }

                    if(cylindricalSpace != null)
                    {
                        cylindricalSpace.enabled = false;
                        Destroy(cylindricalSpace);
                    }
                    break;

                case Schema.SpaceType.Cylindrical:
                    StartCoroutine(SetCylindricalSpaceCoroutine());
                    break;

                case Schema.SpaceType.Spherical:
                    StartCoroutine(SetSphereicalSpaceCoroutine());
                    break;
                default:
                    break;
            }
        }

        IEnumerator SetCylindricalSpaceCoroutine()
        {
            if (sphericalSpace != null)
            {
                sphericalSpace.enabled = false;
                Destroy(sphericalSpace);
            }

            yield return new WaitForEndOfFrame();

            if (cylindricalSpace == null) cylindricalSpace = gameObject.AddComponent<LeapCylindricalSpace>();
            cylindricalSpace.radius = panelSchema.SpaceCurveRadius;

            yield break;
        }

        IEnumerator SetSphereicalSpaceCoroutine()
        {
            if (cylindricalSpace != null)
            {
                cylindricalSpace.enabled = false;
                Destroy(cylindricalSpace);
            }

            yield return new WaitForEndOfFrame();

            if (sphericalSpace == null) sphericalSpace = gameObject.AddComponent<LeapSphericalSpace>();
            sphericalSpace.radius = panelSchema.SpaceCurveRadius;

            yield break;
        }

        void SetSpaceColliderBounds()
        {
            rendererSpaceCollider.size = new Vector3(panelSchema.PanelDimensions.x + (panelSchema.PanelDimensions.x * 0.1f),
                panelSchema.PanelDimensions.y + (panelSchema.PanelDimensions.y * 0.1f), rendererSpaceCollider.size.z);
        }
        #endregion

        #region Border Methods
        void SetBorderColor(Color color)
        {
            switch (panelSchema.PanelType)
            {
                case Schema.PanelType.Square:
                    break;

                case Schema.PanelType.Fillet:
                    filletPanel.SetBorderColor(color);
                    break;

                default:
                    break;
            }
        }

        private void SurfaceUniformColorZone_OnColorSet(ColorZone zone, Color color, Vector3 point)
        {
            switch (panelSchema.PanelType)
            {
                case Schema.PanelType.Square:
                    break;
                case Schema.PanelType.Fillet:
                    filletPanel.SetFaceColor(color);
                    break;
                default:
                    break;
            }
        }

        private void SurfaceGradientColorZone_OnGradientChanged(GradientColorZone sender)
        {
            switch (panelSchema.PanelType)
            {
                case Schema.PanelType.Square:
                    break;
                case Schema.PanelType.Fillet:
                    filletPanel.SetFaceGradient(sender.Gradient);
                    break;
                default:
                    break;
            }
        }

        void SetBorderColliders()
        {
            float radius = panelSchema.Radius;
            float borderThickness = panelSchema.BorderThickness;
            borderUpperCollider.transform.localPosition = Vector3.up * panelSchema.PanelDimensions.y * 0.5f;
            borderUpperCollider.size = new Vector3(panelSchema.PanelDimensions.x, radius * borderThickness,
                panelSchema.Depth * 2);

            borderLowerCollider.transform.localPosition = Vector3.down * panelSchema.PanelDimensions.y * 0.5f;
            borderLowerCollider.size = new Vector3(panelSchema.PanelDimensions.x, radius * borderThickness,
                panelSchema.Depth * 2);

            borderLeftCollider.transform.localPosition = Vector3.left * panelSchema.PanelDimensions.x * 0.5f;
            borderLeftCollider.size = new Vector3(radius * borderThickness, panelSchema.PanelDimensions.y,
                panelSchema.Depth * 2);

            borderRightCollider.transform.localPosition = Vector3.right * panelSchema.PanelDimensions.x * 0.5f;
            borderRightCollider.size = new Vector3(radius * borderThickness, panelSchema.PanelDimensions.y,
                panelSchema.Depth * 2);
        }
        #endregion

    }
}
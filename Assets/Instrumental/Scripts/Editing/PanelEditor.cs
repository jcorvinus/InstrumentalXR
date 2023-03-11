using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;
using Instrumental.Schema;
using Instrumental.Modeling.ProceduralGraphics;

namespace Instrumental.Editing
{
    public class PanelEditor : MonoBehaviour
    {
        [SerializeField]
        UISchema uiSchema;

        [SerializeField]
        Panel panel;

        [Range(0, 1)]
        [SerializeField]
        float panelStowDepth;

        Vector3 panelStartPosition;

        Tweening.TweenPosition positionTweener;

        [Range(0,1)]
        [SerializeField]
        float verticalPanelFloorOffset = 0.012f;

        [Header("Curvature Controls")]
        [SerializeField]
        Transform curvatureButtonContainer;

        /*[SerializeField]
        InteractionSlider curvatureSlider;*/

        [Range(0, 1)]
        [SerializeField]
        float curveButtonOffset = 0.012f;

        [Range(0, 3)]
        [SerializeField]
        float radiusMin = 0.1f;

        [Range(0, 4)]
        [SerializeField]
        float radiusMax = 3;

        public Panel Panel { get { return panel; } }

        private void Awake()
        {
            if (panel)
            {
                panelStartPosition = panel.transform.position;
                positionTweener = panel.GetComponent<Tweening.TweenPosition>();
                positionTweener.StartPosition = panelStartPosition;
                positionTweener.GoalPosition = panelStartPosition + Vector3.forward * panelStowDepth;
            }

            if (uiSchema != null && panel != null)
            {
                panel.InitializeSchema(true, uiSchema.Panel);
            }
        }

        // Use this for initialization
        IEnumerator Start()
        {
            yield return null;

            if (uiSchema != null && panel != null)
            {
                //SetPanelHeight();
                panel.gameObject.SetActive(true);

                SetCurvatureControlPosition();

                // set the slider to its default position
                /*curvatureSlider.HorizontalSliderPercent = Mathf.InverseLerp(radiusMin, radiusMin, uiSchema.Panel.SpaceCurveRadius);
                curvatureSlider.transform.parent.gameObject.SetActive(uiSchema.Panel.SpaceType != SpaceType.Rectilinear);

                curvatureSlider.HorizontalSlideEvent += (float slide) =>
                {
                    float radius = Mathf.Lerp(radiusMin, radiusMax, curvatureSlider.HorizontalSliderPercent);
                    panel.Radius = radius;
                };*/
            }
        }

        public void Save()
        {
            uiSchema.Panel = panel.GetSchema();
        }

        private void Update()
        {
            if(panel.IsResizing)
            {
                SetCurvatureControlPosition();
                //SetPanelHeight();
            }
        }

        public void SetPanelRectilinear()
        {
            panel.SetSpaceType(SpaceType.Rectilinear);
            //curvatureSlider.transform.parent.gameObject.SetActive(false);
        }

        public void SetPanelCylindrical()
        {
            panel.SetSpaceType(SpaceType.Cylindrical);
            /*curvatureSlider.transform.parent.gameObject.SetActive(true);
            curvatureSlider.HorizontalSliderPercent = Mathf.InverseLerp(radiusMin, radiusMin, uiSchema.Panel.SpaceCurveRadius);*/
        }

        public void SetPanelSpherical()
        {
            panel.SetSpaceType(SpaceType.Spherical);
            /*curvatureSlider.transform.parent.gameObject.SetActive(true);
            curvatureSlider.HorizontalSliderPercent = Mathf.InverseLerp(radiusMin, radiusMin, uiSchema.Panel.SpaceCurveRadius);*/
        }

        Vector3 GetCurvatureControlTargetPosition()
        {
            return panel.GetPositionForHandle(PanelHandle.HandleType.LowerRail) + (panel.transform.up * -curveButtonOffset);
        }

        private void SetCurvatureControlPosition()
        {
            curvatureButtonContainer.transform.position = GetCurvatureControlTargetPosition();
        }

        /// <summary>
        /// Move the panel so that its lower bound is a fixed height above the 
        /// work surface
        /// </summary>
        private void SetPanelHeight()
        {
            // move the panel to the lowest possible position
            Vector3 panelBottomPosition = panel.GetPositionForHandle(PanelHandle.HandleType.LowerRail);
            Vector3 offset = panel.transform.position - panelBottomPosition;

            float yHeight = transform.position.y + verticalPanelFloorOffset;

            panel.transform.position = new Vector3(panel.transform.position.x, yHeight + offset.y,
                panel.transform.position.z);
        }

        private void OnDrawGizmos()
        {
            if (uiSchema)
            {
                Gizmos.matrix = panel.transform.localToWorldMatrix;
                Gizmos.color = Color.white;
                if (uiSchema.Panel)
                {
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(uiSchema.Panel.PanelDimensions.x,
                        uiSchema.Panel.PanelDimensions.y, uiSchema.Panel.Depth));

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(FilletPanel.MIN_DIMENSION_SIZE, FilletPanel.MIN_DIMENSION_SIZE,
                        uiSchema.Panel.Depth));

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(FilletPanel.MAX_DIMENSION_WIDTH, FilletPanel.MAX_DIMENSION_HEIGHT,
                        uiSchema.Panel.Depth));
                }
                Gizmos.matrix = Matrix4x4.identity;
            }

            Vector3 startPos = (Application.isPlaying) ? panelStartPosition : panel.transform.position;

            Gizmos.DrawLine(startPos, startPos + Vector3.forward * panelStowDepth);

            //Gizmos.DrawWireCube(GetCurvatureControlTargetPosition(), new Vector3(0.1f, 0, 0.05f));

            //Gizmos.DrawWireCube(transform.position + (Vector3.up * verticalPanelFloorOffset), new Vector3(0.1f, 0, 0.05f));
        }
    }
}

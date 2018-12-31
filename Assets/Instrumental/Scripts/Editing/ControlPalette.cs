using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Editing
{
    public class ControlPalette : MonoBehaviour
    {
        PanelEditor panelEditor;
        BoxCollider controlZoneCollider;
        float rightSideBuffer;
        float originalXPosition;

        private void Awake()
        {
            panelEditor = FindObjectOfType<PanelEditor>();
            controlZoneCollider = GetComponent<BoxCollider>();
        }

        // Use this for initialization
        void Start()
        {
            Vector3 panelRightPoint = panelEditor.Panel.GetPositionForHandle(Controls.PanelHandle.HandleType.RightRail);
            rightSideBuffer = GetSliderPoint().x - panelRightPoint.x;

            originalXPosition = transform.position.x;
        }

        private Vector3 GetRightSidePoint()
        { 
            return panelEditor.Panel.GetPositionForHandle(Controls.PanelHandle.HandleType.RightRail) +
                    (Vector3.right * (rightSideBuffer + (controlZoneCollider.size.y * 0.5f)));
        }

        // Update is called once per frame
        void Update()
        {
            if (panelEditor.Panel.IsResizing)
            {
                Vector3 panelRightPoint = GetRightSidePoint();
                //Vector3 sliderPoint = GetSliderPoint();

                transform.position = new Vector3(Mathf.Max(originalXPosition, panelRightPoint.x), transform.position.y,
                    transform.position.z);
            }
        }

        Vector3 GetSliderPoint()
        {
            return transform.TransformPoint(new Vector3(
                0, (-controlZoneCollider.size.y) * 0.5f, 0) + controlZoneCollider.center);
        }

        private void OnDrawGizmosSelected()
        {
            if(controlZoneCollider == null) controlZoneCollider = GetComponent<BoxCollider>();

            Gizmos.DrawWireSphere(GetSliderPoint(), 0.02f);

            if (panelEditor != null && panelEditor.Panel != null)
            {
                Vector3 panelRightPoint = GetRightSidePoint();

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(panelRightPoint, 0.02f);
            }
        }
    }
}
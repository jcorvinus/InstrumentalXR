using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Controls
{
    public class PanelHandle : MonoBehaviour
    {
        public delegate void HandleHandler(PanelHandle handle);
        public event HandleHandler OnHandleMoved;
        public event HandleHandler OnHandleGrasped;
        public event HandleHandler OnhandleUngrasped;

        public enum HandleType
        {
            UpperRail,
            LowerRail,
            LeftRail,
            RightRail,
            UpperLeftCorner,
            LowerLeftCorner,
            UpperRightCorner,
            LowerRightCorner
        }

        [SerializeField] HandleType type;         
        public HandleType Type { get { return type; } }
        public bool IsGrasped { get { return false; /*return interaction.isGrasped;*/ } }

        //InteractionBehaviour interaction;
        Collider[] colliders;
        Transform model;
        Panel owningPanel;
        Vector3 startScale;

        private void Awake()
        {
            //interaction = GetComponent<InteractionBehaviour>();
            model = transform.GetChild(0);
            colliders = GetComponentsInChildren<Collider>();
        }

        // Use this for initialization
        void Start()
        {
            /*interaction.OnGraspBegin += () =>
            {
                if (OnHandleGrasped != null)
                {
                    OnHandleGrasped(this);
                }
            };

            interaction.OnGraspEnd += () =>
            {
                if(OnhandleUngrasped != null)
                {
                    OnhandleUngrasped(this);
                }
            };*/

            //interaction.OnGraspedMovement += OnGraspedMovement;

            startScale = model.transform.localScale;
        }

        public void SetPanel(Panel panel)
        {
            owningPanel = panel;
            foreach(Collider collider in colliders)
            {
                Physics.IgnoreCollision(collider, panel.PanelCollider);
            }
        }

        void OnGraspedMovement(Vector3 preSolvedPos, Quaternion preSolvedRot,
            Vector3 solvedPos, Quaternion solvedRot)
        {
            Vector3 constrainedLocalPosition = transform.parent.InverseTransformPoint(solvedPos);
            constrainedLocalPosition = new Vector3(constrainedLocalPosition.x, constrainedLocalPosition.y, 0);

            // enforce any handle-specific constraints
            switch (type)
            {
                case HandleType.UpperRail:
                    constrainedLocalPosition.x = 0;
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small
                    break;
                case HandleType.LowerRail:
                    constrainedLocalPosition.x = 0;
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    break;
                case HandleType.LeftRail:
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = 0;
                    break;
                case HandleType.RightRail:
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = 0;
                    break;
                case HandleType.UpperLeftCorner:
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    break;
                case HandleType.LowerLeftCorner:
                    constrainedLocalPosition.x = Mathf.Min(-0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    break;

                case HandleType.UpperRightCorner:
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    constrainedLocalPosition.y = Mathf.Max(0.001f, constrainedLocalPosition.y); // don't let it go negative or get too small

                    break;
                case HandleType.LowerRightCorner:
                    constrainedLocalPosition.y = Mathf.Min(-0.001f, constrainedLocalPosition.y);// don't let it go negative or get too small
                    constrainedLocalPosition.x = Mathf.Max(0.001f, constrainedLocalPosition.x);
                    break;
                default:
                    break;
            }

            /*interaction.rigidbody.MovePosition(transform.parent.TransformPoint(constrainedLocalPosition));
            interaction.rigidbody.MoveRotation(transform.parent.rotation);*/

            owningPanel.SetDimensionsForPanel(this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetGrabbable(bool grabbable)
        {
            //interaction.ignoreGrasping = !grabbable;
            model.transform.localScale = (grabbable) ? startScale : startScale * 0.4f;
        }
    }
}
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Controls
{
    public enum ControlMode
    {
        /// <summary>
        /// UI is live, recieving user manipulation and operating
        /// on data. Usser cannot alter placement of controls.
        /// </summary>
        Runtime,
        /// <summary>
        /// Design mode. User can place controls but not activate them.
        /// </summary>
        Design,
        /// <summary>
        /// Control belongs to a palette menu - user will grab and pull to 
        /// instantiate.
        /// </summary>
        Design_Palette
    }

    public abstract class UIControl : MonoBehaviour
    {
        [SerializeField] ControlMode mode = ControlMode.Design_Palette;

        // check to see if we're a member of a panel
        // should we emit events for property changed so that
        // the panel can update the schema data?

        private Panel attachedPanel; // this can be null, we don't have to be attached to a panel,
                                     // but it is significant if we are.

        /*protected AnchorableBehaviour anchorable;
        protected InteractionBehaviour placementInteraction;*/
        private Rigidbody placementRigidbody;
        protected GameObject editSoundEmitterGameObject;
        protected AudioSource placementGrabSource;
        protected AudioSource placementDropSource;
        protected Editing.SpaceChanger spaceChanger;

        protected string _name="";
        private bool isPrecisionPlacement = false;

        [Header("Debug Vars")]
        [SerializeField]
        [Range(0, 3)]
        int orientationPreviewID;

        protected virtual void Awake()
        {
            /*anchorable = GetComponent<AnchorableBehaviour>();
            placementInteraction = GetComponent<InteractionBehaviour>();*/
            placementRigidbody = GetComponent<Rigidbody>();
            spaceChanger = GetComponent<Editing.SpaceChanger>();

            switch (mode)
            {
                case ControlMode.Runtime:
                    // look for panel in parent
                    attachedPanel = GetComponentInParent<Panel>();
                    break;

                case ControlMode.Design:
                    // if InteractionBehavior doesn't exist,
                    // create it. AnchorableBehavior not necessary?
                    // delete it if it exists?
                    /*if (placementInteraction == null) placementInteraction = gameObject.AddComponent<InteractionBehaviour>();
                    placementInteraction.allowMultiGrasp = true;
                    placementInteraction.graspedMovementType = InteractionBehaviour.GraspedMovementType.Kinematic;*/

                    // if we're in design mode or design palette mode,
                    // create our edit sound emitters
                    CreateEditSoundEmitters();
                    spaceChanger.SpaceChanged += SpaceChanger_SpaceChanged;
                    break;

                case ControlMode.Design_Palette:
                    // if anchorable and InteractionBehavior don't exist,
                    // create them
                    /*if(placementInteraction == null) placementInteraction = gameObject.AddComponent<InteractionBehaviour>();
                    if (anchorable == null) anchorable = gameObject.AddComponent<AnchorableBehaviour>();
                    placementInteraction.allowMultiGrasp = true;
                    placementInteraction.graspedMovementType = InteractionBehaviour.GraspedMovementType.Nonkinematic;*/

                    // if we're in design mode or design palette mode,
                    // create our edit sound emitters
                    CreateEditSoundEmitters();
                    spaceChanger.SpaceChanged += SpaceChanger_SpaceChanged;
                    break;

                default:
                    break;
            }
        }

        private void SpaceChanger_SpaceChanged(Editing.SpaceChanger sender, GameObject oldSpace, GameObject newSpace)
        {
            if(attachedPanel)
            {
                // we're leaving our current panel, tell it to remove us from its recordkeeping system
                attachedPanel.RemoveUIControl(this);
                attachedPanel = null;
            }

            Panel panelCandidate = newSpace.GetComponent<Panel>();
            if (panelCandidate)
            {
                attachedPanel = panelCandidate;

                // we're joining this new panel, tell it to add us to its recordkeeping system.
                string newName = "";
                if(attachedPanel.MustRenameControl(this._name, out newName))
                {
                    this._name = newName;
                    attachedPanel.AddUIControl(this);
                }
            }
        }

        private void CreateEditSoundEmitters()
        {
            editSoundEmitterGameObject = new GameObject("EditSoundEmitters", new System.Type[] { typeof(AudioSource),
            typeof(AudioSource)});

            editSoundEmitterGameObject.transform.SetParent(transform);
            editSoundEmitterGameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);

            AudioSource[] audioSourceComponents = editSoundEmitterGameObject.GetComponents<AudioSource>();
            placementGrabSource = audioSourceComponents[0];
            placementDropSource = audioSourceComponents[1];
        }

        protected virtual void Start()
        {
            // if our edit time sources exist, set them up properly.
            if(placementGrabSource)
            {
                placementGrabSource.playOnAwake = false;
                placementGrabSource.spatialBlend = 1;
                placementGrabSource.spatialize = true;
                placementGrabSource.Stop();
                placementGrabSource.clip = Instrumental.Space.GlobalSpace.Instance.UICommon.GrabClip;
                placementGrabSource.outputAudioMixerGroup = Instrumental.Space.GlobalSpace.Instance.UICommon.MasterGroup;
                placementGrabSource.minDistance = 0.1f;
                placementGrabSource.maxDistance = 2f;

                //placementInteraction.OnGraspBegin += PlacementGraspBegin;
            }

            if(placementDropSource)
            {
                placementDropSource.playOnAwake = false;
                placementDropSource.spatialBlend = 1;
                placementDropSource.spatialize = true;
                placementDropSource.Stop();
                placementDropSource.clip = Instrumental.Space.GlobalSpace.Instance.UICommon.ItemPlaceClip;
                placementDropSource.outputAudioMixerGroup = Instrumental.Space.GlobalSpace.Instance.UICommon.MasterGroup;
                placementDropSource.minDistance = 0.1f;
                placementDropSource.maxDistance = 2f;
            }

            /*if (placementInteraction)
            {
                placementInteraction.allowMultiGrasp = true;
                placementInteraction.OnGraspedMovement += DoEditorGraspMovement;
                placementInteraction.OnGraspEnd += PlacementGraspEnd;
            }*/
        }

        void PlacementGraspBegin()
        {
            placementGrabSource.Play();
            //if (placementInteraction.graspingControllers.Count > 1) isPrecisionPlacement = true;
        }

        void PlacementGraspEnd()
        {
            /*if (placementInteraction.graspingControllers.Count == 0)
            {
                if (attachedPanel) placementDropSource.Play();

                isPrecisionPlacement = false;
                placementInteraction.rigidbody.isKinematic = true;
            }*/

            if(!isPrecisionPlacement)
            {
                if(attachedPanel) InstantAngleSnap();
            }
        }

        [System.Serializable]
        struct AngleSnap
        {
            public Quaternion orientation;
            public float angleDist;
        }

        // up down, left, right angle snaps
        AngleSnap[] angleSnap = new AngleSnap[4];

        void InstantAngleSnap()
        {
            angleSnap[0].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
            angleSnap[0].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[0].orientation);

            angleSnap[1].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(270, Vector3.forward);
            angleSnap[1].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[1].orientation);

            angleSnap[2].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(180, Vector3.forward);
            angleSnap[2].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[2].orientation);

            angleSnap[3].orientation = attachedPanel.transform.rotation;
            angleSnap[3].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[3].orientation);

            AngleSnap closestSnap = angleSnap.First(item => item.angleDist == angleSnap.Min(subItem => subItem.angleDist));

            //placementInteraction.rigidbody.MoveRotation(Quaternion.Slerp(placementRigidbody.rotation, closestSnap.orientation, Time.deltaTime * 6f));
            //placementInteraction.rigidbody.MoveRotation(closestSnap.orientation);
        }

        void SlerpAngleSnap()
        {
            angleSnap[0].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(90, Vector3.forward);
            angleSnap[0].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[0].orientation);

            angleSnap[1].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(270, Vector3.forward);
            angleSnap[1].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[1].orientation);

            angleSnap[2].orientation = attachedPanel.transform.rotation * Quaternion.AngleAxis(180, Vector3.forward);
            angleSnap[2].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[2].orientation);

            angleSnap[3].orientation = attachedPanel.transform.rotation;
            angleSnap[3].angleDist = Quaternion.Angle(placementRigidbody.rotation, angleSnap[3].orientation);

            AngleSnap closestSnap = angleSnap.First(item => item.angleDist == angleSnap.Min(subItem => subItem.angleDist));

            //placementInteraction.rigidbody.rotation = (Quaternion.Slerp(placementRigidbody.rotation, closestSnap.orientation, 0.5f));
            //placementInteraction.rigidbody.MoveRotation(closestSnap.orientation);
        }

        void DoEditorGraspMovement(Vector3 preSolvedPos, Quaternion preSolvedRot,
            Vector3 solvedPos, Quaternion solvedRot/*, List<InteractionController> graspingControllers*/)
        {
            if (attachedPanel != null)
            {
                // todo: add placement assistance and precision movement here
                if(!isPrecisionPlacement)
                {
                    // angle snap
                    // target quaternion should be closest 45 degree angle
                    SlerpAngleSnap();

                    // check for alignment to other bounds

                    // snap to surface
                }
                else
                {
                    // precision placement mode
                }
            }
        }

        /// <summary>Call this when instantiating a control from schema values.</summary>
        /// <param name="controlSchema">Schema to reference when initializing.</param>
        public abstract void SetSchema(Schema.ControlSchema controlSchema);
        /// <summary>Gets schema data for the current control.</summary>
        /// <returns>Schema data for current control.</returns>
        public abstract Schema.ControlSchema GetSchema(); 

        public ControlMode Mode { get { return mode; } }
        public string Name { get { return _name; } }
        public abstract ControlType GetControlType();

        private void OnDrawGizmos()
        {
            if(attachedPanel)
            {
                Quaternion orientation = Quaternion.identity;

                orientation = angleSnap[orientationPreviewID].orientation;

                Vector3 forward, up, right;

                forward = orientation * Vector3.forward;
                up = orientation * Vector3.up;
                right = orientation * Vector3.right;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.position + up * 0.2f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + forward * 0.2f);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + right * 0.2f);
            }
        }
    }
}
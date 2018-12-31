using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

namespace Instrumental.Controls
{
    public class CloneAnchor : MonoBehaviour
    {
        public delegate void CloneHandler();
        public event CloneHandler Cloned;

        float pullDist = 0.15f;
        Anchor anchor;
        GameObject anchorTarget;

        private void Awake()
        {
            anchor = GetComponent<Anchor>();
            anchor.allowMultipleObjects = false;
        }

        // Use this for initialization
        IEnumerator Start()
        {
            anchor.allowMultipleObjects = false;

            while (!anchor.hasAnchoredObjects) yield return null;

            anchorTarget = anchor.anchoredObjects.First(item => true).gameObject;
            InteractionBehaviour targetInteraction = anchorTarget.GetComponent<InteractionBehaviour>();
            targetInteraction.OnGraspBegin += OnGraspBegin;
            targetInteraction.OnGraspEnd += OnGraspEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if (anchorTarget)
            {
                float dist = Vector3.Distance(anchorTarget.transform.position, transform.position);
                if (dist >= pullDist)
                {
                    InteractionBehaviour targetInteraction = anchorTarget.GetComponent<InteractionBehaviour>();
                    targetInteraction.OnGraspEnd -= OnGraspEnd;
                    targetInteraction.OnGraspBegin -= OnGraspBegin;

                    // clone stuff, manage attachment
                    GameObject clone = GameObject.Instantiate(anchorTarget, anchorTarget.transform.parent);
                    InteractionBehaviour cloneInteraction = clone.GetComponent<InteractionBehaviour>();
                    cloneInteraction.OnGraspEnd += OnGraspEnd;
                    cloneInteraction.OnGraspBegin += OnGraspBegin;

                    // enable our anchor
                    anchor.enabled = true;
                    InteractionBehaviour interaction = clone.GetComponent<InteractionBehaviour>();
                    AnchorableBehaviour anchorable = clone.GetComponent<AnchorableBehaviour>();
                    //anchorable.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
                    interaction.rigidbody.position = transform.position;
                    interaction.rigidbody.rotation = transform.rotation;

                    anchorable.anchor = anchor;
                    anchorable.TryAttach(true);
                    anchorTarget = clone.gameObject;
                }
            }
        }

        void OnGraspBegin()
        {
            // disable our anchor so that nothing else can get shoved into the slot
            // while we're out
            anchor.enabled = false;
        }

        void OnGraspEnd()
        {
            // return item to slot!
            anchor.enabled = true;
            AnchorableBehaviour anchorable = anchorTarget.GetComponent<AnchorableBehaviour>();
            anchorable.anchor = anchor;
            anchorable.TryAttach(true);
        }

        private void AnchorObject(InteractionBehaviour interaction)
        {
            anchor.enabled = true;
            interaction.rigidbody.position = anchor.transform.position;
            interaction.rigidbody.rotation = anchor.transform.rotation;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, pullDist);
        }
    }
}
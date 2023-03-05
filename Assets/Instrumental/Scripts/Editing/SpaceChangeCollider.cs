using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Space;

namespace Instrumental.Editing
{ 
    public class SpaceChangeCollider : MonoBehaviour
    {
        int layerMask;
        SpaceChanger changer;

        private void Awake()
        {
            changer = GetComponentInParent<SpaceChanger>();
            layerMask = LayerMask.NameToLayer("SpaceZone");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == layerMask)
            {
                /*LeapGraphicRenderer newRenderer = other.GetComponent<LeapGraphicRenderer>();

                if (newRenderer)
                {
                    changer.ChangeSpaces(newRenderer);
                }*/
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == layerMask)
            {
                /*LeapGraphicRenderer rendererCandidate = other.GetComponent<LeapGraphicRenderer>();

                if (rendererCandidate != null && rendererCandidate != GlobalSpace.Instance.GraphicRenderer)
                {
                    changer.ChangeSpaces(GlobalSpace.Instance.GraphicRenderer);

                    //transform.SetParent(GlobalSpace.Instance.GraphicRenderer.transform);

                    //currentRenderer = GlobalSpace.Instance.GraphicRenderer;
                    //for (int i = 0; i < allGraphics.Length; i++)
                    //{
                    //    // find the render group by name
                    //    LeapGraphicGroup group = GlobalSpace.Instance.GraphicRenderer.FindGroup(allGraphicsInfo[i].favoriteGroupName);
                    //    allGraphics[i].TryDetach();
                    //    group.TryAddGraphic(allGraphics[i]);
                    //}
                }*/
            }
        }
    }
}
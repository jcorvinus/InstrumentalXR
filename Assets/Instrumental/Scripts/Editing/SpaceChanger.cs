//using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.GraphicalRenderer;

using Instrumental.Space;

namespace Instrumental.Editing
{
    public class SpaceChanger : MonoBehaviour
    {
        public delegate void SpaceChangeHandler(SpaceChanger sender, GameObject oldSpace, GameObject newSpace);
        public event SpaceChangeHandler SpaceChanged;

        struct GraphicInfo
        {
            public string favoriteGroupName;
            public int attachedGroupIndex;
        }

        LeapGraphicRenderer currentRenderer;
        LeapGraphic[] allGraphics;
        GraphicInfo[] allGraphicsInfo;

        private void Awake()
        {
            currentRenderer = GetComponentInParent<LeapGraphicRenderer>();

            allGraphics = GetComponentsInChildren<LeapGraphic>(true);
            allGraphicsInfo = new GraphicInfo[allGraphics.Length];
        }

        private void Start()
        {
            LeapGraphicRenderer masterRenderer = GlobalSpace.Instance.GraphicRenderer;
            for (int i = 0; i < allGraphics.Length; i++)
            {
                allGraphicsInfo[i] = new GraphicInfo()
                {
                    favoriteGroupName = allGraphics[i].favoriteGroupName,
                    attachedGroupIndex = masterRenderer.groups.FindIndex(item => item.name == allGraphics[i].favoriteGroupName)
                };
            }
        }
        
        IEnumerator ChangeSpacesCoroutine(LeapGraphicRenderer newRenderer)
        {
            // remove all graphics
            for (int i = 0; i < allGraphics.Length; i++)
            {
                if (allGraphics[i].attachedGroup != null)
                {
                    string attachedGroupName = allGraphics[i].attachedGroup.name;
                    bool didRemove = (allGraphics[i].attachedGroup != null) ? allGraphics[i].attachedGroup.TryRemoveGraphic(allGraphics[i]) : false;

                    if (!didRemove)
                    {
                        Debug.LogError("Tried to detach graphic: " + allGraphics[i].name + " from group " + attachedGroupName);
                    }
                }
                else
                {
                    //Debug.LogError("Couldn't detach graphic: " + allGraphics[i].name + " because it had no attached group.");
                }
            }

            transform.SetParent(newRenderer.transform);

            yield return new WaitForEndOfFrame();

            for (int i = 0; i < allGraphics.Length; i++)
            {
                // find the render group by name
                LeapGraphicGroup group = newRenderer.FindGroup(allGraphicsInfo[i].favoriteGroupName);
                bool didAdd = group.TryAddGraphic(allGraphics[i]);
                if (!didAdd) Debug.LogError("Failed to attach graphic " + allGraphics[i].name + " to group " + group.name);
            }

            if(SpaceChanged != null)
            {
                SpaceChanged(this, currentRenderer.gameObject, newRenderer.gameObject);
            }

            currentRenderer = newRenderer;
        }

        public void ChangeSpaces(LeapGraphicRenderer newRenderer)
        {
            if (currentRenderer == newRenderer) return; // don't double process

            StartCoroutine(ChangeSpacesCoroutine(newRenderer));
        }
    }
}
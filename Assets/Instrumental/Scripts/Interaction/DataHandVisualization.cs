using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Interaction
{
    public class DataHandVisualization : MonoBehaviour
    {
        [SerializeField] Transform rootTransform;
		[Range(0, 0.1f)]
		[SerializeField] float basisDrawDist = 0.02f;

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void DrawBone(Transform bone)
		{
			// draw our basis vectors
			Vector3 forward, up, right;
			forward = bone.rotation * Vector3.forward;
			up = bone.rotation * Vector3.up;
			right = bone.rotation * Vector3.right;

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(bone.position, bone.position + (forward * basisDrawDist));
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(bone.position, bone.position + (up * basisDrawDist));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(bone.position, bone.position + (right * basisDrawDist));
			Gizmos.color = Color.white;

			for(int i=0; i < bone.childCount; i++)
			{
				Transform child = bone.GetChild(i);
				Gizmos.DrawLine(bone.position, child.position);
				DrawBone(child);
			}
		}

		private void OnDrawGizmos()
		{
			if (enabled)
			{
				// draw our entire skeleton from the root
				DrawBone(rootTransform.GetChild(0));
			}
		}
	}
}
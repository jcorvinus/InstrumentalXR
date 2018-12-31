using UnityEngine;
using System.Collections;

namespace Instrumental.Modeling
{
	public class SplineKnot : MonoBehaviour
	{
		public enum KnotType { Corner, Cubic }
		public KnotType Type;
		/// <summary>if TRUE the handles will be kept co-linear.</summary>
		public bool LockHandles;

		[SerializeField]
		private Vector3 localA = new Vector3(1, 0, 0);
		[SerializeField]
		private Vector3 localB = new Vector3(-1, 0, 0);

		public Vector3 LocalA { get { return localA; } set { localA = value; } }
		public Vector3 LocalB { get { return localB; } set { localB = value; } }

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (!Application.isPlaying)
			{
				transform.localScale = Vector3.one;
				transform.localRotation = Quaternion.identity;
			}
		}

		void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(transform.position, 0.0125f);
		}
	}
}
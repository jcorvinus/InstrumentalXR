using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Instrumental.Modeling
{
	[ExecuteInEditMode]
	public class Spline : MonoBehaviour
	{
		[SerializeField]
		private List<SplineKnot> knots;
		[SerializeField]
		private int lineDetail = 10; // we'll eventually need to calculate this based off of camera positioning and such
		[SerializeField]
		private bool closeShape = false;
		[SerializeField]
		private List<Vector3> vertexList;

		public List<SplineKnot> Knots { get { return knots; } set { knots = value; } }
		public int LineDetail { get { return lineDetail; } set { lineDetail = value; } }
		public bool CloseShape { get { return closeShape; } set { closeShape = value; } }

		/// <summary>
		/// Vertex list from the last available render. This is the entire spline, not just the anchor/control points.
		/// If it doesn't have what you expect, try calling RenderVerts().
		/// </summary>
		public List<Vector3> VertexList	{ get { return vertexList; } }
		public Vector3 CenterOfPoints { get { return MathSupplement.CenterOfPoints(vertexList.ToArray()); } }
		public int SegmentCount { get { return knots.Count - ((closeShape) ? 0 : 1); } }

		// Use this for initialization
		void Start()
		{

		}

		void OnEnable()
		{
			GetKnots();

			// todo: we need to validate our spline so that we can make sure the user didn't do anything dumb like child objects to it
			// screw up the order/count of knots, etc.
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void GetKnots()
		{
			if (knots == null) knots = new List<SplineKnot>();
			else knots.Clear();

			if (vertexList == null) vertexList = new List<Vector3>();

			knots.AddRange(GetComponentsInChildren<SplineKnot>());
		}

		#region Interpolation Methods
		/// <summary>
		/// Gets the position between two linear points.
		/// </summary>
		/// <param name="tValue">0-1 value that determines how far between points to go.</param>
		/// <returns>linear interpolation between the two points</returns>
		public Vector3 GetLinear(float tValue, Vector3 point1, Vector3 point2)
		{
			return Vector3.Lerp(point1, point2, tValue);
		}

		/// <summary>
		/// Quadratic interpolation between 3 points
		/// </summary>
		/// <param name="tValue">0-1 percentage value.</param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <returns>the result of the interpolation</returns>
		public Vector3 GetQuadratic(float tValue, Vector3 point1, Vector3 point2, Vector3 point3)
		{
			return new Vector3(Mathf.Pow((1 - tValue), 2) * point1.x + 2 * (1 - tValue) * tValue * point2.x + Mathf.Pow(tValue, 2) * point3.x,
				Mathf.Pow((1 - tValue), 2) * point1.y + 2 * (1 - tValue) * tValue * point2.y + Mathf.Pow(tValue, 2) * point3.y,
				Mathf.Pow((1 - tValue), 2) * point1.z + 2 * (1 - tValue) * tValue * point2.z + Mathf.Pow(tValue, 2) * point3.z);
		}

		/// <summary>
		/// Cubic interpolation between 4 points
		/// </summary>
		/// <param name="tValue">0-1 percentage value.</param>
		/// <param name="point1"></param>
		/// <param name="point2"></param>
		/// <param name="point3"></param>
		/// <param name="point4"></param>
		/// <returns>the result of the interpolation</returns>
		public Vector3 GetCubic(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
		{
			float u = 1 - tValue;
			float tt = tValue * tValue;
			float uu = u * u;
			float uuu = uu * u;
			float ttt = tt * tValue;

			Vector3 p = uuu * point1; //first term

			p += 3 * uu * tValue * point2; //second term
			p += 3 * u * tt * point3; //third term
			p += ttt * point4; //fourth term

			return p;
		}

        public Vector3 GetCubicTangent(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            float omt = 1f - tValue;
            float omt2 = omt * omt;
            float t2 = tValue * tValue;

            Vector3 tangent =
                point1 * (-omt2) +
                point2 * (3 * omt2 - 2 * omt) +
                point3 * (-3 * t2 + 2 * tValue) +
                point4 * (t2);

            return tangent.normalized;
        }

        public Vector3 GetCubicNormal(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4,
            Vector3 up)
        {
            Vector3 tangent = GetCubicTangent(tValue, point1, point2, point3, point4);
            Vector3 binormal = Vector3.Cross(up, tangent);

            return Vector3.Cross(tangent, binormal);
        }
		#endregion

		private List<Vector3> segList;
		private void FillSegBuffer(SplineKnot segBegin, SplineKnot segEnd)
		{
			if (segList == null) segList = new List<Vector3>();
			segList.Clear();

			segList.Add(segBegin.transform.position);
			if (segBegin.Type != SplineKnot.KnotType.Corner) segList.Add(segBegin.transform.TransformPoint(segBegin.LocalB));

			if(segEnd.Type != SplineKnot.KnotType.Corner) segList.Add(segEnd.transform.TransformPoint(segEnd.LocalA));
			segList.Add(segEnd.transform.position);
		}

        private void GetKnotsForSeg(int segIndx, out SplineKnot segBegin, out SplineKnot segEnd)
        {
            if (closeShape && (segIndx == knots.Count - 1))
            {
                segBegin = knots[segIndx];
                segEnd = knots[0];
            }
            else
            {
                segBegin = knots[segIndx];
                segEnd = knots[segIndx + 1];
            }
        }

		private void RenderVerts()
		{
			vertexList.Clear();

			for (int segIndx = 0; segIndx < SegmentCount; segIndx++)
			{
                SplineKnot segBegin, segEnd;

                GetKnotsForSeg(segIndx, out segBegin, out segEnd);

                FillSegBuffer(segBegin, segEnd);

				for (int v = 0; v < LineDetail; v++)
				{
					float t1 = Mathf.InverseLerp(0, LineDetail, v);

					if (segList.Count == 4)
					{
						vertexList.Add(GetCubic(t1, segList[0], segList[1], segList[2], segList[3]));
					}
					else if (segList.Count == 3)
					{
						vertexList.Add(GetQuadratic(t1, segList[0], segList[1], segList[2]));
					}
					else if (segList.Count == 2)
					{
						vertexList.Add(GetLinear(t1, segList[0], segList[1]));
					}
				}
			} // end segment loop
		}

		void OnDrawGizmos()
		{
			RenderVerts();

			Gizmos.color = Color.grey;
			for(int i=0; i < vertexList.Count - 1; i++)
			{
				Gizmos.DrawLine(vertexList[i], vertexList[i + 1]);
			}

			if(closeShape)
			{
				Gizmos.DrawLine(vertexList[vertexList.Count - 1], vertexList[0]);
			}
		}
	}
}
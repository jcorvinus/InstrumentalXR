using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling.ProceduralGraphics
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class ButtonModel : MonoBehaviour
	{
		public delegate void ModelPropertiesHandler(ButtonModel sender);
		public event ModelPropertiesHandler PropertiesChanged;

		[Range(3, 12)]
		[SerializeField] int cornerVertCount = 4;

		[SerializeField] int widthVertCount = 4;

		[Range(0,3)]
		[SerializeField] int bevelSliceCount = 1;

		[Range(0, 1)]
		[SerializeField]
		float extrusionDepth;

		[Range(0, 1)]
		[SerializeField]
		float radius;

		bool closeLoop = true;

		[Range(0,0.1f)]
		[SerializeField]
		float width;

		[Range(0, 1)]
		[SerializeField]
		float bevelRadius;

		[Range(0, 1)]
		[SerializeField]
		float bevelExtrusionDepth;

		Mesh _faceMesh;

		EdgeLoop backLoop;
		EdgeLoop frontLoop;
		EdgeBridge backFrontBridge;

		EdgeLoop[] faceBevelLoops;
		EdgeBridge[] faceBevelBridges;
		Vector3[] vertices;
		int[] triangles;

		int EdgeLoopVertCount { get { return (cornerVertCount * 4) + (widthVertCount * 2); } }

		public Mesh FaceMesh { get { return _faceMesh; } }

		// debug stuff
		[Header("Debug Variables")]
		[SerializeField] bool drawLoops;
		[SerializeField] bool drawMesh;
		[SerializeField] bool regenerate;

		// Use this for initialization
		void Start()
		{
			GenerateMesh();
		}

		private void OnValidate()
		{
			GenerateMesh();
			SetPropertiesChanged();
		}

		private void OnEnable()
		{
			GenerateMesh();
		}

		private void SetPropertiesChanged()
		{
			if (PropertiesChanged != null)
			{
				PropertiesChanged(this);
			}
		}

		void GenerateMesh()
		{
			if (_faceMesh == null) _faceMesh = new Mesh();
			_faceMesh.MarkDynamic();

			int bridgeCount = 0;

			int baseID = 0;
			backLoop = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
				EdgeLoopVertCount);

			frontLoop = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
				EdgeLoopVertCount);

			faceBevelLoops = new EdgeLoop[bevelSliceCount];
			for(int i=0; i < bevelSliceCount; i++)
			{
				faceBevelLoops[i] = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
					EdgeLoopVertCount);
				bridgeCount++;
			}

			vertices = new Vector3[backLoop.VertCount + frontLoop.VertCount + (EdgeLoopVertCount * bevelSliceCount)];

			SetVertices();

			int triangleBaseID = 0;
			backFrontBridge = ModelUtils.CreateExtrustion(ref triangleBaseID,
				frontLoop, backLoop);
			bridgeCount++;

			triangles = new int[backFrontBridge.GetTriangleIndexCount() * bridgeCount];
			backFrontBridge.TriangulateBridge(ref triangles, false);

			// do face bevel bridges
			faceBevelBridges = new EdgeBridge[bridgeCount];

			EdgeLoop firstLoop = frontLoop;
			for(int i=0; i < bevelSliceCount; i++)
			{
				EdgeLoop secondLoop = faceBevelLoops[i];

				faceBevelBridges[i] = ModelUtils.CreateExtrustion(ref triangleBaseID, firstLoop, secondLoop);
				faceBevelBridges[i].TriangulateBridge(ref triangles, true);

				firstLoop = secondLoop;
			}

			// check to see if we're using our last bevel loop/bridge properly

			// do a loop fill on the last loop to fill our face in.

			/*_faceMesh.vertices = vertices;
			_faceMesh.SetTriangles(triangles, 0);
			_faceMesh.RecalculateNormals();*/
		}

		void SetVertices()
		{
			float angleIncrement = 360 / EdgeLoopVertCount;
			float iterator = 0;

			for (int index = frontLoop.VertexBaseID; index < (frontLoop.VertexBaseID + frontLoop.VertCount);
				index++)
			{
				float angle = angleIncrement * iterator;

				Vector3 vertex = Vector3.up * radius;
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertices[index] = vertex;

				iterator++;
			}

			iterator = 0;
			for (int index = backLoop.VertexBaseID; index < (backLoop.VertexBaseID + frontLoop.VertCount);
				index++)
			{
				float angle = angleIncrement * iterator;

				Vector3 vertex = Vector3.up * radius;
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += (Vector3.forward * extrusionDepth);
				vertices[index] = vertex;

				iterator++;
			}

			// do our face bevel verts
			float extraExtrudeDepth = extrusionDepth * bevelExtrusionDepth;
			float totalExtrudeDepth = extraExtrudeDepth;
			float innerRadius = radius * bevelRadius;
			for(int i=0; i < bevelSliceCount; i++)
			{
				float depthTValue = ((float)i + 1) / (float)bevelSliceCount; // precision loss is happening here.
				float tValue = (float)i / (float)bevelSliceCount;
				int startIndex = faceBevelLoops[i].VertexBaseID;
				int endIndex = startIndex + faceBevelLoops[i].VertCount;

				float sliceRadius = MathSupplement.Sinerp(innerRadius, radius, 1 - depthTValue);
				float sliceDepth = (i == bevelSliceCount -1) ? Mathf.Lerp(extrusionDepth, totalExtrudeDepth, ((1 - tValue) +  (1 - depthTValue)) * 0.5f) : Mathf.Lerp(extrusionDepth, totalExtrudeDepth, 1 - depthTValue);

				iterator = 0;
				for (int index = startIndex; index < endIndex; index++)
				{
					float angle = angleIncrement * iterator;

					Vector3 vertex = Vector3.up * sliceRadius;
					vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
					vertex -= (Vector3.forward * sliceDepth);

					vertices[index] = vertex;

					iterator++;
				}
			}
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void OnDrawGizmosSelected()
		{
			if(regenerate)
			{
				regenerate = false;
				GenerateMesh();
			}

			if (drawLoops)
			{
				Gizmos.color = Color.blue;
				ModelUtils.DrawEdgeLoopGizmo(vertices, frontLoop);

				Gizmos.color = Color.yellow;
				ModelUtils.DrawEdgeLoopGizmo(vertices, backLoop);

				Gizmos.color = Color.green;
				for(int i=0; i < faceBevelLoops.Length; i++)
				{
					ModelUtils.DrawEdgeLoopGizmo(vertices, faceBevelLoops[i]);
				}
			}

			if (drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(vertices, triangles);
			}
		}
	}
}
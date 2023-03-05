using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class ExtrusionTest : MonoBehaviour
	{
		int EdgeLoopVertCount { get { return (cornerVertCount * 4) + (widthVertCount * 2); } }

		[Range(0, 1)]
		[SerializeField]
		float extrusionDepth;

		[Range(0,1)]
		[SerializeField]
		float radius;

		[SerializeField]
		bool closeLoop;

		[Range(2, 8)]
		[SerializeField] int cornerVertCount = 4;
		[Range(0, 8)]
		[SerializeField] int widthVertCount = 4;
		[Range(0, 0.1f)]
		[SerializeField]
		float width;

		MeshFilter meshFilter;
		MeshRenderer meshRenderer;
		Mesh _mesh;

		EdgeLoop backLoop;
		EdgeLoop frontLoop;
		EdgeBridge backFrontBridge;
		Vector3[] vertices;
		int[] triangles;

		// debug stuff
		[Header("Debug Variables")]
		[SerializeField] bool drawLoops;
		[SerializeField] bool drawMesh;
		[SerializeField] int drawSegmentID = 0;

		private void Awake()
		{
			meshFilter = GetComponent<MeshFilter>();
			meshRenderer = GetComponent<MeshRenderer>();
		}

		// Use this for initialization
		void Start()
		{
			if (_mesh == null) _mesh = new Mesh();
			_mesh.MarkDynamic();

			meshFilter.mesh = _mesh;
		}

		private void OnValidate()
		{
			GenerateMesh();
		}

		private void OnEnable()
		{
			GenerateMesh();
		}

		void GenerateMesh()
		{
			int baseID = 0;
			backLoop = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
				EdgeLoopVertCount);

			frontLoop = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
				EdgeLoopVertCount);

			vertices = new Vector3[backLoop.VertCount + frontLoop.VertCount];

			SetVertices();

			int triangleBaseID = 0;
			backFrontBridge = ModelUtils.CreateExtrustion(ref triangleBaseID,
				frontLoop, backLoop);

			triangles = new int[backFrontBridge.GetTriangleIndexCount()];
			backFrontBridge.TriangulateBridge(ref triangles, false);
		}

		void LoopSide(int baseID, bool isLeft, float depth, float sideRadius)
		{
			float angleIncrement = 180f / (((float)cornerVertCount * 2f) - 1);

			for (int i = 0; i < cornerVertCount * 2; i++)
			{
				float angle = angleIncrement * i;

				//if (!evenNumber && i == middle) angle = 90f;

				Vector3 vertex = Vector3.up * ((isLeft) ? sideRadius : -sideRadius);
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += Vector3.right * (width * ((isLeft) ? -0.5f : 0.5f));
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void LoopEdge(int baseID, bool isBottom, float depth, float sideRadius)
		{
			Vector3 leftEdge, rightEdge;
			leftEdge = Vector3.right * -(width * 0.5f); // we might also have to subtract the radius here? Not sure.
			rightEdge = Vector3.right * (width * 0.5f);

			leftEdge += Vector3.up * ((isBottom) ? -sideRadius : sideRadius);
			rightEdge += Vector3.up * ((isBottom) ? -sideRadius : sideRadius);
			leftEdge += Vector3.forward * depth;
			rightEdge += Vector3.forward * depth;

			Vector3 startEdge = (isBottom) ? leftEdge : rightEdge;
			Vector3 endEdge = (isBottom) ? rightEdge : leftEdge;

			for (int i = 0; i < widthVertCount; i++)
			{
				vertices[baseID + i] = Vector3.Lerp(startEdge, endEdge, (float)i / widthVertCount);
			}
		}

		void SetVertices()
		{
			LoopSide(backLoop.VertexBaseID, true, 0, radius);
			LoopEdge(backLoop.VertexBaseID + cornerVertCount * 2, true, 0, radius);
			LoopSide((backLoop.VertexBaseID + cornerVertCount * 2) + widthVertCount, false, 0, radius);
			LoopEdge((backLoop.VertexBaseID + cornerVertCount * 4) + widthVertCount, false, 0, radius);

			LoopSide(frontLoop.VertexBaseID, true, extrusionDepth, radius);
			LoopEdge(frontLoop.VertexBaseID + cornerVertCount * 2, true, extrusionDepth, radius);
			LoopSide((frontLoop.VertexBaseID + cornerVertCount * 2) + widthVertCount, false, extrusionDepth, radius);
			LoopEdge((frontLoop.VertexBaseID + cornerVertCount * 4) + widthVertCount, false, extrusionDepth, radius);
		}

		// Update is called once per frame
		void Update()
		{

		}

		void GetVertsForSegment(int segmentCount, int currentSegment, out int first, out int second)
		{
			first = currentSegment;
			second = (segmentCount - 1 == currentSegment) ? 0 : currentSegment + 1;
		}

		void DrawLoopWithSegment(EdgeLoop loop)
		{
			int segmentCount = loop.GetSegmentCount();

			// need to find the index of our bisection plane
			// first one is corner count, second one is cornercount * 3 + width count

			int bisectFirst = cornerVertCount;
			int bisectSecond = (cornerVertCount * 3) + widthVertCount;

			int bufferBisectFirst = loop.VertexBaseID + bisectFirst;
			int bufferBisectSecond = loop.VertexBaseID + bisectSecond;

			Gizmos.color = Color.red;
			Gizmos.DrawLine(vertices[bufferBisectFirst], vertices[bufferBisectSecond]);

			int nextBisect = 0;
			nextBisect = (drawSegmentID < bisectFirst) ? bisectFirst : bisectSecond;
			// get our distance to the bisect. Subtraction
			// then, apply that as an offset. Use mathf.repeat
			int offset = nextBisect - (drawSegmentID + 2);
			int adjacentSegment = nextBisect + offset;

			// ok so I think we can figure out if we've been processed already.
			// segment ID above first bisect, less than first bisect * 2?
			// segment ID above second bisect but below second bisect + bisectfirst?

			bool inFirstRange = (drawSegmentID > bisectFirst - 2) && (drawSegmentID < (bisectFirst * 2) - 1);
			bool inSecondRange = (drawSegmentID > (bisectFirst * 2) - 2 && (drawSegmentID < ((bisectFirst * 2) + bisectFirst)));
			bool hasProcessedAlready = inFirstRange || inSecondRange;

			for (int i = 0; i < segmentCount; i++)
			{
				int currentVert = 0;
				int nextVert = 0;

				GetVertsForSegment(segmentCount, i, out currentVert, out nextVert);

				int currentBufferID = loop.GetBufferIndexForVertIndex(currentVert);
				int nextBufferID = loop.GetBufferIndexForVertIndex(nextVert);

				Gizmos.color = (i == drawSegmentID) ? Color.blue : Color.green;
				if (i == adjacentSegment) Gizmos.color = Color.yellow;
				if (i == drawSegmentID && hasProcessedAlready) Gizmos.color = Color.red;
				Gizmos.DrawLine(vertices[currentBufferID], vertices[nextBufferID]);
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(drawLoops)
			{
				/*Gizmos.color = Color.blue;
				ModelUtils.DrawEdgeLoopGizmo(vertices, frontLoop);

				Gizmos.color = Color.yellow;
				ModelUtils.DrawEdgeLoopGizmo(vertices, backLoop);*/

				DrawLoopWithSegment(frontLoop);
			}

			if(drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(vertices, triangles);
			}
		}
	}
}
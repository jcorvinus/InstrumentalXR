using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling
{
	/// <summary>Describes a sequence of edges.</summary>
	[System.Serializable] // disable when not debugging.
	public struct EdgeLoop
	{
		/// <summary>Indicates how many vertices are in this loop.</summary>
		public int VertCount;
		/// <summary>This is the position in the vertex buffer where the edge loop starts.</summary>
		public int VertexBaseID;

		/// <summary>If TRUE, the first and last vertices in the loop
		/// connect to each other. Otherwise, they don't.</summary>
		public bool IsClosed;

		public int GetBufferIndexForVertIndex(int index)
		{
			return index + VertexBaseID;
		}

		public int GetSegmentCount()
		{
			return ((VertCount - 1) + ((IsClosed) ? 1 : 0));
		}

		public void GetVertsForSegment(int segment, out int first, out int second)
		{
			int segmentCount = GetSegmentCount();
			first = segment;
			second = (segmentCount - 1 == segment) ? 0 : segment + 1;
		}
	}

	/// <summary>Describes a series of triangles that connect two edge loops.</summary>
	[System.Serializable]  // disable when not debugging.
	public struct EdgeBridge
	{
		public EdgeLoop LoopA;
		public EdgeLoop LoopB;

		public int TriangleBaseID;

		public bool IsValid()
		{
			return LoopA.GetSegmentCount() == LoopB.GetSegmentCount();
		}

		public int GetSegmentCount()
		{
			return LoopA.GetSegmentCount();
		}

		public int GetTriangleCount()
		{
			return GetSegmentCount() * 2;
		}

		public int GetTriangleIndexCount()
		{
			return GetTriangleCount() * 3;
		}

		public void TriangulateBridge(ref int[] triangles, bool flip)
		{
			int segmentCount = GetSegmentCount();

			int trackID = TriangleBaseID;

			// start at base ID, loop through segments.
			// each segment gets 2 triangles, or 6 indeces
			for(int i=0; i < segmentCount; i++)
			{
				int currentVert = i;
				int nextVert = (segmentCount - 1 == i) ? 0 : i + 1;

				int triA0 = LoopA.GetBufferIndexForVertIndex(nextVert);
				int triA1 = LoopB.GetBufferIndexForVertIndex(currentVert);
				int triA2 = LoopA.GetBufferIndexForVertIndex(currentVert);

				int triB0 = LoopB.GetBufferIndexForVertIndex(currentVert);
				int triB1 = LoopA.GetBufferIndexForVertIndex(nextVert);
				int triB2 = LoopB.GetBufferIndexForVertIndex(nextVert);

				triangles[trackID] = (flip) ? triA2 : triA0;
				triangles[trackID + 1] = triA1;
				triangles[trackID + 2] = (flip) ? triA0 : triA2;

				triangles[trackID + 3] = (flip) ? triB2 : triB0;
				triangles[trackID + 4] = triB1;
				triangles[trackID + 5] = (flip) ? triB0 : triB2;

				trackID += 6;
			}
		}
	}

	public static class ModelUtils
	{
		// creation
		public static EdgeLoop CreateEdgeLoop(ref int baseID, bool isClosed, int segmentCount)
		{
			EdgeLoop newLoop = new EdgeLoop()
			{
				IsClosed = true,
				VertCount = segmentCount,
				VertexBaseID = baseID
			};

			baseID += segmentCount;

			return newLoop;
		}

		public static EdgeBridge CreateExtrustion(ref int baseID, EdgeLoop loopA, EdgeLoop loopB)
		{
			EdgeBridge bridge = new EdgeBridge()
			{
				LoopA = loopA,
				LoopB = loopB,
				TriangleBaseID = baseID
			};

			baseID += bridge.GetTriangleIndexCount();

			return bridge;
		}

		// visualization
		public static void DrawEdgeLoopGizmo(Vector3[] verts, EdgeLoop loop)
		{
			for (int i = 0; i < loop.VertCount; i++)
			{
				int currentIndex = loop.VertexBaseID + i;
				int nextIndex = currentIndex + 1;

				if (loop.IsClosed && i == (loop.VertCount - 1))
				{
					nextIndex = loop.VertexBaseID;
				}

				Vector3 currentVert, nextVert;
				currentVert = verts[currentIndex];
				nextVert = verts[nextIndex];

				Gizmos.DrawLine(currentVert, nextVert);
			}
		}

		public static void DrawMesh(Vector3[] verts, int[] triangles)
		{
			int triangleCount = triangles.Length / 3;

			for (int triangleIndex = 0; triangleIndex < triangleCount; triangleIndex++)
			{
				int triangleBaseIndex = triangleIndex * 3;
				int a, b, c;

				a = triangles[triangleBaseIndex];
				b = triangles[triangleBaseIndex + 1];
				c = triangles[triangleBaseIndex + 2];

				bool didFail = false;

				try
				{
					Gizmos.DrawLine(verts[a], verts[b]);
					Gizmos.DrawLine(verts[b], verts[c]);
					Gizmos.DrawLine(verts[c], verts[a]);
				}
				catch (System.IndexOutOfRangeException e)
				{
					Debug.Log("IOOR in triangle at base index: " + triangleBaseIndex);
					didFail = true;
				}

				if (didFail) break;
			}
		}
	}
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[ExecuteInEditMode]
	public class ExtrusionTest : MonoBehaviour
	{
		[Range(3, 12)]
		[SerializeField] int edgeLoopVertCount;

		[Range(0, 1)]
		[SerializeField]
		float extrusionDepth;

		[Range(0,1)]
		[SerializeField]
		float radius;

		[SerializeField]
		bool closeLoop;

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
				edgeLoopVertCount);

			frontLoop = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
				edgeLoopVertCount);

			vertices = new Vector3[backLoop.VertCount + frontLoop.VertCount];

			SetVertices();

			int triangleBaseID = 0;
			backFrontBridge = ModelUtils.CreateExtrustion(ref triangleBaseID,
				frontLoop, backLoop);

			triangles = new int[backFrontBridge.GetTriangleIndexCount()];
			backFrontBridge.TriangulateBridge(ref triangles, false);
		}

		void SetVertices()
		{
			float angleIncrement = 360 / edgeLoopVertCount;
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
		}

		// Update is called once per frame
		void Update()
		{

		}

		private void OnDrawGizmosSelected()
		{
			if(drawLoops)
			{
				Gizmos.color = Color.blue;
				ModelUtils.DrawEdgeLoopGizmo(vertices, frontLoop);

				Gizmos.color = Color.yellow;
				ModelUtils.DrawEdgeLoopGizmo(vertices, backLoop);
			}

			if(drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(vertices, triangles);
			}
		}
	}
}
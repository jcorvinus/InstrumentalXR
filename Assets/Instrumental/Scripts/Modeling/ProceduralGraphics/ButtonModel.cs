using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Schema;

namespace Instrumental.Modeling.ProceduralGraphics
{
	[ExecuteInEditMode]
	public class ButtonModel : MonoBehaviour
	{
		public delegate void ModelPropertiesHandler(ButtonModel sender);
		public event ModelPropertiesHandler PropertiesChanged;

		[SerializeField] bool hasRim = true;

		[Range(ButtonSchema.MIN_CORNER_VERT_COUNT, ButtonSchema.MAX_CORNER_VERT_COUNT)]
		[SerializeField] int cornerVertCount = 4;

		[Range(ButtonSchema.MIN_WIDTH_VERT_COUNT, ButtonSchema.MAX_WIDTH_VERT_COUNT)]
		[SerializeField] int widthVertCount = 4;

		[Range(ButtonSchema.MIN_BEVEL_SLICE_COUNT, ButtonSchema.MAX_BEVEL_SLICE_COUNT)]
		[SerializeField] int bevelSliceCount = 1;

		[Range(ButtonSchema.MIN_EXTRUSION_DEPTH, ButtonSchema.MAX_EXTRUSION_DEPTH)]
		[SerializeField]
		float extrusionDepth;

		[Range(ButtonSchema.MIN_RADIUS, ButtonSchema.MAX_RADIUS)]
		[SerializeField]
		float radius;

		const bool closeLoop = true;

		[Range(0, 0.1f)]
		[SerializeField]
		float width;

		[Range(ButtonSchema.MIN_BEVEL_RADIUS_PERCENT, ButtonSchema.MAX_BEVEL_RADIUS_PERCENT)]
		[SerializeField]
		float bevelRadius;

		[Range(ButtonSchema.MIN_BEVEL_EXTRUSION_PERCENT, ButtonSchema.MAX_BEVEL_EXTRUSION_PERCENT)]
		[SerializeField]
		float bevelExtrusionDepth;

		[Range(ButtonSchema.MIN_RIM_WIDTH_PERCENT, ButtonSchema.MAX_RIM_WIDTH_PERCENT)]
		[SerializeField]
		float rimWidthPercentage = 1;

		[Range(ButtonSchema.MIN_RIM_DEPTH_PERCENT, ButtonSchema.MAX_RIM_DEPTH_PERCENT)]
		[SerializeField]
		float rimDepthPercentage = 0.5f;

		public bool HasRim { set { HasRim = value; SetPropertiesChanged(); } }
		public int CornerVertCount { set { cornerVertCount = Mathf.Clamp(value, ButtonSchema.MIN_CORNER_VERT_COUNT, ButtonSchema.MAX_CORNER_VERT_COUNT); SetPropertiesChanged(); } }
		public int WidthVertCount { set { widthVertCount = Mathf.Max(0, value); SetPropertiesChanged(); } }
		public int BevelSliceCount { set { bevelSliceCount = Mathf.Clamp(value, ButtonSchema.MIN_BEVEL_SLICE_COUNT, ButtonSchema.MAX_BEVEL_SLICE_COUNT); SetPropertiesChanged(); } }

		public float Depth { set { extrusionDepth = Mathf.Clamp(value, ButtonSchema.MIN_EXTRUSION_DEPTH, ButtonSchema.MAX_EXTRUSION_DEPTH); SetPropertiesChanged(); } }

		public float Radius { set { radius = Mathf.Clamp(value, ButtonSchema.MIN_RADIUS, ButtonSchema.MAX_RADIUS); SetPropertiesChanged(); } }

		public float Width { set { width = value; SetPropertiesChanged(); } }

		public float BevelRadius { set { bevelRadius = Mathf.Clamp(value, ButtonSchema.MIN_BEVEL_RADIUS_PERCENT, ButtonSchema.MAX_BEVEL_RADIUS_PERCENT); SetPropertiesChanged(); } }

		public float BevelDepth { set { bevelExtrusionDepth = Mathf.Clamp(value, ButtonSchema.MIN_BEVEL_EXTRUSION_PERCENT, ButtonSchema.MAX_BEVEL_EXTRUSION_PERCENT); SetPropertiesChanged(); } }
		public float RimWidth { set { rimWidthPercentage = Mathf.Clamp01(value); SetPropertiesChanged(); } }

		public float RimDepth { set { rimDepthPercentage = Mathf.Clamp01(value); SetPropertiesChanged(); } }


		#region Face Mesh Stuff
		[Header("Face Color")]
		[SerializeField]
		ColorType faceColorType = ColorType.FlatColor;
		[SerializeField]
		Gradient faceGradient;
		[SerializeField]
		GradientInfo faceGradientInfo;
		[SerializeField]
		Color faceColor = Color.white;

		Mesh _faceMesh;
		EdgeLoop backLoop;
		EdgeLoop frontLoop;
		EdgeBridge backFrontBridge;
		LinearEdgeLoopFaceFill faceFill;

		EdgeLoop[] faceBevelLoops;
		EdgeBridge[] faceBevelBridges;
		Vector3[] faceVertices;
		int[] faceTriangles;
		Color[] faceColors;

		public Mesh FaceMesh { get { return _faceMesh; } }
		#endregion

		#region Rim Mesh Stuff
		[Header("Rim Color")]
		[SerializeField]
		ColorType rimColorType = ColorType.FlatColor;
		[SerializeField]
		Gradient rimGradient;
		[SerializeField]
		GradientInfo rimGradientInfo;
		[SerializeField]
		Color rimColor = Color.white;

		Mesh _rimMesh;
		EdgeLoop rimOuterLoopBack, rimOuterLoopFront;
		EdgeLoop rimInnerLoopBack, rimInnerLoopFront;
		EdgeBridge rimOuterBridge;
		EdgeBridge rimOuterInnerBridge;
		EdgeBridge rimInnerBridge;
		Vector3[] rimVertices;
		int[] rimTriangles;
		Color[] rimColors;

		public Mesh RimMesh { get { return _rimMesh; } }
		#endregion

		int EdgeLoopVertCount { get { return (cornerVertCount * 4) + (widthVertCount * 2); } }

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

		#region Mesh Generation
		void GenerateFaceMesh()
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
			for (int i = 0; i < bevelSliceCount; i++)
			{
				faceBevelLoops[i] = ModelUtils.CreateEdgeLoop(ref baseID, closeLoop,
					EdgeLoopVertCount);
				bridgeCount++;
			}

			faceVertices = new Vector3[backLoop.VertCount + frontLoop.VertCount + (EdgeLoopVertCount * bevelSliceCount)];

			SetFaceVertices();

			GenerateFaceColors(out faceColors);

			int triangleBaseID = 0;
			backFrontBridge = ModelUtils.CreateExtrustion(ref triangleBaseID,
				frontLoop, backLoop);
			bridgeCount++;

			int bridgeTriangleIndexCount = backFrontBridge.GetTriangleIndexCount() * bridgeCount;

			// do face bevel bridges
			faceBevelBridges = new EdgeBridge[bridgeCount];
			EdgeLoop firstLoop = frontLoop;
			for (int i = 0; i < bevelSliceCount; i++)
			{
				EdgeLoop secondLoop = faceBevelLoops[i];
				faceBevelBridges[i] = ModelUtils.CreateExtrustion(ref triangleBaseID, firstLoop, secondLoop);
				firstLoop = secondLoop;
			}

			faceFill = ModelUtils.CreateLinearFaceFill(ref triangleBaseID, faceBevelLoops[bevelSliceCount - 1],
				cornerVertCount, widthVertCount);

			int faceFillTriangleIndexCount = faceFill.GetTriangleIndexCount();

			// triangulate everything
			faceTriangles = new int[bridgeTriangleIndexCount + faceFillTriangleIndexCount];
			backFrontBridge.TriangulateBridge(ref faceTriangles, true);
			for (int i = 0; i < bevelSliceCount; i++)
			{
				faceBevelBridges[i].TriangulateBridge(ref faceTriangles, false);
			}

			faceFill.TriangulateFace(ref faceTriangles, false);
			_faceMesh.vertices = faceVertices;
			_faceMesh.SetTriangles(faceTriangles, 0);
			_faceMesh.colors = faceColors;
			_faceMesh.RecalculateNormals();
		}

		void GenerateFaceColors(out Color[] vertexColors)
		{
			vertexColors = new Color[faceVertices.Length];

			for(int i=0; i < vertexColors.Length; i++)
			{
				switch (faceColorType)
				{
					case ColorType.FlatColor:
						vertexColors[i] = faceColor;
						break;
					case ColorType.Gradient:
						vertexColors[i] = faceColor; // todo: replace this with a radial gradient
						break;
					default:
						break;
				}
			}
		}


		void GenerateRimColors(out Color[] vertexColors)
		{
			vertexColors = new Color[rimVertices.Length];
			
			for(int i=0; i < vertexColors.Length; i++)
			{
				switch (faceColorType)
				{
					case ColorType.FlatColor:
						vertexColors[i] = rimColor;
						break;
					case ColorType.Gradient:
						vertexColors[i] = rimColor; // todo: replace this with a linear depth gradient
						break;
					default:
						break;
				}
			}
		}
		void GenerateRimMesh()
		{
			if (hasRim)
			{
				if (_rimMesh == null) _rimMesh = new Mesh();
				_rimMesh.MarkDynamic();

				int vertexBaseID = 0;
				rimOuterLoopBack = ModelUtils.CreateEdgeLoop(ref vertexBaseID, closeLoop,
					EdgeLoopVertCount);
				rimOuterLoopFront = ModelUtils.CreateEdgeLoop(ref vertexBaseID, closeLoop,
					EdgeLoopVertCount);

				rimInnerLoopBack = ModelUtils.CreateEdgeLoop(ref vertexBaseID, closeLoop,
					EdgeLoopVertCount);
				rimInnerLoopFront = ModelUtils.CreateEdgeLoop(ref vertexBaseID, closeLoop,
					EdgeLoopVertCount);

				rimVertices = new Vector3[rimOuterLoopBack.VertCount + rimOuterLoopFront.VertCount +
					rimInnerLoopBack.VertCount + rimInnerLoopFront.VertCount];

				int triangleBaseID = 0;
				rimOuterBridge = ModelUtils.CreateExtrustion(ref triangleBaseID, rimOuterLoopBack,
					rimOuterLoopFront);
				rimOuterInnerBridge = ModelUtils.CreateExtrustion(ref triangleBaseID, rimOuterLoopFront,
					rimInnerLoopFront);
				rimInnerBridge = ModelUtils.CreateExtrustion(ref triangleBaseID, rimInnerLoopBack,
					rimInnerLoopFront);

				SetRimVertices();
				GenerateRimColors(out rimColors);

				int triangleIndexCount = rimOuterBridge.GetTriangleIndexCount() +
					rimOuterInnerBridge.GetTriangleIndexCount() +
					rimInnerBridge.GetTriangleIndexCount();
				rimTriangles = new int[triangleIndexCount];

				// triangulate our extrusions
				rimOuterBridge.TriangulateBridge(ref rimTriangles, false);
				rimOuterInnerBridge.TriangulateBridge(ref rimTriangles, false);
				rimInnerBridge.TriangulateBridge(ref rimTriangles, false);
				_rimMesh.vertices = rimVertices;
				_rimMesh.triangles = rimTriangles;
				_rimMesh.colors = rimColors;
				_rimMesh.RecalculateNormals();
			}
			else
			{
				_rimMesh = null;
				rimVertices = new Vector3[0];
				rimTriangles = new int[0];
				rimColors = new Color[0];
			}
		}

		void GenerateMesh()
		{
			GenerateFaceMesh();
			GenerateRimMesh();
		}
		#endregion

		#region Vertex Setting
		void LoopSide(ref Vector3[] vertices,
			int baseID, bool isLeft, float depth, float sideRadius)
		{
			float angleIncrement = 180f / (((float)cornerVertCount * 2f) - 1);

			for (int i = 0; i < cornerVertCount * 2; i++)
			{
				float angle = angleIncrement * i;

				Vector3 vertex = Vector3.up * ((isLeft) ? sideRadius : -sideRadius);
				vertex = Quaternion.AngleAxis(angle, Vector3.forward) * vertex;
				vertex += Vector3.right * (width * ((isLeft) ? -0.5f : 0.5f));
				vertex += Vector3.forward * depth;
				vertices[baseID + i] = vertex;
			}
		}

		void LoopEdge(ref Vector3[] vertices, 
			int baseID, bool isBottom, float depth, float sideRadius)
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

			for (int i=0; i < widthVertCount; i++)
			{
				vertices[baseID + i] = Vector3.Lerp(startEdge, endEdge, (float)i / widthVertCount);
			}
		}

		void SetFaceVertices()
		{
			LoopSide(ref faceVertices, frontLoop.VertexBaseID, true, extrusionDepth, radius);
			LoopEdge(ref faceVertices, frontLoop.VertexBaseID + (cornerVertCount * 2), true, extrusionDepth, radius);
			LoopSide(ref faceVertices, (frontLoop.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, extrusionDepth, radius);
			LoopEdge(ref faceVertices, (frontLoop.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, extrusionDepth, radius);

			LoopSide(ref faceVertices, backLoop.VertexBaseID, true, 0, radius);
			LoopEdge(ref faceVertices, backLoop.VertexBaseID + (cornerVertCount * 2), true, 0, radius);
			LoopSide(ref faceVertices, (backLoop.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, 0, radius);
			LoopEdge(ref faceVertices, (backLoop.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, 0, radius);

			// do our face bevel verts
			float extraExtrudeDepth = extrusionDepth * bevelExtrusionDepth;
			float totalExtrudeDepth = extrusionDepth + extraExtrudeDepth;
			float innerRadius = radius * bevelRadius;
			for(int i=0; i < bevelSliceCount; i++)
			{
				float depthTValue = ((float)i + 1) / (float)bevelSliceCount; // precision loss is happening here.
				float tValue = (float)i / (float)bevelSliceCount;
				int startIndex = faceBevelLoops[i].VertexBaseID;

				float sliceRadius = MathSupplement.Sinerp(innerRadius, radius, 1 - depthTValue);
				float sliceDepth = (i == bevelSliceCount -1) ? Mathf.Lerp(extrusionDepth, totalExtrudeDepth, ((tValue) +  (depthTValue)) * 0.5f) : Mathf.Lerp(extrusionDepth, totalExtrudeDepth, depthTValue);

				LoopSide(ref faceVertices, startIndex, true, sliceDepth, sliceRadius);
				LoopEdge(ref faceVertices, startIndex + (cornerVertCount * 2), true, sliceDepth, sliceRadius);
				LoopSide(ref faceVertices, (startIndex + (cornerVertCount * 2)) + widthVertCount, false, sliceDepth, sliceRadius);
				LoopEdge(ref faceVertices, (startIndex + (cornerVertCount * 4)) + widthVertCount, false, sliceDepth, sliceRadius);
			}
		}

		void SetRimVertices()
		{
			float rimOuterRadius = radius + (radius * rimWidthPercentage);
			float rimFrontDepth = (extrusionDepth * 0.5f);
			LoopSide(ref rimVertices, rimOuterLoopBack.VertexBaseID, true, 0, rimOuterRadius);
			LoopEdge(ref rimVertices, rimOuterLoopBack.VertexBaseID + (cornerVertCount * 2), true, 0, rimOuterRadius);
			LoopSide(ref rimVertices, (rimOuterLoopBack.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, 0, rimOuterRadius);
			LoopEdge(ref rimVertices, (rimOuterLoopBack.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, 0, rimOuterRadius);

			LoopSide(ref rimVertices, rimOuterLoopFront.VertexBaseID, true, rimFrontDepth, rimOuterRadius);
			LoopEdge(ref rimVertices, rimOuterLoopFront.VertexBaseID + (cornerVertCount * 2), true, rimFrontDepth, rimOuterRadius);
			LoopSide(ref rimVertices, (rimOuterLoopFront.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, rimFrontDepth, rimOuterRadius);
			LoopEdge(ref rimVertices, (rimOuterLoopFront.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, rimFrontDepth, rimOuterRadius);

			LoopSide(ref rimVertices, rimInnerLoopFront.VertexBaseID, true, rimFrontDepth, radius);
			LoopEdge(ref rimVertices, rimInnerLoopFront.VertexBaseID + (cornerVertCount * 2), true, rimFrontDepth, radius);
			LoopSide(ref rimVertices, (rimInnerLoopFront.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, rimFrontDepth, radius);
			LoopEdge(ref rimVertices, (rimInnerLoopFront.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, rimFrontDepth, radius);

			LoopSide(ref rimVertices, rimInnerLoopBack.VertexBaseID, true, 0, radius);
			LoopEdge(ref rimVertices, rimInnerLoopBack.VertexBaseID + (cornerVertCount * 2), true, 0, radius);
			LoopSide(ref rimVertices, (rimInnerLoopBack.VertexBaseID + (cornerVertCount * 2)) + widthVertCount, false, 0, radius);
			LoopEdge(ref rimVertices, (rimInnerLoopBack.VertexBaseID + (cornerVertCount * 4)) + widthVertCount, false, 0, radius);
		}
		#endregion

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
				ModelUtils.DrawEdgeLoopGizmo(faceVertices, frontLoop);

				Gizmos.color = Color.yellow;
				ModelUtils.DrawEdgeLoopGizmo(faceVertices, backLoop);

				Gizmos.color = Color.green;
				for(int i=0; i < faceBevelLoops.Length; i++)
				{
					ModelUtils.DrawEdgeLoopGizmo(faceVertices, faceBevelLoops[i]);
				}
			}

			if (drawMesh)
			{
				Gizmos.color = Color.green;
				ModelUtils.DrawMesh(faceVertices, faceTriangles);

				if (hasRim)
				{
					Gizmos.color = Color.blue;
					ModelUtils.DrawMesh(rimVertices, rimTriangles);
				}
			}
		}
	}
}
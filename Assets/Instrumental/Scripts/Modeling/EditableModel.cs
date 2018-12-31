/*-----------------------------------------------------------
 * Lucidigital Geometry
 * By JCorvinus
 * Currently unlicenced for any use.
 * ----------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lucidigital.Modeling
{
	/// <summary>
	/// Lets us define verts, polys, matIDs and other things required to build a mesh.
	/// Yes, I'm insane enough to turn Unity into a 3d modeling tool.
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class EditableModel : MonoBehaviour
	{
		[System.Serializable]
		public struct FaceDefinition
		{
			public int A;
			public int B;
			public int C;
		}

		[SerializeField]
		private List<Vector3> verts = new List<Vector3>(); // you should do them in this order, specifically
		[SerializeField]
		private List<Vector3> normals = new List<Vector3>(); // unless you're up to major shenanigans, this should always be as long as verts.
		[SerializeField]
		private List<Vector2> uvs = new List<Vector2>();
		[SerializeField]
		private List<FaceDefinition> faces = new List<FaceDefinition>();

		public List<Vector3> Vertices { get { return verts; } }
		public List<Vector3> Normals { get { return normals; } }
		public List<Vector2> UVCoords { get { return uvs; } }
		public List<FaceDefinition> Faces { get { return faces; } /*set { faces = value; }*/ }

		[SerializeField]
		private Mesh mesh;
		public Mesh GeometryAsMesh { get { return mesh; } }

		private MeshRenderer meshRenderer;
		private MeshFilter meshFilter;

		// Use this for initialization
		void Start()
		{

		}

		/// <summary>
		/// If there's an existing mesh component in our model, turn it into an internal representaion we can use!
		/// </summary>
		private void LoadMesh()
		{
			verts = new List<Vector3>(mesh.vertices);
			if(mesh.normals != null) normals = new List<Vector3>(mesh.normals);
			if((mesh.triangles != null) && mesh.GetTopology(0) == MeshTopology.Triangles)
			{
				faces = new List<FaceDefinition>();

				for(int i=0; i < mesh.triangles.Length; i+=3)
				{
					faces.Add(new FaceDefinition() { A = mesh.triangles[i], B = mesh.triangles[i + 1], C = mesh.triangles[i + 2] });
				}
			}
		}

		[ExecuteInEditMode]
		void OnEnable()
		{
			if (mesh == null)
			{
				mesh = new Mesh();
			}

			GetRequiredComponents();

			if (meshFilter.mesh != null) mesh = meshFilter.sharedMesh;

			if (mesh.triangles.Length != faces.Count * 3) UpdateFaces();
		}

		private void GetRequiredComponents()
		{
			meshRenderer = GetComponent<MeshRenderer>();
			meshFilter = GetComponent<MeshFilter>();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void PullMeshFromFilter()
		{
			GetRequiredComponents();

			verts.Clear();
			normals.Clear();
			uvs.Clear();
			faces.Clear();

			mesh = meshFilter.sharedMesh;

			LoadMesh();
		}

		#region Vertex Methods
		private void UpdateVerts()
		{
			OnEnable();

			mesh.vertices = verts.ToArray();
			mesh.normals = normals.ToArray();

			meshFilter.sharedMesh = mesh;
		}

		public void SetVertex(Vector3 vertex, int index)
		{
			verts[index] = vertex;
		}

		public void AddNewVertex(Vector3 vertex = new Vector3())
		{
			verts.Add(vertex);
			normals.Add(new Vector3());

			UpdateVerts();
		}
		#endregion

		#region Face Methods
		public void AddNewFaces(FaceDefinition[] face)
		{
			faces.AddRange(face);
			UpdateFaces();
		}

		private void UpdateFaces()
		{
			GetRequiredComponents();
			mesh.Clear();

			mesh.vertices = verts.ToArray();
			mesh.normals = normals.ToArray();
			int[] faceList = new int[faces.Count * 3];

			for (int i = 0; i < faces.Count; i++)
			{
				FaceDefinition currentFace = faces[i];

				faceList[i * 3] = currentFace.A;
				faceList[(i * 3) + 1] = currentFace.B;
				faceList[(i * 3) + 2] = currentFace.C;
			}

			mesh.triangles = faceList;

			meshFilter.mesh = mesh;
		}

		public void FlipFace(int indx)
		{
			int[] faceIndx = new int[3] { faces[indx].A, faces[indx].B, faces[indx].C };
			faces[indx] = new FaceDefinition() { A = faceIndx[2], B = faceIndx[1], C = faceIndx[0] };
			UpdateFaces();
		}

		public bool PointInTriangle(Vector3 point, int triangleID)
		{
			Vector3 A, B, C;
			A = verts[faces[triangleID].A];
			B = verts[faces[triangleID].B];
			C = verts[faces[triangleID].C];

			return (FaceSameSide(point, A, B, C) && FaceSameSide(point, B, A, C) && FaceSameSide(point, C, A, B));
		}
		#endregion

		#region Static Methods
		private static bool FaceSameSide(Vector3 point1, Vector3 point2, Vector3 vertA, Vector3 vertB)
		{
			Vector3 crossProductp1 = Vector3.Cross(vertB - vertA, point1 - vertA);
			Vector3 crossProductp2 = Vector3.Cross(vertB - vertA, point2 - vertA);

			if (Vector3.Dot(crossProductp1, crossProductp2) >= 0) return true;
			else return false;
		}

        public static Mesh Quad()
        {
            float size = 1;
            Mesh mesh = new Mesh();
            mesh.name = "ProceduralQuad";
            mesh.vertices = new Vector3[] { new Vector3(-size, -size, 0.01f), new Vector3(size, -size, 0.01f), new Vector3(size, size, 0.01f), new Vector3(-size, size, 0.01f) };
            mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();

            return mesh;
        }
		#endregion
	}
}

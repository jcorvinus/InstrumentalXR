using UnityEngine;
using UnityEditor;
using System.Collections;
using Instrumental.Modeling;

/// <summary>
/// This class is an editor for our geometry objects. It allows simple model editing.
/// </summary>
[CustomEditor(typeof(EditableModel))]
public class ModelEditor : Editor
{
	public enum VertexToolMode { Select, Move, Align }
	public enum FaceToolMode { Select, Create }
	public enum EdgeToolMode { Select, Move }
	public enum EditorSubObjectMode { Vertex, Edge, Face }

	private EditorSubObjectMode oldEditorMode;
	private EditorSubObjectMode editorMode;
	private VertexToolMode vertexToolMode;
	private FaceToolMode faceToolMode;
	private EdgeToolMode edgeToolMode;
	private int selectedVertexID = -1;
	private int selectedFaceID = -1;
	private int selectedEdgeID;
	private EditableModel model;

	#region Vertex Variables
	public const float VertexTickSize = 0.0025f;
	private bool alignX = false;
	private bool alignY = false;
	private bool alignZ = false;
	#endregion

	#region Face Building Variables
	private bool FaceGenActive { get { return currentTempFaceIndx >= 0; } }
	private int currentTempFaceIndx = -1;
	private int[] tempFace;
	#endregion

	void OnEnable()
	{
		model = target as EditableModel;
		tempFace = new int[3] { -1, -1, -1 };
	}

	#region Tool Methods
	void DoVertexTools()
	{
		switch (vertexToolMode)
		{
			case VertexToolMode.Select:				
				for (int i=0; i < model.Vertices.Count; i++)
				{
					Vector3 vertex = model.Vertices[i];

					if (Handles.Button(model.transform.TransformPoint(vertex), Quaternion.identity, VertexTickSize, VertexTickSize, Handles.CubeHandleCap))
					{
						selectedVertexID = i;
						this.Repaint();
					}
				}
				break;

			case VertexToolMode.Move:
				for (int i = 0; i < model.Vertices.Count; i++)
				{
					Vector3 vertex = model.Vertices[i];

					Handles.Button(vertex, Quaternion.identity, VertexTickSize, VertexTickSize, Handles.CubeHandleCap);

					if (i == selectedVertexID)
					{
						model.SetVertex(Handles.PositionHandle(vertex, Quaternion.identity), i); // todo: possibly check against editor's gizmo space and adjust the rotation?
					}
				}
				break;

			case VertexToolMode.Align:
				for (int i = 0; i < model.Vertices.Count; i++)
				{
					Vector3 currentVertex = model.Vertices[i];
					Vector3 selectedVertex = model.Vertices[selectedVertexID];

					if (i == selectedVertexID)
					{
						Handles.Button(model.transform.TransformPoint(currentVertex), Quaternion.identity, VertexTickSize, VertexTickSize * 2, Handles.SphereHandleCap);
					}
					else
					{
						if (Handles.Button(model.transform.TransformPoint(currentVertex), Quaternion.identity, VertexTickSize, VertexTickSize, Handles.CubeHandleCap))
						{
							model.SetVertex(new Vector3((alignX) ? currentVertex.x : selectedVertex.x,
								(alignY) ? currentVertex.y : selectedVertex.y,
								(alignZ) ? currentVertex.z : selectedVertex.z), selectedVertexID);
						}
					}
				}
				break;
		}
	}

	void DoEdgeTools()
	{

	}

	void DoFaceTools()
	{
		// draw our selection polygon
		if (selectedFaceID >= 0)
		{
			try
			{
				EditableModel.FaceDefinition face = model.Faces[selectedFaceID];
				Handles.color = Color.blue;
				Handles.DrawLine(model.Vertices[face.A], model.Vertices[face.B]);
				Handles.DrawLine(model.Vertices[face.A], model.Vertices[face.C]);
				Handles.DrawLine(model.Vertices[face.B], model.Vertices[face.C]);
			}
			catch(System.ArgumentOutOfRangeException e)
			{
                Debug.LogError(e.Message);
			}
		}

		switch (faceToolMode)
		{
			case FaceToolMode.Select:
				Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); // we're trying to do hover picking here.
				float intersectDist;
				bool pointInFace = false;
				EditableModel.FaceDefinition face = new EditableModel.FaceDefinition();
				
				for (int i = 0; i < model.Faces.Count; i ++)
				{
					face = model.Faces[i];

					Plane facePlane = new Plane(model.Vertices[face.A], model.Vertices[face.B], model.Vertices[face.C]);
					Vector3 intersectPoint;
					if (facePlane.Raycast(mouseRay, out intersectDist))
					{
						intersectPoint = mouseRay.origin + (mouseRay.direction * intersectDist);
						
						// check point against the edges
						if(model.PointInTriangle(intersectPoint, i))
						{
							if (Event.current.type == EventType.MouseDown) selectedFaceID = i;
							pointInFace = true;
							break;
						}
					}
				}

				if (pointInFace)
				{
					Handles.color = Color.red;
					Handles.DrawLine(model.Vertices[face.A], model.Vertices[face.B]);
					Handles.DrawLine(model.Vertices[face.A], model.Vertices[face.C]);
					Handles.DrawLine(model.Vertices[face.B], model.Vertices[face.C]);
					SceneView.RepaintAll();
				}
				else SceneView.RepaintAll();
				break;

			case FaceToolMode.Create:
				for (int i = 0; i < model.Vertices.Count; i++)
				{
					if (Handles.Button(model.Vertices[i], Quaternion.identity, VertexTickSize, VertexTickSize, Handles.CubeHandleCap))
					{
						bool canAdd=true;
						for (int checkIndx = 0; checkIndx < 3; checkIndx++)
						{
							if (tempFace[checkIndx] == i)
							{
								canAdd = false;
								break;
							}
						}

						if (canAdd)
						{
							currentTempFaceIndx++;
							if (currentTempFaceIndx >= 2) // add new face
							{
								tempFace[currentTempFaceIndx] = i;
								model.AddNewFaces(new EditableModel.FaceDefinition[] { new EditableModel.FaceDefinition { A = tempFace[0], B = tempFace[1], C = tempFace[2] } });
								ResetFaceGen();
								Debug.Log("Made new face!");
							}
							else
							{
								tempFace[currentTempFaceIndx] = i;
								Debug.Log(string.Format("PickIndx: {0} | {1}", i, currentTempFaceIndx));
							}

							Repaint();
						}
					}
				}
				break;

			default:
				break;
		}
	}

	private void ResetFaceGen()
	{
		currentTempFaceIndx = -1;
		tempFace = new int[3] { -1, -1, -1 };
	}
	#endregion

	void OnSceneGUI()
	{
		switch (editorMode)
		{
			case EditorSubObjectMode.Vertex:
				DoVertexTools();
				break;

			case EditorSubObjectMode.Edge:
				DoEdgeTools();
				break;

			case EditorSubObjectMode.Face:
				DoFaceTools();
				break;

			default:
				break;
		}
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField("Selection Mode:");
		editorMode = (EditorSubObjectMode)EditorGUILayout.EnumPopup(editorMode);

		if (editorMode != oldEditorMode) ResetFaceGen();

		switch (editorMode)
		{
			case EditorSubObjectMode.Vertex:
				EditorGUILayout.LabelField("Tools:");
				vertexToolMode = (VertexToolMode)EditorGUILayout.EnumPopup(vertexToolMode);
				EditorGUILayout.LabelField(string.Format("Selected Vertex: {0}", (selectedVertexID >= 0) ? selectedVertexID.ToString() : "none"));
				if (GUILayout.Button("Clear Selection"))
				{
					selectedVertexID = -1;
					this.Repaint();
				}

				if ((vertexToolMode == VertexToolMode.Select) || (vertexToolMode == VertexToolMode.Move))
				{
					if (GUILayout.Button("Add Vertex"))
					{
						model.AddNewVertex();
						selectedVertexID = model.Vertices.Count - 1; // out-select our new bad-boy
						vertexToolMode = VertexToolMode.Move;
						Repaint();
						return;
					}
				}
				else if (vertexToolMode == VertexToolMode.Align)
				{
					alignX = EditorGUILayout.Toggle("Align X", alignX);
					alignY = EditorGUILayout.Toggle("Align Y", alignY);
					alignZ = EditorGUILayout.Toggle("Align Z", alignZ);
				}
				break;

			case EditorSubObjectMode.Edge:
				EditorGUILayout.LabelField("Tools:");
				edgeToolMode = (EdgeToolMode)EditorGUILayout.EnumPopup(edgeToolMode);
				break;

			case EditorSubObjectMode.Face:
				EditorGUILayout.LabelField("Tools:");
				faceToolMode = (FaceToolMode)EditorGUILayout.EnumPopup(faceToolMode);
				EditorGUILayout.LabelField(string.Format("Selected Face: {0}", (selectedFaceID >= 0) ? selectedFaceID.ToString() : "none"));
				if (GUILayout.Button("Clear Selection"))
				{
					selectedFaceID = -1;
					this.Repaint();
				}

				if(faceToolMode == FaceToolMode.Create)
				{
					EditorGUILayout.LabelField(string.Format("Face: {0}, {1}, {2}", tempFace[0], tempFace[1], tempFace[2]));
				}
				else if(faceToolMode == FaceToolMode.Select)
				{
					if(GUILayout.Button("Flip Face"))
					{
						model.FlipFace(selectedFaceID);
					}
				}
				break;

			default:
				break;
		}

		EditorGUILayout.LabelField("_________________");
		EditorGUILayout.LabelField("Stats:");
		EditorGUILayout.LabelField(string.Format("Verts: {0}", model.Vertices.Count));
		EditorGUILayout.LabelField(string.Format("Norms: {0}", model.Normals.Count));
		EditorGUILayout.LabelField(string.Format("Faces: {0}", model.Faces.Count));

		if(GUILayout.Button("Pull mesh from filter."))
		{
			
			model.PullMeshFromFilter();
		}

		oldEditorMode = editorMode;
	}
}

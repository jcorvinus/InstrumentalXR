using UnityEngine;
using UnityEditor;
using System.Collections;
using Instrumental.Modeling;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
	public static float HandleScale = 0.0125f;
	private Spline m_instance;
	private int selectedKnot=-1;

	void OnEnable()
	{
		m_instance = target as Spline;
		m_instance.GetKnots();
	}

	void OnSceneGUI()
	{
		if (m_instance.Knots == null) m_instance.GetKnots();
		if (m_instance.Knots == null) return;

		for (int i = 0; i < m_instance.Knots.Count; i++)
		{
			Handles.color = (i == selectedKnot) ? Color.red : Color.blue;

			if (Handles.Button(m_instance.Knots[i].transform.position, m_instance.transform.rotation, HandleScale, HandleScale, Handles.SphereCap))
			{
				SelectIndex(i);
				Repaint();
			}

			if (selectedKnot == i)
			{
				Gizmos.color = Color.green;
				#region Knot Editing
				m_instance.Knots[i].transform.position = Handles.PositionHandle(m_instance.Knots[selectedKnot].transform.position, m_instance.transform.rotation);

				if (m_instance.Knots[selectedKnot].Type != SplineKnot.KnotType.Corner)
				{
					m_instance.Knots[selectedKnot].LocalA = m_instance.Knots[selectedKnot].transform.InverseTransformPoint(Handles.PositionHandle(m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalA), m_instance.Knots[selectedKnot].transform.rotation));
					Handles.DrawLine(m_instance.Knots[selectedKnot].transform.position, m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalA));
					Handles.SphereCap(0, m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalA), Quaternion.identity, HandleScale);

					if (m_instance.Knots[selectedKnot].Type == SplineKnot.KnotType.Cubic)
					{
						Handles.SphereCap(0, m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalB), Quaternion.identity, HandleScale);
						Handles.DrawLine(m_instance.Knots[selectedKnot].transform.position, m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalB));
						m_instance.Knots[selectedKnot].LocalB = m_instance.Knots[selectedKnot].transform.InverseTransformPoint(Handles.PositionHandle(m_instance.Knots[selectedKnot].transform.TransformPoint(m_instance.Knots[selectedKnot].LocalB), m_instance.Knots[selectedKnot].transform.rotation));
					}
				}
				#endregion
			}
		}
	}

	private void SelectIndex(int indx)
	{
		selectedKnot = indx;
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.LabelField(string.Format("Selected Knot: {0}", (selectedKnot >= 0)? selectedKnot.ToString() : "None"));
		if(selectedKnot >= 0)
		{
			m_instance.Knots[selectedKnot].Type = (SplineKnot.KnotType)EditorGUILayout.EnumPopup(m_instance.Knots[selectedKnot].Type);
			m_instance.Knots[selectedKnot].LockHandles = GUILayout.Toggle(m_instance.Knots[selectedKnot].LockHandles, "Lock Handles");
			if(GUILayout.Button("Reset Control Points"))
			{
				float scaleDistance = 0;
				if (m_instance.Knots.Count >= 2)
				{
					if ((selectedKnot >= 0) && (selectedKnot < m_instance.Knots.Count - 1)) // Get the distance to or from the next dot, and use this to decide how far to put the handles
					{
						scaleDistance = Vector3.Distance(m_instance.Knots[selectedKnot].transform.position, m_instance.Knots[selectedKnot + 1].transform.position);
					}
					else
					{
						scaleDistance = Vector3.Distance(m_instance.Knots[m_instance.Knots.Count - 1].transform.position, m_instance.Knots[m_instance.Knots.Count - 2].transform.position);
					}
				}

				m_instance.Knots[selectedKnot].LocalA = Vector3.right * (scaleDistance * 0.25f);
				m_instance.Knots[selectedKnot].LocalB = Vector3.right * (-1 * (scaleDistance * 0.25f));
				Repaint();
			}

			if(GUILayout.Button("Delete Knot"))
			{
				Destroy(m_instance.Knots[selectedKnot]);
				m_instance.GetKnots();
				Repaint();
			}
		}

		if (GUILayout.Button("Add Knot"))
		{
			GameObject newKnot = new GameObject();
			newKnot.name = string.Format("Knot ({0})", m_instance.Knots.Count);
			newKnot.transform.SetParent(m_instance.transform);
			newKnot.transform.localScale = Vector3.one;
			newKnot.AddComponent<SplineKnot>();

			// get the direction from our previous two points
			newKnot.transform.position = m_instance.Knots[m_instance.Knots.Count - 1].transform.position - m_instance.Knots[m_instance.Knots.Count - 2].transform.position;

			m_instance.GetKnots();
			Repaint();
		}

		m_instance.CloseShape = GUILayout.Toggle(m_instance.CloseShape, "Close shape");

		EditorGUILayout.LabelField("_________________");

		EditorGUILayout.LabelField("Settings:");
		HandleScale = EditorGUILayout.FloatField("Handle Scale", HandleScale);
	}
}

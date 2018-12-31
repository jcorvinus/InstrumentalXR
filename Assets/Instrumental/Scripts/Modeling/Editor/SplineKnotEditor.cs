using UnityEngine;
using UnityEditor;
using System.Collections;
using Instrumental.Modeling;

[CustomEditor(typeof(SplineKnot))]
public class SplineKnotEditor : Editor
{
	SplineKnot m_instance;

	void OnEnable()
	{
		m_instance = target as SplineKnot;
	}

	public override void OnInspectorGUI()
	{
		GUILayout.Label(string.Format("Use the editor in the Spline object, don't mess\nwith the individual knots."));
	}
}

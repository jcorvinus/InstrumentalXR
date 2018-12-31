using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lucidigital.Modeling.ProceduralGraphics
{
    [CustomEditor(typeof(TubeGraphic))]
    public class TubeGraphicEditor : Editor
    {
        public static float HandleScale = 0.0125f;

        TubeGraphic m_instance;
        SerializedObject tubeGraphic;
        SerializedProperty pointsProperty;
        SerializedProperty drawMeshProperty;
        SerializedProperty drawIDTypeProperty;
        SerializedProperty selectedSegmentProperty;

        int selectedPoint = -1;

        private void OnEnable()
        {
            tubeGraphic = new SerializedObject(target);
            m_instance = target as TubeGraphic;
            pointsProperty = tubeGraphic.FindProperty("points");
            drawMeshProperty = tubeGraphic.FindProperty("drawMesh");
            drawIDTypeProperty = tubeGraphic.FindProperty("drawType");
            selectedSegmentProperty = tubeGraphic.FindProperty("selectedSegment");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.LabelField(string.Format("Selected Point: {0}", selectedPoint));

            // stupid and doesn't work for some reason
            //selectedSegmentProperty.intValue = EditorGUILayout.IntSlider(selectedSegmentProperty.intValue, 0, m_instance.SegmentCount() - 1);       
            
            if(GUILayout.Button("Add new segment"))
            {
                AddNewSegment();
            }

            if (GUILayout.Button("Delete last segment"))
            {
                DeleteLastSegment();
            }

            if (GUILayout.Button("Generate Data"))
            {
                m_instance.GenerateData();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void AddNewSegment()
        {
            // get the position of the last point in the spline (assume origin if no points exist)
            Vector3 endPoint = m_instance.EndPoint();
            Vector3 direction = m_instance.EndDirection();
            Vector3 upDirection = m_instance.EndUpDirection();

            TubeGraphic.OrientedPoint[] newPoints = new TubeGraphic.OrientedPoint[(m_instance.PointCount() == 0) ? 4 : 3];
            Vector3 placementPoint = endPoint;
            for(int i=0; i < newPoints.Length; i++) // new point generation
            {
                placementPoint += direction * 0.2f;

                TubeGraphic.OrientedPoint newPoint = new TubeGraphic.OrientedPoint();

                newPoint.Position = placementPoint;
                newPoint.Rotation = Quaternion.LookRotation(direction, upDirection);
                newPoints[i] = newPoint;
            }

            m_instance.AddNewPoints(newPoints);
        }

        void DeleteLastSegment()
        {
            if (m_instance.PointCount() > 4)
            {
                TubeGraphic.OrientedPoint[] newPoints = new TubeGraphic.OrientedPoint[m_instance.PointCount() - ((m_instance.SegmentCount() > 1) ? 3 : 4)];

                for(int i=0; i < newPoints.Length; i++)
                {
                    newPoints[i] = m_instance.GetPointAtIndex(i);
                }

                m_instance.SetAllPoints(newPoints);
            }
        }

        private void SelectIndex(int index)
        {
            selectedPoint = index;
        }

        public void OnSceneGUI()
        {
            for(int pointIndx=0; pointIndx < m_instance.PointCount(); pointIndx++)
            {
                Handles.color = (pointIndx == selectedPoint) ? Color.red : Color.blue;

                TubeGraphic.OrientedPoint point = m_instance.GetPointAtIndex(pointIndx);

                if(Handles.Button(point.Position, Quaternion.identity, HandleScale, HandleScale, Handles.SphereHandleCap))
                {
                    SelectIndex(pointIndx);
                    Repaint();
                }

                if(selectedPoint == pointIndx)
                {
                    // show our handle and allow editing
                    switch (Tools.current)
                    {
                        case Tool.View:
                        case Tool.None:
                        case Tool.Rect:
                            break;

                        case Tool.Move:
                            Vector3 newPosition = Handles.PositionHandle(point.Position, point.Rotation);
                            m_instance.SetPointAtIndex(new TubeGraphic.OrientedPoint()
                            { Position = newPosition, Rotation = point.Rotation }, pointIndx);
                            break;

                        case Tool.Rotate:
                            Quaternion newRotation = Handles.RotationHandle(point.Rotation, point.Position);
                            m_instance.SetPointAtIndex(new TubeGraphic.OrientedPoint()
                            { Position = point.Position, Rotation = newRotation }, pointIndx);
                            break;

                        case Tool.Scale:
                            break;

                        case Tool.Transform:
                            break;
                        default:
                            break;
                    }
                }
            }

            if(drawMeshProperty.boolValue)
            {
                TubeGraphic.DrawIDType drawType = (TubeGraphic.DrawIDType)drawIDTypeProperty.enumValueIndex;

                switch (drawType)
                {
                    case TubeGraphic.DrawIDType.vertex:
                        DrawVertIDs();
                        break;

                    case TubeGraphic.DrawIDType.face:
                        DrawFaceIDs();
                        break;

                    case TubeGraphic.DrawIDType.both:
                        DrawVertIDs();
                        DrawFaceIDs();
                        break;
                    default:
                        break;
                }
            }
        }

        private void DrawFaceIDs()
        {
            for(int i=0; i < m_instance.GetTriangleCount(); i++)
            {
                int vertexA, vertexB, vertexC;

                m_instance.GetTriangleForIndex(i, out vertexA, out vertexB, out vertexC);

                Vector3 centerOfPoints = Vector3.zero;

                centerOfPoints += m_instance.GetVertexForIndex(vertexA);
                centerOfPoints += m_instance.GetVertexForIndex(vertexB);
                centerOfPoints += m_instance.GetVertexForIndex(vertexC);

                centerOfPoints /= 3;

                Handles.Label(centerOfPoints, i.ToString());
            }
        }

        private void DrawVertIDs()
        {
            for (int i = 0; i < m_instance.GetVertCount(); i++)
            {
                Handles.Label(m_instance.GetVertexForIndex(i), i.ToString());
            }
        }


    } // end of class


} // end of namespace
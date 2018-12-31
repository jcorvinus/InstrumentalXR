using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidigital.Modeling.ProceduralGraphics
{
    public class TubeGraphic : MonoBehaviour
    {
        public enum DrawIDType { none, vertex, face, both }

        [System.Serializable]
        public struct OrientedPoint
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public Vector3 LocalToWorld(Vector3 point)
            {
                return (Rotation * point);
            }

            public Vector3 WorldToLocal(Vector3 point)
            {
                return (Quaternion.Inverse(Rotation) * (point - Position));
            }

            public Vector3 LocalToWorldDirection(Vector3 dir)
            {
                return (Rotation * dir);
            }
        }

        [Header("Mesh Vars")]
        [Range(3, 24)]
        [SerializeField]
        int vertsInSlice=6;

        [SerializeField]
        float radius = 0.025f;

        protected Vector3[] vertices;
        protected Vector3[] normals;
        protected int[] tris;

        Mesh mesh;

        [Header("Spline Vars")]
        [SerializeField]
        OrientedPoint[] points;

        [SerializeField]
        int splineSegmentVertCount = 10;

        [Header("Display")]
        [SerializeField]
        float normalLength = 0.01f;

        [SerializeField]
        bool drawSpline = true;

        [SerializeField]
        bool drawMesh = false;

        [Range(0,1)]
        [SerializeField]
        float slerpRefTValue;

        [SerializeField]
        bool drawSlerpedRef = false;

        //[HideInInspector]
        [SerializeField]
        int selectedSegment;

        [SerializeField]
        DrawIDType drawType;

        private void Awake()
        {
            InitMesh();
        }

        private void InitMesh()
        {
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        // Update is called once per frame
        public void Update()
        {
            // todo: change this so that it emits an event when the data is changed, and 
            // other representations only update when needed.
            GenerateData();
        }

        #region Data Access
        public int GetVertCount()
        {
            return vertices != null ? vertices.Length : 0;
        }

        public Vector3 GetVertexForIndex(int index)
        {
            return vertices[index];
        }

        public int GetTriangleCount()
        {
            return (tris == null || tris.Length == 0) ? 0 : tris.Length / 3;
        }

        public void GetTriangleForIndex(int index, out int vertA, out int vertB, out int vertC)
        {
            int baseID = index * 3;
            vertA = tris[baseID];
            vertB = tris[baseID + 1];
            vertC = tris[baseID + 2];
        }

        public Mesh GetMesh()
        {
            return mesh;
        }
        #endregion

        #region Spline Segment Methods
        public int SegmentCount()
        {
            if (points == null) return 0;
            else if (points.Length == 0) return 0;
            else return points.Length / 3;
        }

        public OrientedPoint[] GetSegmentForIndex(int segmentIndex)
        {
            Debug.Assert(segmentIndex >= 0, "Segment index cannot be negative.");
            Debug.Assert(segmentIndex < SegmentCount(), string.Format("Index {0} was out of range {1}", segmentIndex, SegmentCount() - 1));

            OrientedPoint[] segmentPoints = new OrientedPoint[4];

            int startIndex = segmentIndex * 3;

            for (int i = 0; i < 4; i++)
            {
                int pointIndex = startIndex + i;

                segmentPoints[i] = points[pointIndex];
            }

            return segmentPoints;
        }
        #endregion

        #region Spline Point Methods
        public Vector3 EndPoint()
        {
            return (SegmentCount() > 0) ? points[PointCount() - 1].Position : Vector3.zero;
        }

        public Vector3 EndDirection()
        {
            if (points == null || points.Length == 0) return Vector3.forward;
            else
            {
                return points[points.Length - 1].Rotation * Vector3.forward;
            }
        }

        public Vector3 EndUpDirection()
        {
            if (points == null) return Vector3.up;
            else if (points.Length > 2)
            {
                return points[points.Length - 1].Rotation * Vector3.up;
            }
            else return Vector3.up;
        }

        public int PointCount()
        {
            if (points == null) return 0;
            else return points.Length;
        }

        public OrientedPoint GetPointAtIndex(int pointIndex)
        {
            return points[pointIndex];
        }

        public void SetPointAtIndex(OrientedPoint point, int pointIndex)
        {
            points[pointIndex] = point;
        }

        public void AddNewPoints(OrientedPoint[] pointsToAdd)
        {
            int pointsLength = (points != null) ? points.Length : 0;
            OrientedPoint[] newPoints = new OrientedPoint[pointsLength + pointsToAdd.Length];

            for(int i=0; i < newPoints.Length; i++)
            {
                newPoints[i] = (i < pointsLength) ? points[i] : pointsToAdd[i - pointsLength];
            }

            points = newPoints;
        }

        public void SetAllPoints(OrientedPoint[] newPoints)
        {
            points = newPoints;
        }
        #endregion

        #region Spline Interpolation
        /// <summary>
        /// Cubic interpolation between 4 points
        /// </summary>
        /// <param name="tValue">0-1 percentage value.</param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="point4"></param>
        /// <returns>the result of the interpolation</returns>
        private Vector3 GetCubic(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            float u = 1 - tValue;
            float tt = tValue * tValue;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * tValue;

            Vector3 p = uuu * point1; //first term

            p += 3 * uu * tValue * point2; //second term
            p += 3 * u * tt * point3; //third term
            p += ttt * point4; //fourth term

            return p;
        }

        private Vector3 GetCubicTangent(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
        {
            float omt = 1f - tValue;
            float omt2 = omt * omt;
            float t2 = tValue * tValue;

            Vector3 tangent =
                point1 * (-omt2) +
                point2 * (3 * omt2 - 2 * omt) +
                point3 * (-3 * t2 + 2 * tValue) +
                point4 * (t2);

            return tangent.normalized;
        }

        private Vector3 GetCubicNormal(float tValue, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4,
            Vector3 up)
        {
            Vector3 tangent = GetCubicTangent(tValue, point1, point2, point3, point4);
            Vector3 binormal = Vector3.Cross(up, tangent);

            return Vector3.Cross(tangent, binormal);
        }

        void GetQuaterBoundsAndTValue(float tValue, out int lowBound, out int highBound, out float remappedTValue)
        {
            const float q1 = 0.25f;
            const float q2 = 0.75f;
            const float q3 = 1f;

            if(tValue < q1)
            {
                lowBound = 0;
                highBound = 1;

                remappedTValue = Mathf.InverseLerp(0, q1, tValue);
                return;
            }
            else if (tValue < q2)
            {
                lowBound = 1;
                highBound = 2;
                remappedTValue = Mathf.InverseLerp(q1, q2, tValue);
                return;
            }
            else //if (tValue < q3)
            {
                lowBound = 2;
                highBound = 3;
                remappedTValue = Mathf.InverseLerp(q2, q3, tValue);
                return;
            }

            lowBound = 0;
            highBound = 0;
            remappedTValue = 0;
            return;
        }

        Quaternion CubicSlerp(float tValue, Quaternion rot1, Quaternion rot2, Quaternion rot3, Quaternion rot4)
        {
            Quaternion a = Quaternion.Slerp(rot1, rot2, tValue);
            Quaternion b = Quaternion.Slerp(rot2, rot3, tValue);
            Quaternion c = Quaternion.Slerp(rot3, rot4, tValue);
            Quaternion d = Quaternion.Slerp(a, b, tValue);
            Quaternion e = Quaternion.Slerp(c, b, tValue);

            return Quaternion.Slerp(d, e, tValue);
        }

        Quaternion HardCubicSlerp(float tValue, Quaternion rot1, Quaternion rot2, Quaternion rot3, Quaternion rot4)
        {
            float increment = (1f / 4f);

            float zoneOneLower = increment * 0;
            float zoneOneUpper = increment * 1;
            float zoneOne = Mathf.InverseLerp(zoneOneLower, zoneOneUpper, tValue);

            if(zoneOne < 1)
            {
                return Quaternion.Slerp(rot1, rot2, zoneOne);
            }

            float zoneTwoLower = zoneOneUpper;
            float zoneTwoUpper = increment * 2;
            float zoneTwo = Mathf.InverseLerp(zoneTwoLower, zoneTwoUpper, tValue);

            if(zoneTwo < 1)
            {
                return Quaternion.Slerp(rot2, rot3, zoneTwo);
            }

            float zoneThreeLower = increment * 2;
            float zoneThreeUpper = increment * 3;
            float zoneThree = Mathf.InverseLerp(zoneThreeLower, zoneThreeUpper, tValue);

            return Quaternion.Slerp(rot3, rot4, zoneThree);         
        }

        #endregion

        #region Mesh Generation Methods
        public void GenerateData()
        {
            int vertCount = vertsInSlice * splineSegmentVertCount * SegmentCount();
            if (vertices == null || vertices.Length != vertCount)
            {
                vertices = new Vector3[vertCount];
                normals = new Vector3[vertCount];
            }

            int triangleCount = (vertsInSlice * 2) * splineSegmentVertCount * SegmentCount();
            bool triangleCountChanged = tris == null || tris.Length != triangleCount * 3;
            if (tris == null || triangleCountChanged) tris = new int[triangleCount * 3];

            int trianglePlaceIndx = 0;

            int placeIndex = 0;
            for (int segID = 0; segID < SegmentCount(); segID++)
            {
                GetVertsForSegment(segID, triangleCountChanged, ref vertices, ref normals, ref placeIndex, ref tris, ref trianglePlaceIndx);
            }

            if (!mesh) InitMesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
        }

        Vector3[] GetCylinderVerts(float radius, int vertCount)
        {
            Vector3[] verts = new Vector3[vertCount];

            float angleIncrement = 360 / vertCount;

            for (int i = 0; i < vertCount; i++)
            {
                verts[i] = (Quaternion.AngleAxis(angleIncrement * i, Vector3.forward) * Vector3.up) * radius;
            }

            return verts;
        }

        private void GetVertsForSegment(int segmentID, bool processTriangles, ref Vector3[] verts, ref Vector3[] normals, ref int placeIndx, ref int[] triangles, ref int trianglePlaceIndx)
        {
            OrientedPoint[] points = GetSegmentForIndex(segmentID);

            for (int sliceIndx = 0; sliceIndx < splineSegmentVertCount; sliceIndx++)
            {
                int sliceVertBaseValue = placeIndx;
                float tValue = Mathf.InverseLerp(0, splineSegmentVertCount - 1, sliceIndx);

                Vector3[] cylLocalVerts = GetCylinderVerts(radius, vertsInSlice);

                Vector3 upDirection = HardCubicSlerp(tValue, points[0].Rotation, points[1].Rotation, points[2].Rotation, points[3].Rotation) * Vector3.up;
                GetCubic(tValue, points[0].LocalToWorld(Vector3.up), points[1].LocalToWorld(Vector3.up), points[2].LocalToWorld(Vector3.up), points[3].LocalToWorld(Vector3.up));

                Vector3 center = GetCubic(tValue, points[0].Position, points[1].Position, points[2].Position, points[3].Position);
                Vector3 tangent = GetCubicTangent(tValue, points[0].Position, points[1].Position, points[2].Position, points[3].Position);
                Vector3 normal = GetCubicNormal(tValue, points[0].Position, points[1].Position, points[2].Position, points[3].Position, upDirection);
                Quaternion rotation = Quaternion.LookRotation(tangent, normal);

                // rotate and place verts.
                for (int vertIndx = 0; vertIndx < cylLocalVerts.Length; vertIndx++)
                {
                    cylLocalVerts[vertIndx] = rotation * cylLocalVerts[vertIndx];
                    cylLocalVerts[vertIndx] += center;

                    if (sliceIndx == 0 && segmentID > 0)
                    {
                        // if slice index is equal to zero, we need to
                        // connect to the previous slice.
                        // do this by setting *both* slice's verts to
                        // the average positions

                        // get the index of our matching previous slice index
                        int prevSliceVertIndx = sliceVertBaseValue - (vertsInSlice - vertIndx);

                        Vector3 averagePos = (verts[prevSliceVertIndx] + cylLocalVerts[vertIndx]) / 2;

                        verts[prevSliceVertIndx] = averagePos;
                        cylLocalVerts[vertIndx] = averagePos;
                    }

                    // put vert into vert array
                    verts[placeIndx] = cylLocalVerts[vertIndx];
                    normals[placeIndx] = (cylLocalVerts[vertIndx] - center).normalized;
                    placeIndx++;
                }

                #region Generating Faces For Slice
                if (sliceIndx > 0)
                {
                    int previousSliceID = sliceIndx - 1;
                    int previousSliceVertBaseID = sliceVertBaseValue - vertsInSlice;

                    // generate faces, connecting to previous slice
                    // how many faces do we generate?
                    // should be vertsinslice * 2
                    int facesToGenerate = vertsInSlice * 1;
                    for(int faceIndx=0; faceIndx < facesToGenerate; faceIndx++)
                    {
                        bool lastFace = faceIndx == facesToGenerate - 1;

                        // each face is a quad, split it into two tris
                        int[] triangleA = new int[3];
                        int[] triangleB = new int[3];

                        triangleA[0] = sliceVertBaseValue + faceIndx + 0;
                        triangleA[1] = previousSliceVertBaseID + faceIndx + 0;
                        triangleA[2] = sliceVertBaseValue  + ((lastFace) ? 0 : faceIndx + 1); // fadeindx + 1 will go out of bounds if faceIndx == facesToGenerate - 1

                        triangleB[0] = previousSliceVertBaseID + faceIndx + 0;
                        triangleB[1] = previousSliceVertBaseID + ((lastFace) ? 0 : faceIndx + 1);  // fadeindx + 1 will go out of bounds if faceIndx == facesToGenerate - 1
                        triangleB[2] = sliceVertBaseValue + ((lastFace) ? 0 : faceIndx + 1);  // fadeindx + 1 will go out of bounds if faceIndx == facesToGenerate - 1

                        triangles[trianglePlaceIndx] = triangleA[0];
                        trianglePlaceIndx++;
                        triangles[trianglePlaceIndx] = triangleA[1];
                        trianglePlaceIndx++;
                        triangles[trianglePlaceIndx] = triangleA[2];

                        trianglePlaceIndx++;
                        triangles[trianglePlaceIndx] = triangleB[0];
                        trianglePlaceIndx++;
                        triangles[trianglePlaceIndx] = triangleB[1];
                        trianglePlaceIndx++;
                        triangles[trianglePlaceIndx] = triangleB[2];
                        trianglePlaceIndx++;                            
                    }
                }
                #endregion
            }
        }
        #endregion

        private void DrawSpline()
        {
            for (int i = 0; i < SegmentCount(); i++)
            {
                // get verts for segment
                OrientedPoint[] segmentPoints = GetSegmentForIndex(i);

                for (int segPointIndx = 0; segPointIndx < segmentPoints.Length; segPointIndx++)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(segmentPoints[segPointIndx].Position, segmentPoints[segPointIndx].LocalToWorldDirection(Vector3.up) * normalLength);

                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(segmentPoints[segPointIndx].Position, segmentPoints[segPointIndx].LocalToWorldDirection(Vector3.right) * normalLength * 0.5f);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(segmentPoints[segPointIndx].Position, segmentPoints[segPointIndx].LocalToWorldDirection(Vector3.forward) * normalLength * 0.5f);
                }

                Gizmos.color = Color.white;
                for (int vertIndx = 1; vertIndx < splineSegmentVertCount; vertIndx++)
                {
                    float tValuePrevious = Mathf.InverseLerp(0, splineSegmentVertCount - 1, vertIndx - 1);
                    float tValue = Mathf.InverseLerp(0, splineSegmentVertCount - 1, vertIndx);

                    Gizmos.DrawLine(
                        GetCubic(tValuePrevious, segmentPoints[0].Position, segmentPoints[1].Position,
                            segmentPoints[2].Position, segmentPoints[3].Position),
                        GetCubic(tValue, segmentPoints[0].Position, segmentPoints[1].Position,
                            segmentPoints[2].Position, segmentPoints[3].Position));
                }
            }
        }

        private void DrawMesh()
        {
            if (vertices != null && tris != null)
            {
                for (int i = 0; i < GetTriangleCount(); i++)
                {
                    int indexA, indexB, indexC;

                    GetTriangleForIndex(i, out indexA, out indexB, out indexC);

                    Gizmos.DrawLine(vertices[indexA], vertices[indexB]);
                    Gizmos.DrawLine(vertices[indexB], vertices[indexC]);
                    Gizmos.DrawLine(vertices[indexA], vertices[indexC]);
                }
            }
        }

        void DrawCubicLerp(float tValue, Vector3 pos1, Vector3 pos2, Vector3 pos3, Vector3 pos4)
        {
            Vector3 a = Vector3.Lerp(pos1, pos2, tValue);
            Vector3 b = Vector3.Lerp(pos2, pos3, tValue);
            Vector3 c = Vector3.Lerp(pos3, pos4, tValue);
            Vector3 d = Vector3.Lerp(a, b, tValue);
            Vector3 e = Vector3.Lerp(c, b, tValue);

            // lerping between d and e gives you the final point

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos1, pos2);
            Gizmos.DrawLine(pos2, pos3);
            Gizmos.DrawLine(pos3, pos4);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(d, e);
        }

        private void OnDrawGizmos()
        {
            if (drawSpline)
            {
                DrawSpline();
            }

            if(drawMesh)
            {
                GenerateData();
                DrawMesh();
            }

            if(drawSlerpedRef)
            {
                if(SegmentCount() >= 1 && 
                    selectedSegment < SegmentCount() && selectedSegment >= 0)
                {
                    //normalLength
                    OrientedPoint[] refPoints = GetSegmentForIndex(selectedSegment);

                    Vector3 lerpPoint;
                    Quaternion slerpRotation;

                    lerpPoint = GetCubic(slerpRefTValue, refPoints[0].Position, refPoints[1].Position, refPoints[2].Position, refPoints[3].Position);
                    slerpRotation = HardCubicSlerp(slerpRefTValue, refPoints[0].Rotation, refPoints[1].Rotation, refPoints[2].Rotation, refPoints[3].Rotation);

                    Vector3 up, forward, right;

                    up = slerpRotation * Vector3.up;
                    forward = slerpRotation * Vector3.forward;
                    right = slerpRotation * Vector3.right;

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(lerpPoint, lerpPoint + (up * normalLength));

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lerpPoint, lerpPoint + (forward * normalLength));

                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(lerpPoint, lerpPoint + (right * normalLength));

                    // draws the reference lines
                    DrawCubicLerp(slerpRefTValue, refPoints[0].Position, refPoints[1].Position, refPoints[2].Position, refPoints[3].Position);
                }
            }
        }
    }
}
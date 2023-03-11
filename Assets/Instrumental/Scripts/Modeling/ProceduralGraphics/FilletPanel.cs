using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Modeling;

namespace Instrumental.Modeling.ProceduralGraphics
{
    public class FilletPanel : MonoBehaviour
    {
        public delegate void PanelPropertiesHandler(FilletPanel sender);
        public event PanelPropertiesHandler PropertiesChanged;            

        public enum BorderType
        {
            None,
            Outline,
            OutlineAndExtrude
        }

        public enum VisualizationMode
        {
            None,
            IdealOutlines,
            ActualOutlines,
            Mesh
        }

        public struct CornerInfo
        {
            public Vector3 Center;
            public Vector3 Normal;
            public Vector3 From;
            public float Angle;
            public bool Valid;
            public float Radius;
        }

        public struct FaceVertexArrayInfo
        {
            public int UpperEdgeBaseID;
            public int LowerEdgeBaseID;
            public int LeftEdgeBaseID;
            public int RightEdgeBaseID;

            public int UpperLeftCornerBaseID;
            public int UpperRightCornerBaseID;
            public int LowerLeftCornerBaseID;
            public int LowerRightCornerBaseID;

            public int InnerGridEndID; // if this is -1, we don't have an inner grid
        }

        public struct PanelInfo
        {
            public FaceVertexArrayInfo FrontVertInfo;
            public FaceVertexArrayInfo BackVertInfo;
            public FaceVertexArrayInfo FrontPanelExtrudeVertInfo;
            public FaceVertexArrayInfo BackPanelExtrudeVertInfo;
            public FaceVertexArrayInfo FrontOuterVertOutlineExtrudeInfo;
            public FaceVertexArrayInfo FrontInnerVertOutlineExtrudeInfo;
        }

        public const int MIN_FILLET_SEGMENTS = 2;
        public const int MAX_FILLET_SEGMENTS = 8;

        public const float MIN_INSET_PERCENT = 0.1f;
        public const float MAX_INSET_PERCENT = 0.5f;

        public const int MIN_BORDER_SEGMENTS = 1;
        public const int MAX_BORDER_SEGMENTS = 4;

        public const float MIN_DEPTH = 0;
        public const float MAX_DEPTH = 0.2f;

        public const float MIN_DIMENSION_SIZE = 0.09f;
        public const float MAX_DIMENSION_WIDTH = 0.9f;
        public const float MAX_DIMENSION_HEIGHT = 0.6f;

        [SerializeField]
        Vector2 panelDimensions;

        [SerializeField]
        float depth = 0.01f;

        [SerializeField]
        float radius;

        [Range(MIN_FILLET_SEGMENTS, MAX_FILLET_SEGMENTS)]
        [SerializeField]
        int filletSegments;

        [SerializeField]
        int widthSegments;

        [SerializeField]
        int heightSegments;

        [SerializeField]
        bool useVColors = true;

        [SerializeField]
        ColorType faceColorType = ColorType.FlatColor;

        [SerializeField]
        Gradient faceGradient;

        [SerializeField]
        GradientInfo faceGradientInfo;

        [SerializeField]
        Color faceColor = Color.white;

        [Header("Border")]
        [SerializeField]
        BorderType border = BorderType.None;

        [Range(MIN_INSET_PERCENT, MAX_INSET_PERCENT)]
        [SerializeField]
        float borderInsetPercent = 0.1f;

        [Range(MIN_BORDER_SEGMENTS, MAX_BORDER_SEGMENTS)]
        [SerializeField]
        int borderSegments = 1;

        [SerializeField]
        Color borderColor = Color.white;

        public ColorType FaceColorType { get { return faceColorType; } }

        #region Mesh Vars
        Mesh mesh;

        PanelInfo panelInfo;
        Vector3[] verts;
        Vector2[] uvs;
        int[] tris;
        Color[] vColors;

        public Mesh Mesh { get { return mesh; } }
        public Vector3[] Verts { get { return verts; } }
        public Vector2[] UVs { get { return uvs; } }
        public int[] Tris { get { return tris; } }
        #endregion

        #region Debug Variables
        [SerializeField]
        VisualizationMode visualizationMode = VisualizationMode.ActualOutlines;

        [SerializeField]
        bool displayVertIDs = false;

        [SerializeField]
        bool displaySegmentLines = true;

        [SerializeField]
        bool displayNormals = false;

        [SerializeField]
        bool doBackFace = true;

        public bool DoExtrusion { get { return depth > 0; } }
        #endregion

        private void Awake()
        {
            GenerateMesh();
        }

        // Use this for initialization
        void Start()
        {

        }

        private void OnValidate()
        {
            widthSegments = Mathf.Max(0, widthSegments);
            heightSegments = Mathf.Max(0, heightSegments);

            if(panelDimensions.x < MIN_DIMENSION_SIZE)
            {
                panelDimensions = new Vector2(MIN_DIMENSION_SIZE, panelDimensions.y);
            }

            if(panelDimensions.y < MIN_DIMENSION_SIZE)
            {
                panelDimensions = new Vector2(MIN_DIMENSION_SIZE, 0.001f);
            }

            radius = Mathf.Max(radius, 0);

            depth = (depth >= 0) ? depth : 0;

            GenerateMesh();

            SetPropertiesChanged();
        }

        private void SetPropertiesChanged()
        {
            if (PropertiesChanged != null)
            {
                PropertiesChanged(this);
            }
        }

        public void SetDimensions(Vector2 dimensions)
        {
            panelDimensions = dimensions;
            SetPropertiesChanged();
        }

        public void SetDepth(float depth)
        {
            this.depth = depth;
            SetPropertiesChanged();
        }

        public void SetRadius(float radius)
        {
            this.radius = radius;
            SetPropertiesChanged();
        }

        public void SetBorderColor(Color color)
        {
            borderColor = color;

            if(useVColors)
            {
                //GenerateVerts(out verts, out panelInfo);
                GenerateVColors(out vColors);
                //mesh.vertices = verts;
                mesh.colors = vColors;
            }

            SetPropertiesChanged();
        }

        public void SetBorderInsetPercent(float percent)
        {
            borderInsetPercent = percent;
            SetPropertiesChanged();
        }

        public void SetFaceGradient(Gradient surfaceGradient)
        {
            faceGradient = surfaceGradient;

            if (useVColors)
            {
                //GenerateVerts(out verts, out panelInfo);
                GenerateVColors(out vColors);
                //mesh.vertices = verts;
                mesh.colors = vColors;
            }

            SetPropertiesChanged();
        }

        public void SetFaceColor(Color surfaceColor)
        {
            faceColor = surfaceColor;

            if (useVColors)
            {
                //GenerateVerts(out verts, out panelInfo);
                GenerateVColors(out vColors);
                //mesh.vertices = verts;
                mesh.colors = vColors;
            }

            SetPropertiesChanged();
        }

        #region Panel Shape Methods
        // assuming both vector3s are on a plane (and as such, actually vector2s),
        // get the normal that points 'towards' the reference point
        private Vector3 GetNormal(Vector3 position1, Vector3 position2, Vector3 reference, out Vector3 center)
        {
            Vector3 direction = (position2 - position1).normalized;
            center = (position1 + position2) * 0.5f;

            Vector3 directionToReference = (reference - center).normalized;

            Vector3 normal1 = Quaternion.AngleAxis(90, Vector3.forward) * direction;
            Vector3 normal2 = Quaternion.AngleAxis(-90, Vector3.forward) * direction;

            return (Vector3.Dot(normal1, directionToReference) > Vector3.Dot(normal2, directionToReference)) ?
                normal1 : normal2;
        }

        public void GetCorners(out Vector3 v1, out Vector3 v2, out Vector3 v3, out Vector3 v4,
            float insetValue)
        {
            v1 = Vector3.up * (panelDimensions.y - insetValue) + Vector3.right * -1 * (panelDimensions.x - insetValue);
            v1 *= 0.5f;
            v2 = Vector3.down * (panelDimensions.y - insetValue) + Vector3.right * -1 * (panelDimensions.x - insetValue);
            v2 *= 0.5f;
            v3 = Vector3.down * (panelDimensions.y - insetValue) + Vector3.right * (panelDimensions.x - insetValue);
            v3 *= 0.5f;
            v4 = Vector3.up * (panelDimensions.y - insetValue) + Vector3.right * (panelDimensions.x - insetValue);
            v4 *= 0.5f;

            return;
        }

        public Vector3[] GetUpperPoints(float inset = 0)
        {
            Vector3 upperLeft, upperRight;
            upperLeft = Vector3.up * (panelDimensions.y - inset) + Vector3.right * -1 * (panelDimensions.x - inset);
            upperLeft *= 0.5f;
            upperLeft = upperLeft + Vector3.right * radius;

            upperRight = Vector3.up * (panelDimensions.y - inset) + Vector3.right * (panelDimensions.x - inset);
            upperRight *= 0.5f;
            upperRight = upperRight + Vector3.right * -radius;

            Vector3[] points = new Vector3[widthSegments + 2];

            points[0] = upperLeft;
            points[points.Length - 1] = upperRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(widthSegments + 1f)) * i;

                points[i] = Vector3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vector3[] GetLowerPoints(float inset=0)
        {
            Vector3 lowerLeft, lowerRight;
            lowerLeft = Vector3.down * (panelDimensions.y - inset) + Vector3.right * -1 * (panelDimensions.x - inset);
            lowerLeft *= 0.5f;
            lowerLeft = lowerLeft + Vector3.right * radius;

            lowerRight = Vector3.down * (panelDimensions.y - inset) + Vector3.right * (panelDimensions.x - inset);
            lowerRight *= 0.5f;
            lowerRight = lowerRight + Vector3.right * -radius;

            Vector3[] points = new Vector3[widthSegments + 2];

            points[0] = lowerLeft;
            points[points.Length - 1] = lowerRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(widthSegments + 1f)) * i;

                points[i] = Vector3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vector3[] GetLeftPoints(float inset=0)
        {
            Vector3 upperLeft, lowerLeft;

            upperLeft = Vector3.up * (panelDimensions.y - inset) + Vector3.right * -1 * (panelDimensions.x - inset);
            upperLeft *= 0.5f;
            upperLeft = upperLeft + Vector3.up * -radius;

            lowerLeft = Vector3.down * (panelDimensions.y - inset) + Vector3.right * -1 * (panelDimensions.x - inset);
            lowerLeft *= 0.5f;
            lowerLeft = lowerLeft + Vector3.up * radius;

            Vector3[] points = new Vector3[heightSegments + 2];
            points[0] = upperLeft;
            points[points.Length - 1] = lowerLeft;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(heightSegments + 1f)) * i;

                points[i] = Vector3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public Vector3[] GetRightPoints(float inset=0)
        {
            Vector3 upperRight, lowerRight;

            upperRight = Vector3.up * (panelDimensions.y - inset) + Vector3.right * (panelDimensions.x - inset);
            upperRight *= 0.5f;
            upperRight = upperRight + Vector3.up * -radius;

            lowerRight = Vector3.down * (panelDimensions.y - inset) + Vector3.right * (panelDimensions.x - inset);
            lowerRight *= 0.5f;
            lowerRight = lowerRight + Vector3.up * radius;

            Vector3[] points = new Vector3[heightSegments + 2];
            points[0] = upperRight;
            points[points.Length - 1] = lowerRight;

            for (int i = 1; i < points.Length - 1; i++)
            {
                float tValue = (1f / (float)(heightSegments + 1f)) * i;

                points[i] = Vector3.Lerp(points[0], points[points.Length - 1], tValue);
            }

            return points;
        }

        public CornerInfo GetCorner(Vector3 v1, Vector3 v2, Vector3 v3, float _radius)
        {
            Vector3 v2ToV1Dir = (v1 - v2).normalized;
            Vector3 v2ToV3Dir = (v3 - v2).normalized;

            Vector3 avgDir = (v2ToV1Dir + v2ToV3Dir) * 0.5f;
            avgDir = avgDir.normalized;

            // drawing normals
            Vector3 v2ToV1Center;
            Vector3 v2ToV3Center;

            Vector3 v2ToV1Normal = GetNormal(v2, v1, (v2 + avgDir * _radius), out v2ToV1Center);
            Vector3 v2ToV3Normal = GetNormal(v2, v3, (v2 + avgDir * _radius), out v2ToV3Center);


            Vector3 offsetL1V1 = v2 + (v2ToV1Normal * _radius);
            Vector3 offsetL1V2 = v1 + (v2ToV1Normal * _radius);

            Vector3 offsetL2V1 = v2 + (v2ToV3Normal * _radius);
            Vector3 offsetL2V2 = v3 + (v2ToV3Normal * _radius);

            // let's turn l2 into a Plane and then do Plane.raycast
            Plane l2Plane = new Plane(offsetL2V1, offsetL2V2, offsetL2V1 + Vector3.forward);
            Ray l1Ray = new Ray(offsetL1V1, offsetL1V2 - offsetL1V1);

            Vector3 center = Vector3.zero;

            float intersect = 0f;
            if (l2Plane.Raycast(l1Ray, out intersect))
            {
                center = l1Ray.origin + l1Ray.direction * intersect;

                // get our intersect points by walking back up our normals to the
                // original lines
                Plane v2ToV1Plane = new Plane(v1, v2, v1 + Vector3.forward);
                Plane v2ToV3Plane = new Plane(v2, v3, v3 + Vector3.forward);

                Ray arcStartRay = new Ray(center, v2ToV1Normal * -1);
                float arcStartDistance = 0f;
                Vector3 arcStartPoint = v2ToV1Center;
                if (v2ToV1Plane.Raycast(arcStartRay, out arcStartDistance))
                {
                    arcStartPoint = arcStartRay.origin + arcStartRay.direction * arcStartDistance;

                    float normalMult = 1; // we need to figure out when to flip this!

                    return new CornerInfo()
                    {
                        Angle = Vector3.Angle(v2ToV1Dir, v2ToV3Dir),
                        Center = center,
                        From = (arcStartPoint - center).normalized,
                        Normal = Vector3.forward * normalMult,
                        Radius = _radius,
                        Valid = true
                    };
                }
                else
                {
                    return new CornerInfo()
                    {
                        Center = center,
                        Normal = Vector3.forward,
                        Radius = _radius,
                        Valid = false
                    };
                }
            }
            else
            {
                return new CornerInfo()
                {
                    Center = center,
                    Normal = Vector3.forward,
                    Radius = _radius,
                    Valid = false
                };
            }
        }

        #endregion

        #region Meshing Methods

        public void GenerateMesh()
        {
            PanelInfo panelInfo;

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.MarkDynamic();
                mesh.name = "fillet";
            }

            GenerateVerts(out verts, out panelInfo);
            GenerateUVs(verts, out uvs);

            if(useVColors)
            {
                GenerateVColors(out vColors);
            }

            int[] triangles;
            GenerateTriangles(panelInfo, out triangles);

            tris = triangles;

            mesh.Clear();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            if (useVColors) mesh.colors = vColors;
            else mesh.colors = null;
            mesh.RecalculateNormals();

            this.panelInfo = panelInfo;
        }

        public void GenerateVerts(out Vector3[] verts, out PanelInfo panelInfo)
        {
            verts = new Vector3[GetTotalVertBufferSize()];

            FaceVertexArrayInfo frontVertInfo;
            FaceVertexArrayInfo backVertInfo;
            FaceVertexArrayInfo frontPanelExtrudeVertInfo;
            FaceVertexArrayInfo backPanelExtrudeVertInfo;
            FaceVertexArrayInfo frontOuterVertOutlineExtrudeInfo;

            switch (border)
            {
                case BorderType.Outline:
                    GenerateVertsOutline(ref verts, out frontVertInfo,
                        out backVertInfo, out frontOuterVertOutlineExtrudeInfo, out frontPanelExtrudeVertInfo,
                        out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontOuterVertOutlineExtrudeInfo = frontOuterVertOutlineExtrudeInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
                case BorderType.OutlineAndExtrude:
                    FaceVertexArrayInfo frontInnerExtrudeInfo;
                    GenerateVertsOutlineExtrude(ref verts, out frontVertInfo,
                        out backVertInfo, out frontOuterVertOutlineExtrudeInfo, out frontInnerExtrudeInfo,
                        out frontPanelExtrudeVertInfo, out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontOuterVertOutlineExtrudeInfo = frontOuterVertOutlineExtrudeInfo,
                        FrontInnerVertOutlineExtrudeInfo = frontInnerExtrudeInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
                default:
                    GenerateVertsNoOutline(ref verts, out frontVertInfo, out backVertInfo,
                        out frontPanelExtrudeVertInfo, out backPanelExtrudeVertInfo);

                    panelInfo = new PanelInfo()
                    {
                        FrontVertInfo = frontVertInfo,
                        BackVertInfo = backVertInfo,
                        FrontPanelExtrudeVertInfo = frontPanelExtrudeVertInfo,
                        BackPanelExtrudeVertInfo = backPanelExtrudeVertInfo
                    };
                    break;
            }
        }

        public void GenerateUVs(Vector3[] verts, out Vector2[] uvs)
        {
            uvs = new Vector2[verts.Length];

            // come up with a method of normalizing the vert locations
            // according to the panel dimensions
            for (int i = 0; i < verts.Length; i++)
            {
                uvs[i] = new Vector2(verts[i].x, verts[i].y);
            }
        }

        void GenerateVertsNoOutline(ref Vector3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontPanelExtrudeVertInfo,
            out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            Vector3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, 0);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false,
                radius, 0, out frontVertInfo);

            if (DoExtrusion)
            {
                // get front extrusion edge
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontPanelExtrudeVertInfo);

                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (doBackFace)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        void GenerateVertsOutline(ref Vector3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontOutlineVertInfo,
            out FaceVertexArrayInfo frontPanelExtrudeVertInfo, out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            float inset = radius * borderInsetPercent;

            Vector3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false, radius, inset, out frontVertInfo);

            GetCorners(out c1, out c2, out c3, out c4, 0);

            int frontOutlineBaseID = baseID;
            // do our front outline here
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontOutlineVertInfo);

            if (DoExtrusion)
            {
                // get front extrusion edge
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontPanelExtrudeVertInfo, 0);

                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (doBackFace)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        void GenerateVertsOutlineExtrude(ref Vector3[] verts, out FaceVertexArrayInfo frontVertInfo,
            out FaceVertexArrayInfo backVertInfo, out FaceVertexArrayInfo frontOutlineVertInfo,
            out FaceVertexArrayInfo frontInnerExtrudeVertInfo, out FaceVertexArrayInfo frontPanelExtrudeVertInfo,
            out FaceVertexArrayInfo backPanelExtrudeVertInfo)
        {
            float inset = radius * borderInsetPercent;

            Vector3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int baseID = 0;

            baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, false,
                radius, inset, out frontVertInfo);

            GetCorners(out c1, out c2, out c3, out c4, 0);

            int frontOutlineBaseID = baseID;

            // do our front outer outline here
            baseID = GenerateBorderEdge(ref verts, baseID, radius,
                out frontPanelExtrudeVertInfo);

            // adjust front outline here
            for (int i = frontOutlineBaseID; i < baseID; i++)
            {
                verts[i] += Vector3.back * depth;
            }

            // re-purposing our frontOutlineVertInfo to be the duplicate edge
            // of the front panel
            int frontInnerFaceDuplicateVerts = baseID;
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontOutlineVertInfo, inset);

            int frontInnerExtrudeBaseID = baseID;
            baseID = GenerateBorderEdge(ref verts, baseID, radius, out frontInnerExtrudeVertInfo, inset);

            for (int i = frontInnerExtrudeBaseID; i < baseID; i++)
            {
                // push these verts forward
                verts[i] += Vector3.back * depth;
            }

            if (DoExtrusion)
            {
                int backExtrusionID = baseID;
                baseID = GenerateBorderEdge(ref verts, baseID, radius, out backPanelExtrudeVertInfo);

                for (int i = backExtrusionID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                frontPanelExtrudeVertInfo = new FaceVertexArrayInfo();
                backPanelExtrudeVertInfo = new FaceVertexArrayInfo();
            }

            if (DoExtrusion)
            {
                int backSideBaseID = baseID;
                baseID = GenerateFaceVerts(ref verts, baseID, c1, c2, c3, c4, true,
                    radius, 0, out backVertInfo);

                for (int i = backSideBaseID; i < baseID; i++)
                {
                    // push these verts back
                    verts[i] += Vector3.forward * depth;
                }
            }
            else
            {
                backVertInfo = new FaceVertexArrayInfo();
            }
        }

        private int GenerateFaceVerts(ref Vector3[] verts, int baseID,
            Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, bool isBack,
            float _radius, float inset, out FaceVertexArrayInfo faceInfo)
        {
            baseID = GetGridInnerVerts(ref verts, baseID, inset);

            int innerGridEndID = baseID;

            int upBaseID, downBaseID, leftBaseID, rightBaseID;

            baseID = GetGridEdgeVerts(ref verts, baseID, out upBaseID,
                out downBaseID, out leftBaseID, out rightBaseID, inset);

            // getting our vertex fans
            //corner 0: (v4, v1, v2); // upper left
            //corner 1: (v1, v2, v3); // upper right
            //corner 2: (v2, v3, v4); // lower left
            //corner 3: (v3, v4, v1); // lower right

            int upperLeftCornerBaseID = 0, lowerLeftCornerBaseID = 0, lowerRightCornerBaseID = 0, upperRightCornerBaseID = 0;

            if (filletSegments > 2)
            {
                upperLeftCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c4, c1, c2, _radius);

                lowerLeftCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c1, c2, c3, _radius);

                lowerRightCornerBaseID = baseID;
                baseID = GetCornerFanVerts(ref verts, baseID, c2, c3, c4, _radius);

                upperRightCornerBaseID = baseID; // actually upper right
                baseID = GetCornerFanVerts(ref verts, baseID, c3, c4, c1, _radius);
            }
            else
            {
                upperLeftCornerBaseID = leftBaseID;
                upperRightCornerBaseID = upBaseID + widthSegments + 1;
                lowerLeftCornerBaseID = downBaseID;
                lowerRightCornerBaseID = rightBaseID + heightSegments + 1;
            }

            faceInfo = new FaceVertexArrayInfo()
            {
                UpperEdgeBaseID = upBaseID,
                LowerEdgeBaseID = downBaseID,
                LeftEdgeBaseID = leftBaseID,
                RightEdgeBaseID = rightBaseID,
                UpperLeftCornerBaseID = upperLeftCornerBaseID,
                UpperRightCornerBaseID = upperRightCornerBaseID,
                LowerLeftCornerBaseID = lowerLeftCornerBaseID,
                LowerRightCornerBaseID = lowerRightCornerBaseID,
                InnerGridEndID = innerGridEndID
            };

            return baseID;
        }

        int GetCornerFanVerts(ref Vector3[] verts, int baseID, Vector3 c1, Vector3 c2, Vector3 c3, float _radius)
        {
            int trackID = baseID;

            int cornerVertsCount = filletSegments;

            CornerInfo cornerInfo = GetCorner(c1, c2, c3, _radius);

            float angleIncrement = cornerInfo.Angle / (cornerVertsCount - 1);

            for (int i = 1; i < cornerVertsCount - 1; i++)
            {
                verts[trackID] = cornerInfo.Center + (Quaternion.AngleAxis(angleIncrement * (i), cornerInfo.Normal) *
                    cornerInfo.From) * cornerInfo.Radius;

                trackID++;
            }

            return trackID;
        }

        /// <summary>
        /// Add the inner grid of verts to the vertex buffer
        /// </summary>
        /// <param name="verts">Vertes budffer</param>
        /// <param name="baseID">starting offset</param>
        /// <returns>index of last placed vertex</returns>
        int GetGridInnerVerts(ref Vector3[] verts, int baseID, float inset = 0)
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            Vector2 innerDimensions = panelDimensions - (Vector2.one * radius * 2) - (Vector2.one * inset);

            Vector3 startPos = (Vector3.left * 0.5f * (panelDimensions.x - inset)) +
                (Vector3.up * 0.5f * (panelDimensions.y - inset)) + new Vector3(1, -1, 0) * radius;

            float widthIncrement = innerDimensions.x / (float)(widthSegments + 1);
            float heightIncrement = innerDimensions.y / (float)(heightSegments + 1);

            int trackedIndx = baseID;
            for (int vertIndx = 0; vertIndx < innerHeightSements; vertIndx++)
            {
                for (int horizIndx = 0; horizIndx < innerWidthSegments; horizIndx++)
                {
                    verts[trackedIndx] = startPos + (Vector3.right * widthIncrement * horizIndx) +
                        (Vector3.down * heightIncrement * vertIndx);

                    trackedIndx++;
                }
            }

            return trackedIndx;
        }

        /// <summary>
        /// Add the outer edge verts to the vertex buffer
        /// </summary>
        /// <param name="verts">Vertes budffer</param>
        /// <param name="baseID">starting offset</param>
        /// <returns>index of last placed vertex</returns>
        int GetGridEdgeVerts(ref Vector3[] verts, int baseID, out int upBaseID, out int downBaseID,
            out int leftBaseID, out int rightBaseID, float inset = 0)
        {
            int horizVertCount = widthSegments + 2;
            int verticalVertCount = heightSegments + 2;

            int vertTrackID = baseID;
            upBaseID = baseID;
            downBaseID = 0;
            leftBaseID = 0;
            rightBaseID = 0;

            Vector2 innerDimensions = panelDimensions - (Vector2.one * radius * 2) - (Vector2.one * inset);
            float widthIncrement = innerDimensions.x / (float)(widthSegments + 1);
            float heightIncrement = innerDimensions.y / (float)(heightSegments + 1);

            // up verts
            for (int i = 0; i < horizVertCount; i++)
            {
                Vector3 startPos = (Vector3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vector3.up * 0.5f * (panelDimensions.y - inset)) + new Vector3(1, 0, 0) * radius;

                verts[vertTrackID] = startPos + (Vector3.right * widthIncrement * i);
                vertTrackID++;
            }

            // down verts
            downBaseID = vertTrackID;
            for (int i = 0; i < horizVertCount; i++)
            {
                Vector3 startPos = (Vector3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vector3.up * 0.5f * -(panelDimensions.y - inset)) + new Vector3(1, 0, 0) * radius;

                verts[vertTrackID] = startPos + (Vector3.right * widthIncrement * i);
                vertTrackID++;
            }

            // left verts
            leftBaseID = vertTrackID;
            for (int i = 0; i < verticalVertCount; i++)
            {
                Vector3 startPos = (Vector3.left * 0.5f * (panelDimensions.x - inset)) +
                    (Vector3.up * 0.5f * (panelDimensions.y - inset)) + new Vector3(0, -1, 0) * radius;

                verts[vertTrackID] = startPos + (Vector3.down * heightIncrement * i);
                vertTrackID++;
            }

            // right verts
            rightBaseID = vertTrackID;
            for (int i = 0; i < verticalVertCount; i++)
            {
                Vector3 startPos = (Vector3.right * 0.5f * (panelDimensions.x - inset)) +
                    (Vector3.up * 0.5f * (panelDimensions.y - inset)) + new Vector3(0, -1, 0) * radius;

                verts[vertTrackID] = startPos + (Vector3.down * heightIncrement * i);
                vertTrackID++;
            }

            return vertTrackID;
        }

        int GenerateBorderEdge(ref Vector3[] _verts, int baseID, float _radius,
            out FaceVertexArrayInfo edgeInfo, float inset = 0)
        {
            Vector3 c1, c2, c3, c4;
            GetCorners(out c1, out c2, out c3, out c4, inset);

            int upBaseID, downBaseID, leftBaseID, rightBaseID;

            baseID = GetGridEdgeVerts(ref _verts, baseID, out upBaseID,
                out downBaseID, out leftBaseID, out rightBaseID, inset);

            int upperLeftCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c4, c1, c2, _radius);

            int lowerLeftCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c1, c2, c3, _radius);

            int lowerRightCornerBaseID = baseID;
            baseID = GetCornerFanVerts(ref _verts, baseID, c2, c3, c4, _radius);

            int upperRightCornerBaseID = baseID; // actually upper right
            baseID = GetCornerFanVerts(ref _verts, baseID, c3, c4, c1, _radius);

            edgeInfo = new FaceVertexArrayInfo
            {
                UpperEdgeBaseID = upBaseID,
                LowerEdgeBaseID = downBaseID,
                LeftEdgeBaseID = leftBaseID,
                RightEdgeBaseID = rightBaseID,
                UpperLeftCornerBaseID = upperLeftCornerBaseID,
                UpperRightCornerBaseID = upperRightCornerBaseID,
                LowerLeftCornerBaseID = lowerLeftCornerBaseID,
                LowerRightCornerBaseID = lowerRightCornerBaseID,
                InnerGridEndID = -1
            };

            return baseID;
        }

        void GenerateVColors(out Color[] vertexColors)
        {
            vertexColors = new Color[verts.Length];

            int frontStartIndex = 0;
            int frontEndIndex = frontStartIndex + GetVertexCountForFaceSide(true);

            for(int i=0; i < verts.Length; i++)
            {
                if(i >= frontStartIndex && i < frontEndIndex)
                {
                    switch (faceColorType)
                    {
                        case ColorType.FlatColor:
                            // do our front face color
                            vertexColors[i] = faceColor;
                            break;
                        case ColorType.Gradient:
                            float gradientValue = 0;
                            if(faceGradientInfo.Type == GradientType.Horizontal)
                            {
                                gradientValue = Mathf.InverseLerp(-panelDimensions.x * 0.5f, panelDimensions.x * 0.5f, verts[i].x);
                            }
                            else if (faceGradientInfo.Type == GradientType.Vertical)
                            {
                                gradientValue = Mathf.InverseLerp(-panelDimensions.y * 0.5f, panelDimensions.y * 0.5f, verts[i].y);
                            }
                            else if (faceGradientInfo.Type == GradientType.Radial)
                            {
                                gradientValue = uvs[i].magnitude;
                            }

                            if (faceGradientInfo.Invert) gradientValue = 1 - gradientValue;
                            vertexColors[i] = faceGradient.Evaluate(gradientValue);

                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // just do our border color
                    vertexColors[i] = borderColor;
                }
            }
        }

        public void GenerateVertexColors()
        {
            if(useVColors)
            {
                GenerateVColors(out vColors);
                mesh.colors = vColors;
            }
        }

        //
        // Triangles
        //

        private void GenerateTriangles(PanelInfo panelInfo, out int[] triangles)
        {
            triangles = new int[GetTotalTriangleCount() * 3];

            int trackID = 0;

            GenerateFaceTriangles(panelInfo.FrontVertInfo, true, ref trackID, ref triangles);
            if (doBackFace) GenerateFaceTriangles(panelInfo.BackVertInfo, false, ref trackID, ref triangles); // generating back faces

            switch (border)
            {
                case BorderType.Outline:
                    if (DoExtrusion)
                    {
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                        TriangulateExtrusion(panelInfo.FrontOuterVertOutlineExtrudeInfo, panelInfo.FrontVertInfo, false, ref trackID, ref triangles);
                    }
                    break;
                case BorderType.OutlineAndExtrude:
                    if (DoExtrusion)
                    {
                        // from back panel to front outline extrusion
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                        // front panel outer extrusion outline to outline forward inset
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.FrontInnerVertOutlineExtrudeInfo, false, ref trackID, ref triangles);
                        // outline forward inset to front face duplicate verts (warning: second name has been remapped)
                        TriangulateExtrusion(panelInfo.FrontInnerVertOutlineExtrudeInfo, panelInfo.FrontOuterVertOutlineExtrudeInfo, false, ref trackID, ref triangles);
                    }
                    break;
                default:
                    // generate extrusion triangles
                    if (DoExtrusion)
                    {
                        TriangulateExtrusion(panelInfo.FrontPanelExtrudeVertInfo, panelInfo.BackPanelExtrudeVertInfo, true, ref trackID, ref triangles);
                    }
                    break;
            }
        }

        private void GenerateFaceTriangles(FaceVertexArrayInfo vertInfo, bool flip, ref int trackID, ref int[] triangles)
        {
            int innerGridBase = vertInfo.InnerGridEndID - GetInnerGridVertexCount();

            // triangulate inner grid
            int numberOfQuads = InnerGridTriangleCount() / 2;
            for (int i = 0; i < numberOfQuads; i++) // dividing by two so we can iterate by quad
            {
                // we lose accuracy every loop, so we'll need a 'bump' counter that goes up
                // after every width iteration
                int bumpCounter = i / (widthSegments + 1);

                // upper left vert id = base * quad ID
                // upper right vert id = (base * quad ID) + 1

                // lower left vert = base * lower quad ID (lower quad ID is quad ID + quad grid width)
                // lower right vert = (base * lower quad ID) + 1

                // bottom left, upper left, upper right
                // bottom left, upper right, lower left

                // we need to find a way to get the next row's base id
                int upperRight = (i + 1);
                int upperLeft = i;
                int lowerLeft = (i + widthSegments + 2);
                int lowerRight = (lowerLeft + 1);

                triangles[trackID] = ((flip) ? (lowerLeft + bumpCounter) : (upperRight + bumpCounter)) + innerGridBase;
                triangles[trackID + 1] = (upperLeft + bumpCounter) + innerGridBase;
                triangles[trackID + 2] = ((flip) ? (upperRight + bumpCounter) : (lowerLeft + bumpCounter)) + innerGridBase;

                triangles[trackID + 3] = ((flip) ? (lowerLeft + bumpCounter) : (lowerRight + bumpCounter)) + innerGridBase;
                triangles[trackID + 4] = (upperRight + bumpCounter) + innerGridBase;
                triangles[trackID + 5] = ((flip) ? (lowerRight + bumpCounter) : (lowerLeft + bumpCounter)) + innerGridBase;

                trackID += 6;
            }

            #region Outer Edges
            // triangulate outer edges
            // do upper edge
            for (int i = 0; i < widthSegments + 1; i++)
            {
                int upperLeft = vertInfo.UpperEdgeBaseID + i;
                int upperRight = upperLeft + 1;
                int lowerLeft = innerGridBase + i;
                int lowerRight = innerGridBase + i + 1;

                triangles[trackID] = (flip) ? lowerLeft : upperRight;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? upperRight : lowerLeft;

                triangles[trackID + 3] = (flip) ? lowerLeft : lowerRight;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerRight : lowerLeft;

                trackID += 6;
            }

            // do lower edge
            for (int i = 0; i < widthSegments + 1; i++)
            {
                int innerGridLowEdgeBase = vertInfo.InnerGridEndID - (widthSegments + 2);

                int upperLeft = vertInfo.LowerEdgeBaseID + i;
                int upperRight = upperLeft + 1;
                int lowerLeft = innerGridLowEdgeBase + i;
                int lowerRight = innerGridLowEdgeBase + i + 1;

                triangles[trackID] = (flip) ? upperRight : lowerLeft;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? lowerLeft : upperRight;

                triangles[trackID + 3] = (flip) ? lowerRight : lowerLeft;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerLeft : lowerRight;

                trackID += 6;
            }

            // do left edge
            for (int i = 0; i < heightSegments + 1; i++)
            {
                int upperLeft = vertInfo.LeftEdgeBaseID + i;
                int upperRight = innerGridBase + MathSupplement.GetLeftEdgeForHeightIndex(0, i, widthSegments + 2);

                int lowerLeft = upperLeft + 1;
                int lowerRight = innerGridBase + MathSupplement.GetLeftEdgeForHeightIndex(0, i + 1, widthSegments + 2);

                triangles[trackID] = (flip) ? lowerLeft : upperRight;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? upperRight : lowerLeft;

                triangles[trackID + 3] = (flip) ? lowerLeft : lowerRight;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerRight : lowerLeft;

                trackID += 6;
            }

            // do right edge
            for (int i = 0; i < heightSegments + 1; i++)
            {
                int upperLeft = vertInfo.RightEdgeBaseID + i;
                int upperRight = innerGridBase + MathSupplement.GetRightEdgeForHeightIndex(0, i, widthSegments + 2);

                int lowerLeft = upperLeft + 1;
                int lowerRight = innerGridBase + MathSupplement.GetRightEdgeForHeightIndex(0, i + 1, widthSegments + 2);

                triangles[trackID] = (flip) ? upperRight : lowerLeft;
                triangles[trackID + 1] = upperLeft;
                triangles[trackID + 2] = (flip) ? lowerLeft : upperRight;

                triangles[trackID + 3] = (flip) ? lowerRight : lowerLeft;
                triangles[trackID + 4] = upperRight;
                triangles[trackID + 5] = (flip) ? lowerLeft : lowerRight;

                trackID += 6;
            }
            #endregion

            #region Triangulate Corner Fans
            // upper left
            TriangulateFan(vertInfo.UpperEdgeBaseID, vertInfo.LeftEdgeBaseID,
                innerGridBase, vertInfo.UpperLeftCornerBaseID, flip, ref trackID, ref triangles);

            // lower left
            TriangulateFan(
                vertInfo.LeftEdgeBaseID + heightSegments + 1,
                vertInfo.LowerEdgeBaseID,
                innerGridBase + MathSupplement.GetLeftEdgeForHeightIndex(0, heightSegments + 1, widthSegments + 2),
                vertInfo.LowerLeftCornerBaseID,
                flip,
                ref trackID,
                ref triangles);

            // upper right
            TriangulateFan(
                vertInfo.RightEdgeBaseID,
                vertInfo.UpperEdgeBaseID + widthSegments + 1,
                innerGridBase + MathSupplement.GetRightEdgeForHeightIndex(0, 0, widthSegments + 2),
                vertInfo.UpperRightCornerBaseID,
                flip,
                ref trackID,
                ref triangles);

            // lower right
            TriangulateFan(
                vertInfo.LowerEdgeBaseID + widthSegments + 1,
                vertInfo.RightEdgeBaseID + heightSegments + 1,
                innerGridBase + MathSupplement.GetRightEdgeForHeightIndex(0, heightSegments + 1, widthSegments + 2),
                vertInfo.LowerRightCornerBaseID,
                flip,
                ref trackID,
                ref triangles);
            #endregion
        }

        private void TriangulateFan(int lowEdgeID, int highEdgeID, int centerID,
            int baseID, bool flip, ref int trackID, ref int[] triangles)
        {
            int triangleCount = filletSegments - 2;

            // do our entry triangle
            triangles[trackID] = (flip) ? baseID : centerID;
            triangles[trackID + 1] = lowEdgeID;
            triangles[trackID + 2] = (flip) ? centerID : baseID;
            trackID += 3;

            // do our fan triangles
            for (int triIndx = 0; triIndx < triangleCount; triIndx++)
            {
                int lastID = (triIndx == triangleCount - 1) ? highEdgeID : (triIndx + 1) + baseID;
                triangles[trackID] = (flip) ? lastID : centerID;
                triangles[trackID + 1] = (triIndx) + baseID;
                triangles[trackID + 2] = (flip) ? centerID : lastID;
                trackID += 3;
            }
        }

        private void TriangulateExtrusion(FaceVertexArrayInfo frontVertInfo, FaceVertexArrayInfo backVertInfo,
            bool flip, ref int trackID, ref int[] triangles)
        {
            int widthQuadCount = widthSegments + 1;
            int heightQuadCount = heightSegments + 1;

            // triangulate upper edge
            for (int i = 0; i < widthQuadCount; i++)
            {
                int triA0 = frontVertInfo.UpperEdgeBaseID + (i + 1);
                int triA1 = backVertInfo.UpperEdgeBaseID + i;
                int triA2 = frontVertInfo.UpperEdgeBaseID + i;

                int triB0 = backVertInfo.UpperEdgeBaseID + i;
                int triB1 = frontVertInfo.UpperEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.UpperEdgeBaseID + (i + 1);

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate lower edge
            for (int i = 0; i < widthQuadCount; i++)
            {
                int triA0 = frontVertInfo.LowerEdgeBaseID + i;
                int triA1 = backVertInfo.LowerEdgeBaseID + i;
                int triA2 = frontVertInfo.LowerEdgeBaseID + (i + 1);

                int triB0 = backVertInfo.LowerEdgeBaseID + (i + 1);
                int triB1 = frontVertInfo.LowerEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.LowerEdgeBaseID + i;

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate left edge
            for (int i = 0; i < heightQuadCount; i++)
            {
                int triA0 = frontVertInfo.LeftEdgeBaseID + i;
                int triA1 = backVertInfo.LeftEdgeBaseID + i;
                int triA2 = frontVertInfo.LeftEdgeBaseID + (i + 1);

                int triB0 = backVertInfo.LeftEdgeBaseID + (i + 1);
                int triB1 = frontVertInfo.LeftEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.LeftEdgeBaseID + i;

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate right edge
            for (int i = 0; i < heightQuadCount; i++)
            {
                int triA0 = frontVertInfo.RightEdgeBaseID + (i + 1);
                int triA1 = backVertInfo.RightEdgeBaseID + i;
                int triA2 = frontVertInfo.RightEdgeBaseID + i;

                int triB0 = backVertInfo.RightEdgeBaseID + i;
                int triB1 = frontVertInfo.RightEdgeBaseID + (i + 1);
                int triB2 = backVertInfo.RightEdgeBaseID + (i + 1);

                triangles[trackID] = (flip) ? triA2 : triA0;
                triangles[trackID + 1] = triA1;
                triangles[trackID + 2] = (flip) ? triA0 : triA2;

                triangles[trackID + 3] = (flip) ? triB2 : triB0;
                triangles[trackID + 4] = triB1;
                triangles[trackID + 5] = (flip) ? triB0 : triB2;

                trackID += 6;
            }

            // triangulate UL fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.UpperLeftCornerBaseID, frontVertInfo.LeftEdgeBaseID, frontVertInfo.UpperEdgeBaseID,
                backVertInfo.UpperLeftCornerBaseID, backVertInfo.LeftEdgeBaseID, backVertInfo.UpperEdgeBaseID,
                ref trackID, ref triangles);

            // triangulate LL fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.LowerLeftCornerBaseID, frontVertInfo.LowerEdgeBaseID, frontVertInfo.LeftEdgeBaseID + (heightSegments + 1),
                backVertInfo.LowerLeftCornerBaseID, backVertInfo.LowerEdgeBaseID, backVertInfo.LeftEdgeBaseID + (heightSegments + 1),
                ref trackID, ref triangles);

            // triangulate UR fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.UpperRightCornerBaseID, frontVertInfo.UpperEdgeBaseID + (widthSegments + 1), frontVertInfo.RightEdgeBaseID,
                backVertInfo.UpperRightCornerBaseID, backVertInfo.UpperEdgeBaseID + (widthSegments + 1), backVertInfo.RightEdgeBaseID,
                ref trackID, ref triangles);

            // triangulate LR fan edge
            TriangulateExtrusionFan(flip,
                frontVertInfo.LowerRightCornerBaseID, frontVertInfo.RightEdgeBaseID + (heightSegments + 1), frontVertInfo.LowerEdgeBaseID + (widthSegments + 1),
                backVertInfo.LowerRightCornerBaseID, backVertInfo.RightEdgeBaseID + (heightSegments + 1), backVertInfo.LowerEdgeBaseID + (widthSegments + 1),
                ref trackID, ref triangles);
        }

        private void TriangulateExtrusionFan(bool flip,
            int frontCornerBaseID, int frontCornerHighID, int frontCornerLowID,
            int backCornerBaseID, int backCornerHighID, int backCornerLowID,
            ref int trackID, ref int[] triangles)
        {
            int fanQuadCount = filletSegments;

            if (filletSegments > 2)
            {
                // do our leading triangles
                int leadingTriA0 = frontCornerLowID;
                int leadingTriA1 = backCornerBaseID;
                int leadingTriA2 = frontCornerBaseID;

                int leadingTriB0 = backCornerBaseID;
                int leadingTriB1 = frontCornerLowID;
                int leadingTriB2 = backCornerLowID;

                triangles[trackID] = (!flip) ? leadingTriA0 : leadingTriA2;
                triangles[trackID + 1] = leadingTriA1;
                triangles[trackID + 2] = (!flip) ? leadingTriA2 : leadingTriA0;

                triangles[trackID + 3] = (!flip) ? leadingTriB0 : leadingTriB2;
                triangles[trackID + 4] = leadingTriB1;
                triangles[trackID + 5] = (!flip) ? leadingTriB2 : leadingTriB0;

                trackID += 6;

                for (int i = 0; i < fanQuadCount - 3; i++)
                {
                    int triA0 = frontCornerBaseID + i;
                    int triA1 = backCornerBaseID + i;
                    int triA2 = frontCornerBaseID + (i + 1);

                    int triB0 = backCornerBaseID + (i + 1);
                    int triB1 = frontCornerBaseID + (i + 1);
                    int triB2 = backCornerBaseID + i;

                    triangles[trackID] = (!flip) ? triA0 : triA2;
                    triangles[trackID + 1] = triA1;
                    triangles[trackID + 2] = (!flip) ? triA2 : triA0;

                    triangles[trackID + 3] = (!flip) ? triB0 : triB2;
                    triangles[trackID + 4] = triB1;
                    triangles[trackID + 5] = (!flip) ? triB2 : triB0;

                    trackID += 6;
                }

                // do our trailing triangles
                int offset = (filletSegments - 3);
                int trailingTriA0 = frontCornerBaseID + offset;
                int trailingTriA1 = backCornerBaseID + offset;
                int trailingTriA2 = frontCornerHighID;

                int trailingTriB0 = backCornerHighID;
                int trailingTriB1 = frontCornerHighID;
                int trailingTriB2 = backCornerBaseID + offset;

                triangles[trackID] = (!flip) ? trailingTriA0 : trailingTriA2;
                triangles[trackID + 1] = trailingTriA1;
                triangles[trackID + 2] = (!flip) ? trailingTriA2 : trailingTriA0;

                triangles[trackID + 3] = (!flip) ? trailingTriB0 : trailingTriB2;
                triangles[trackID + 4] = trailingTriB1;
                triangles[trackID + 5] = (!flip) ? trailingTriB2 : trailingTriB0;

                trackID += 6;
            }
            else
            {
                // custom handling for direct reference of border edges,
                // no fillet segments

                // do our leading triangles
                int leadingTriA0 = frontCornerLowID;
                int leadingTriA1 = backCornerHighID;
                int leadingTriA2 = frontCornerHighID;

                int leadingTriB0 = backCornerHighID;
                int leadingTriB1 = frontCornerLowID;
                int leadingTriB2 = backCornerLowID;

                triangles[trackID] = (!flip) ? leadingTriA0 : leadingTriA2;
                triangles[trackID + 1] = leadingTriA1;
                triangles[trackID + 2] = (!flip) ? leadingTriA2 : leadingTriA0;

                triangles[trackID + 3] = (!flip) ? leadingTriB0 : leadingTriB2;
                triangles[trackID + 4] = leadingTriB1;
                triangles[trackID + 5] = (!flip) ? leadingTriB2 : leadingTriB0;

                trackID += 6;
            }
        }

        private int InnerGridTriangleCount()
        {
            int widthQuads = widthSegments + 1;
            int heightQuads = heightSegments + 1;
            return (widthQuads * heightQuads) * 2;
        }

        private int ExtrusionEdgeTriangleCount()
        {
            int widthQuadCount = widthSegments + 1;
            int heightQuadCount = heightSegments + 1;

            int filletQuadCount = filletSegments + 2;

            int totalQuadCount = (widthQuadCount * 2) + (heightQuadCount * 2) +
                (filletQuadCount * 4);

            return (totalQuadCount * 2);
        }

        private int EdgeGridTriangleCount()
        {
            return (((widthSegments + 1) * 2) + ((heightSegments + 1) * 2)) * 2;
        }

        private int CornerFanTriangleCount()
        {
            return filletSegments - 1;
        }

        private int FaceTriangleCount()
        {
            int innerGridTriangleCount = InnerGridTriangleCount();

            int edgeGridTriangleCount = EdgeGridTriangleCount();

            int cornerFanCount = CornerFanTriangleCount();

            return innerGridTriangleCount + edgeGridTriangleCount + (cornerFanCount * 4);
        }

        private int GetTotalTriangleCount()
        {
            int faceCount = FaceTriangleCount();

            if (doBackFace) faceCount *= 2;

            int extrusionCount = 1;

            switch (border)
            {
                case BorderType.None:
                    extrusionCount = (DoExtrusion) ? 1 : 0;
                    break;
                case BorderType.Outline:
                    extrusionCount = (DoExtrusion) ? 2 : 0;
                    break;
                case BorderType.OutlineAndExtrude:
                    extrusionCount = (DoExtrusion) ? 4 : 0;
                    break;
                default:
                    break;
            }

            int extrusionTriangleCount = ExtrusionEdgeTriangleCount() * extrusionCount;

            return faceCount + extrusionTriangleCount;
        }

        private int GetTotalVertBufferSize()
        {
            int faceSide = GetVertexCountForFaceSide(true);
            int edgeSide = GetVertexCountForFaceSide(false);

            int numberOfEdgeSides = 0;

            if (border == BorderType.OutlineAndExtrude) numberOfEdgeSides = 2;
            else if (border == BorderType.Outline) numberOfEdgeSides = 2;
            if (DoExtrusion) numberOfEdgeSides += 2;

            int faceCount = (doBackFace) ? 2 : 1;

            return (faceSide * faceCount) + (edgeSide * numberOfEdgeSides);
        }

        private int GetInnerGridVertexCount()
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            return innerWidthSegments * innerHeightSements;
        }

        private int GetVertexCountForFaceSide(bool includeInnerGridVerts)
        {
            int innerWidthSegments = widthSegments + 2;
            int innerHeightSements = heightSegments + 2;

            int innerGridSize = (includeInnerGridVerts) ? GetInnerGridVertexCount() : 0;

            int outerEdgesSize = (innerWidthSegments * 2) + (innerHeightSements * 2);

            int vertexFanCount = (filletSegments - 2) * 4;

            return innerGridSize + outerEdgesSize + vertexFanCount;
        }
        #endregion

        // Update is called once per frame
        void Update()
        {
            UpdateVerts();
        }

        void UpdateVerts()
        {
            if (mesh == null)
            {
                GenerateMesh();
            }
            else
            {
                GenerateVerts(out verts, out panelInfo);
            }

            mesh.vertices = verts;
        }

        private void DrawGizmoMesh()
        {
            GenerateVerts(out verts, out panelInfo);
            GenerateTriangles(panelInfo, out tris);

            Color[] colors = { Color.green, Color.white };
            Gizmos.matrix = transform.localToWorldMatrix;

            for (int i = 0; i < tris.Length; i += 3)
            {
                Gizmos.color = (tris[i] > panelInfo.BackVertInfo.InnerGridEndID - GetInnerGridVertexCount()) ? Color.green : Color.yellow;

                int vertA, vertB, vertC;

                vertA = i;
                vertB = vertA + 1;
                vertC = vertB + 1;

                try
                {
                    Gizmos.DrawLine(verts[tris[vertA]], verts[tris[vertB]]);
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 0. Index: {0} {1}", tris[vertA], tris[vertB]));
                }

                try
                {
                    Gizmos.DrawLine(verts[tris[vertB]], verts[tris[vertC]]);
                }
                catch (System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 1. Index: {0} {1}", tris[vertB], tris[vertC]));
                }

                try
                {
                    Gizmos.DrawLine(verts[tris[vertA]], verts[tris[vertC]]);
                }
                catch(System.IndexOutOfRangeException e)
                {
                    Debug.Log(string.Format("IOOR in triangle edge 2. Index: {0} {1}", tris[vertA], tris[vertC]));
                }
            }

            float normalDisplayLength = Mathf.Min(panelDimensions.x, panelDimensions.y) * 0.2f;

            if (displayNormals && mesh != null)
            {
                for(int i=0; i < mesh.vertexCount; i++)
                {
                    Vector3 start = verts[i];
                    Vector3 direction = mesh.normals[i] * normalDisplayLength * 0.5f;

                    Gizmos.DrawLine(start, start + direction);
                }
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnDrawGizmosSelected()
        {
            if (visualizationMode == VisualizationMode.Mesh)
            {
                DrawGizmoMesh();
            }
        }
    }
}
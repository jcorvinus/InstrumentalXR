using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Schema
{
    public enum PanelType
    {
        Square,
        Fillet
    }

    public enum SpaceType
    {
        Rectilinear,
        Cylindrical,
        Spherical
    }

    [System.Serializable]
    public struct PanelSchema
    {
        public PanelType PanelType;
        public SpaceType SpaceType;
        public float SpaceCurveRadius;
        public Vector2 PanelDimensions;
        public float Depth;
        public float Radius;
        public int RadiusSegments;
        public float BorderThickness;

        public Color BorderColor;

        #region UI Controls
        public ControlSchema[] Controls;
        #endregion

        public static PanelSchema GetDefaults()
        {
            return new PanelSchema()
            {
                PanelType = PanelType.Fillet,
                SpaceType = SpaceType.Rectilinear,
                SpaceCurveRadius = 1,
                Depth = 0.0125f,
                PanelDimensions = new Vector2(0.245f, 0.125f),
                Radius = 0.03f,
                BorderThickness = 0.437f,
                RadiusSegments = 5,
                BorderColor = Color.white,
                Controls = new ControlSchema[0]
            };
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Modeling
{
    public enum ColorType
    {
        FlatColor,
        Gradient
    }

    public enum GradientType
    {
        Horizontal,
        Vertical,
        Radial
    }

    [System.Serializable]
    public struct GradientInfo
    {
        public GradientType Type;
        public bool Invert;
    }
}
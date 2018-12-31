using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Controls;

namespace Instrumental.Schema
{
    [System.Serializable]
    public struct ControlVariable
    {
        public string Name;
        public System.Type Type;
        public string Value;
    }

    /// <summary>
    /// Because Unity serialization doesn't support inheritance,
    /// we're going to dump all the variables into a single struct and
    /// let the control classes handle loading and saving
    /// </summary>
    [System.Serializable]
    public struct ControlSchema
    {
        [Header("Common Variables")]
        public ControlType Type;
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;

        [Header("Type-Specific Variables")]
        public List<ControlVariable> ControlVariables;
    }
}
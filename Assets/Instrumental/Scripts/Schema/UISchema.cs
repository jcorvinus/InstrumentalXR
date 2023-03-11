using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Schema
{
    /// <summary>
    /// Storage class for a UI description.
    /// </summary>
    [CreateAssetMenu(fileName = "UISchema", menuName ="Instrumental/UISchema")]
    public class UISchema : ScriptableObject
    {
        public const string Version = "v.0.0.0";
        public PanelSchema Panel;
        public List<ControlSchema> Controls;
    }
}
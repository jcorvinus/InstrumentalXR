using UnityEngine;
using System.Collections;

namespace Instrumental.Tweening
{
    /// <summary>
    /// Allows you to tween blend shapes.
    /// </summary>
    [AddComponentMenu("Instrumental/Tweening/Tween Blend Shapes")]
    public class TweenBlendShapes : Tweener
    {
        [Range(0, 100)]
        public float StartValue = 0;
        [Range(0, 100)]
        public float GoalValue = 1;

        private float value;
        public float Value { get { return this.value; } }

        public SkinnedMeshRenderer[] BlendObjects;

        // Use this for initialization
        void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
            base.Update();
            value = Mathf.Lerp(StartValue, GoalValue, TValue);
        }

        void LateUpdate()
        {
            foreach(SkinnedMeshRenderer skinRenderer in BlendObjects)
            {
                int count = skinRenderer.sharedMesh.blendShapeCount;
                for (int i = 0; i < count; i++) skinRenderer.SetBlendShapeWeight(i, value);
            }
        }
    }
}
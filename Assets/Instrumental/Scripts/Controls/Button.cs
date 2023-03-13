using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Modeling.ProceduralGraphics;
using Instrumental.Schema;

namespace Instrumental.Controls
{
    public class Button : UIControl
    {
		[SerializeField] ButtonModel buttonModel;
		[SerializeField] ButtonSchema buttonSchema = ButtonSchema.GetDefault();
		[SerializeField] BoxCollider boxCollider;
		[SerializeField] ButtonRuntime buttonRuntimeBehavior;

		[SerializeField] float hoverHeight = 0.03f;
		[SerializeField] float underFlow = 0.01f;

		#region Schema value accessors (Incomplete)
		public bool HasRim { get { return buttonSchema.HasRim; }
			set
			{
				if (HasRim != value) // don't do un-necessary work
				{
					buttonSchema.HasRim = value;
				}
			}
		}

		public int CornerVertCount { get { return buttonSchema.CornerVertCount; }
			set
			{
				if (CornerVertCount != value) { buttonSchema.CornerVertCount = value; RebuildMesh(); }
			}
		}

		public int WidthSliceCount { get { return buttonSchema.WidthVertCount;  }
			set { if (WidthSliceCount != value) { buttonSchema.WidthVertCount = value; RebuildMesh(); } }
		}

		public int BevelSliceCount { get { return buttonSchema.BevelSliceCount; }
			set { if (BevelSliceCount != value) { buttonSchema.BevelSliceCount = value; RebuildMesh(); } }
		}

		public float Depth { get { return buttonSchema.Depth; } 
			set { if (Depth != value) { buttonSchema.Depth = value; UpdateVertsOnly(); }  } }

		/// <summary>
		/// In round buttons, height is also radius!
		/// </summary>
		public float Height
		{
			get { return buttonSchema.Radius; }
			set
			{
				if (Height != value) { buttonSchema.Radius = value; UpdateVertsOnly(); }
			}
		}

		public float Width { get { return buttonSchema.Width; }
			set
			{
				if (Width != value) { buttonSchema.Width = value; UpdateVertsOnly(); }
			}
		}

		public float BevelRadius { get { return buttonSchema.BevelRadius; }
			set { if (BevelRadius != value) { buttonSchema.BevelRadius = value; UpdateVertsOnly(); } }
		}

		public float RimWidth { get { return buttonSchema.RimWidth; }
			set { if (RimWidth != value) { buttonSchema.RimWidth = value; UpdateVertsOnly(); } }
		}

		public float RimDepth { get { return buttonSchema.RimDepth; }
			set { if (RimDepth != value) { buttonSchema.RimDepth = value; UpdateVertsOnly(); } }
		}
		#endregion

		private void OnValidate()
		{
			buttonModel.SetNewButtonSchema(buttonSchema);

			float physDepth = (buttonSchema.Depth + (buttonSchema.Depth * buttonSchema.BevelDepth));
			float physAndHoverDepth = physDepth + (hoverHeight);
			float totalDepth = physAndHoverDepth + underFlow;// + hoverHeight + underFlow;
			boxCollider.center = new Vector3(0, 0, (physAndHoverDepth * 0.5f) - (underFlow * 0.5f));
			boxCollider.size = new Vector3(buttonSchema.Width + (buttonSchema.Radius * 2), buttonSchema.Radius * 2,
				totalDepth);

			buttonRuntimeBehavior.ButtonFaceDistance = physDepth;
			buttonRuntimeBehavior.ButtonThrowDistance = (buttonSchema.Depth * buttonSchema.RimDepth) * 0.5f; //(buttonSchema.Depth * buttonSchema.RimDepth);
		}

		public override void SetSchema(ControlSchema controlSchema)
        {
            // set things based off the schema
            transform.localPosition = controlSchema.Position;
            transform.localRotation = controlSchema.Rotation;
            _name = controlSchema.Name;

			buttonSchema = ButtonSchema.CreateFromControl(controlSchema);
        }

		protected override void Awake()
        {
            _name = "Button";

			buttonRuntimeBehavior = GetComponent<ButtonRuntime>();

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        public override ControlSchema GetSchema()
        {
            ControlSchema schema = new ControlSchema()
            {
                Name = _name,
                Position = transform.localPosition,
                Rotation = transform.localRotation,
                Type = GetControlType()
            };

			buttonSchema.SetControlSchema(ref schema);

            return schema;
        }

		#region Meshing
		void RebuildMesh()
		{

		}

		void UpdateVertsOnly()
		{
			
		}
		#endregion

		public override ControlType GetControlType()
        {
            return ControlType.Button;
        }
    }
}
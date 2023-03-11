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
		[SerializeField] ButtonSchema buttonSchema;

		// todo: handle all of our proc gen graphics and stuff
		ButtonUnityGraphic buttonGraphic;

		// accessors for schema variables
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
				if (value != CornerVertCount) { buttonSchema.CornerVertCount = value; RebuildMesh(); }
			}
		}

		public float Width { get { return buttonSchema.Width; }
			set
			{
				if (Width != value) { buttonSchema.Width = value; UpdateVertsOnly(); }
			}
		}

		public float Height { get { return buttonSchema.Radius; }
			set
			{
				if (Height != value) { buttonSchema.Radius = value; UpdateVertsOnly(); }
			}
		}

		public float Depth { get { return buttonSchema.Depth; } }

		public int WidthSegments { get { return buttonSchema.WidthVertCount; }
			set
			{
				if (WidthSegments != value) { buttonSchema.WidthVertCount = value; RebuildMesh(); }
			}
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
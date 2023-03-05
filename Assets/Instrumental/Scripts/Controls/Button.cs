using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Instrumental.Schema;

namespace Instrumental.Controls
{
    public class Button : UIControl
    {
        GameObject physicsObject;
        GameObject rimObject;

		ButtonSchema buttonSchema;

		// todo: handle all of our proc gen graphics and stuff
		MeshRenderer faceModel;
		MeshCollider faceModelCollider;
		Mesh faceMesh;

		MeshRenderer rimModel;
		MeshCollider rimModelCollider;
		Mesh rimMesh;

		// accessors for schema variables
		public bool HasRim { get { return buttonSchema.HasRim; }
			set
			{
				if (HasRim != value) // don't do un-necessary work
				{
					buttonSchema.HasRim = value; rimObject.gameObject.SetActive(HasRim);
				}
			}
		}

		public int RadialSegments { get { return buttonSchema.RadialSegments; }
			set
			{
				if (value != RadialSegments) { buttonSchema.RadialSegments = value; RebuildMesh(); }
			}
		}

		public float Width { get { return buttonSchema.Width; }
			set
			{
				if (Width != value) { buttonSchema.Width = value; UpdateVertsOnly(); }
			}
		}

		public float Height { get { return buttonSchema.Height; }
			set
			{
				if (Height != value) { buttonSchema.Height = value; UpdateVertsOnly(); }
			}
		}

		public float Depth { get { return buttonSchema.Depth; } }

		public int WidthSegments { get { return buttonSchema.WidthSegments; }
			set
			{
				if (WidthSegments != value) { buttonSchema.WidthSegments = value; RebuildMesh(); }
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

            physicsObject = transform.Find("Physics").gameObject;
            rimObject = transform.Find("Rim").gameObject;

			// also get our graphics so we can do hover animations
			faceModel = physicsObject.GetComponentInChildren<MeshRenderer>();
			rimModel = rimObject.GetComponent<MeshRenderer>();

			faceModelCollider = faceModel.GetComponent<MeshCollider>();
			rimModelCollider = rimModel.GetComponent<MeshCollider>();

			faceMesh = new Mesh();
			faceMesh.MarkDynamic();

			rimMesh = new Mesh();
			rimMesh.MarkDynamic();
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
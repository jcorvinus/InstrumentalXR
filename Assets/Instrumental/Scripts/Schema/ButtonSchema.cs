using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Schema
{
	[System.Serializable]
	public struct ButtonSchema
	{
		#region Constraint Defines
		public const int MIN_CORNER_VERT_COUNT = 2;
		public const int MAX_CORNER_VERT_COUNT = 8;

		public const int MIN_WIDTH_VERT_COUNT = 0;
		public const int MAX_WIDTH_VERT_COUNT = 8;

		public const int MIN_BEVEL_SLICE_COUNT = 0;
		public const int MAX_BEVEL_SLICE_COUNT = 3;

		public const float MIN_EXTRUSION_DEPTH = 0;
		public const float MAX_EXTRUSION_DEPTH = 1;

		public const float MIN_RADIUS = 0;
		public const float MAX_RADIUS = 1;

		public const float MIN_BEVEL_RADIUS_PERCENT = 0;
		public const float MAX_BEVEL_RADIUS_PERCENT = 1;

		public const float MIN_BEVEL_EXTRUSION_PERCENT = 0;
		public const float MAX_BEVEL_EXTRUSION_PERCENT = 1;

		public const float MIN_RIM_WIDTH_PERCENT = 0;
		public const float MAX_RIM_WIDTH_PERCENT = 1;

		public const float MIN_RIM_DEPTH_PERCENT = 0;
		public const float MAX_RIM_DEPTH_PERCENT = 1;
		#endregion

		// what kind of button options do we want to allow?

		// rim or not rimmed?
		public bool HasRim;

		// number of radius slices
		[Range(MIN_CORNER_VERT_COUNT, MAX_CORNER_VERT_COUNT)]
		public int CornerVertCount;

		[Range(MIN_WIDTH_VERT_COUNT, MAX_WIDTH_VERT_COUNT)]
		public int WidthVertCount;

		[Range(MIN_BEVEL_SLICE_COUNT, MAX_BEVEL_SLICE_COUNT)]
		public int BevelSliceCount;

		// button mesh depth
		[Range(MIN_EXTRUSION_DEPTH, MAX_EXTRUSION_DEPTH)]
		public float Depth;

		// button height
		[Range(MIN_RADIUS, MAX_RADIUS)]
		public float Radius;

		// square or round?

		// width
		[Range(0, 0.1f)]
		public float Width;

		[Range(MIN_BEVEL_RADIUS_PERCENT, MAX_BEVEL_RADIUS_PERCENT)]
		public float BevelRadius;

		[Range(MIN_BEVEL_EXTRUSION_PERCENT, MAX_BEVEL_EXTRUSION_PERCENT)]
		public float BevelDepth;

		[Range(MIN_RIM_WIDTH_PERCENT, MAX_RIM_WIDTH_PERCENT)]
		public float RimWidth;

		[Range(MIN_RIM_DEPTH_PERCENT, MAX_RIM_DEPTH_PERCENT)]
		public float RimDepth;

		// button throw distance
		public float ThrowDistance; // throw distance and height are good candidates
				// for fixed values instead of variable

		public static ButtonSchema CreateFromControl(ControlSchema control)
		{
			#region HasRim
			bool hasRim = true;

			// when convenient (ie good internet connection and time to kill),
			// figure out if this can be used to reduce code usage.
			/*Predicate<ControlVariable> hasRimPredicate = (item => 
				item.Name == "HasRim" && item.Type == typeof(bool));
			Func<ControlVariable, bool> hasRimExitFunc = new Func<ControlVariable, bool>(hasRimPredicate);*/

			if (control.ControlVariables.Any(item => item.Name == "HasRim" &&
			item.Type == typeof(bool)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "HasRim" && item.Type == typeof(bool));

				if(!bool.TryParse(controlVariable.Value, out hasRim))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region radial segments
			int cornerVertCount = 4;

			if(control.ControlVariables.Any(item => item.Name == "RadialSegments" &&
				item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => 
				item.Name == "RadialSegments" && item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out cornerVertCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Width
			float width = 0.1f;

			if(control.ControlVariables.Any(item => item.Name == "Width" && 
				item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
				item.Name == "Width" && item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out width))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Height
			float height = 0.1f;

			if(control.ControlVariables.Any(item => item.Name == "Height" && item.Type == typeof(float)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => item.Name == "Height" &&
				item.Type == typeof(float));

				if(!float.TryParse(controlVariable.Value, out height))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Depth
			float depth = 0.02f;
			#endregion

			#region Width Segments
			int widthVertCount = 4;

			if(control.ControlVariables.Any(item => item.Name == "WidthVertCount" && item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
					item.Name == "WidthVertCount" && item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out widthVertCount))
				{
					DoVariableError(control, controlVariable);
				}
			}
			#endregion

			#region Throw Distance
			float throwDistance = 0.06f;
			#endregion

			ButtonSchema button = new ButtonSchema
			{
				HasRim = hasRim,
				Radius = height,
				Width = width,
				Depth = depth,
				CornerVertCount = cornerVertCount,
				ThrowDistance = throwDistance,
				WidthVertCount = widthVertCount
			};

			return button;
		}

		public static ButtonSchema GetDefault()
		{
			return new ButtonSchema()
			{
				HasRim = true,
				CornerVertCount = 5, 
				WidthVertCount = 0,
				BevelSliceCount = 3,
				Depth = 0.017f,
				Radius = 0.022f,
				Width = 0.0011f,
				BevelRadius = 0.697f,
				BevelDepth = 0.246f,
				RimWidth = 0.106f,
				RimDepth = 0.25f
			};
		}

		private static void DoVariableError(ControlSchema control, ControlVariable variable)
		{
			Debug.LogWarning(string.Format("There was a problem with {0}'s {1} variable. It did not parse to {2} properly.",
				control.Name, variable.Name, variable.Type.ToString()));
		}

		public void SetControlSchema(ref ControlSchema control)
		{
			// set all of our proeprties back into the control
			#region HasRim
			Func<ControlVariable, bool> hasRimFunc = new Func<ControlVariable, bool>(item => 
				item.Name == "HasRim" && item.Type == typeof(bool));
			if (control.ControlVariables.Any(hasRimFunc))
			{
				// modify existing control variable
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(hasRimFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = HasRim.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				//create control variable
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "HasRim",
					Type = typeof(bool),
					Value = HasRim.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			#region Radial Segments
			Func<ControlVariable, bool> cornerVertCountFunc = new Func<ControlVariable, bool>(item => 
				item.Name == "CornerVertCount" && item.Type == typeof(int));

			if(control.ControlVariables.Any(cornerVertCountFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(cornerVertCountFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = CornerVertCount.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "CornerVertCount",
					Type = typeof(int),
					Value = CornerVertCount.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			#region Width
			Func<ControlVariable, bool> widthFunc = new Func<ControlVariable, bool>(item =>
				item.Name == "Width" && item.Type == typeof(float));

			if(control.ControlVariables.Any(widthFunc))
			{
				// modify existing varaible
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(widthFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = Width.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				// create new one
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "Width",
					Type = typeof(int),
					Value = Width.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			#region Height
			Func<ControlVariable, bool> heightFunc = new Func<ControlVariable, bool>(item =>
				item.Name == "Radius" && item.Type == typeof(float));

			if(control.ControlVariables.Any(heightFunc))
			{
				// modify existing varaible
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(heightFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = Radius.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				// create new one
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "Radius",
					Type = typeof(int),
					Value = Radius.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			// don't do depth, it's a hardcoded value

			#region Width Segments
			Func<ControlVariable, bool> widthSegmentsFunc = new Func<ControlVariable, bool>(item =>
				item.Name == "WidthSegments" && item.Type == typeof(int));

			if (control.ControlVariables.Any(widthSegmentsFunc))
			{
				// modify existing varaible
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(widthSegmentsFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = WidthVertCount.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				// create new one
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "WidthSegments",
					Type = typeof(int),
					Value = WidthVertCount.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			// don't do throw distance, it's a hardcoded value
		}
	}
}
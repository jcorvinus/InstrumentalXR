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
		// what kind of button options do we want to allow?

		// rim or not rimmed?
		public bool HasRim;

		// number of radius slices
		public int RadialSegments;

		// square or round?

		// width
		public float Width;

		// button height
		public float Height;

		// button mesh depth
		public float Depth;

		// number of segments?
		public int WidthSegments;

		// button throw distance
		public float ThrowDistance; // throw distance and height are good candidates
				// for fixed values instead of variable

		// InteractionButton or PreciseButton?

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
			int radialSegments = 4;

			if(control.ControlVariables.Any(item => item.Name == "RadialSegments" &&
				item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item => 
				item.Name == "RadialSegments" && item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out radialSegments))
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
			int widthSegments = 4;

			if(control.ControlVariables.Any(item => item.Name == "WidthSegments" &&	item.Type == typeof(int)))
			{
				ControlVariable controlVariable = control.ControlVariables.First(item =>
					item.Name == "WidthSegments" && item.Type == typeof(int));

				if(!int.TryParse(controlVariable.Value, out widthSegments))
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
				Height = height,
				Width = width,
				Depth = depth,
				RadialSegments = radialSegments,
				ThrowDistance = throwDistance,
				WidthSegments = widthSegments
			};

			return button;
		}

		public static ButtonSchema GetDefault()
		{
			return new ButtonSchema()
			{
				HasRim = true,
				WidthSegments = 4,
				Height = 0.02f,
				RadialSegments = 4,
				ThrowDistance = 0.06f,
				Width = 0.2f
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
			Func<ControlVariable, bool> radialSegmentsFunc = new Func<ControlVariable, bool>(item => 
				item.Name == "RadialSegments" && item.Type == typeof(int));

			if(control.ControlVariables.Any(radialSegmentsFunc))
			{
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(radialSegmentsFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = RadialSegments.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "RadialSegments",
					Type = typeof(int),
					Value = RadialSegments.ToString()
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
				item.Name == "Height" && item.Type == typeof(float));

			if(control.ControlVariables.Any(heightFunc))
			{
				// modify existing varaible
				int indexOf = control.ControlVariables.IndexOf(
					control.ControlVariables.First(heightFunc));

				ControlVariable controlVariable = control.ControlVariables[indexOf];

				controlVariable.Value = Height.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				// create new one
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "Height",
					Type = typeof(int),
					Value = Height.ToString()
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

				controlVariable.Value = WidthSegments.ToString();

				control.ControlVariables[indexOf] = controlVariable;
			}
			else
			{
				// create new one
				ControlVariable controlVariable = new ControlVariable()
				{
					Name = "WidthSegments",
					Type = typeof(int),
					Value = WidthSegments.ToString()
				};

				control.ControlVariables.Add(controlVariable);
			}
			#endregion

			// don't do throw distance, it's a hardcoded value
		}
	}
}
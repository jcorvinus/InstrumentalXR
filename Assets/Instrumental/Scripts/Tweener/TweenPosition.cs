using UnityEngine;
using System.Collections;

namespace Instrumental.Tweening
{
	[AddComponentMenu("Instrumental/Tweening/Tween Positon")]
	public class TweenPosition : Tweener
	{
		public Vector3 StartPosition;
		public Vector3 GoalPosition;
		public bool IsPositionLocal = true;

		void Start()
		{
			base.Start();
		}

		void Update()
		{
			base.Update();

			if (IsPositionLocal) transform.localPosition = Vector3.Lerp(StartPosition, GoalPosition, TValue);
			else transform.position = Vector3.Lerp(StartPosition, GoalPosition, TValue);
		}
	}
}
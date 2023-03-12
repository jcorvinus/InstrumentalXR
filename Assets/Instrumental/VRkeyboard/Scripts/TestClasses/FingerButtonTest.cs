using UnityEngine;
using System.Collections;

namespace Instrumental.Controls
{
	[RequireComponent(typeof(ButtonRuntime))]
	public class ButtonRuntimeTest : MonoBehaviour
	{
		private ButtonRuntime button;

		[Header("Variables")]
		public float ThrowValue;
		public bool ApplyThrow = false;

		[Header("Commands")]
		public bool Activate = false;
		public bool Hover = false;

		// Use this for initialization
		void Awake()
		{
			button = GetComponent<ButtonRuntime>();
		}

		// Update is called once per frame
		void Update()
		{
			if (ApplyThrow)
			{
				button.CurrentThrowValue = ThrowValue;
			}

			if (Activate)
			{
				button.Activate();
				Activate = false;
			}

			if (Hover)
			{
				button.Hover();
				Hover = false;
			}
		}
	}
}
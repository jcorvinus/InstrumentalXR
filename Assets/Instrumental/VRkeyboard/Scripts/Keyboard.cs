using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine.UI;

using CatchCo;

using Instrumental.Controls;

namespace VRKeyboard
{
    /// <summary>
    /// A virtual keyboard for providing text input.
    /// </summary>
    public class Keyboard : MonoBehaviour
    {
        public delegate void KeyboardModeHandler(Keyboard sender, bool normalMode);
        public event KeyboardModeHandler ModeChange;

        public UnityEvent OnReturn;

        [System.Serializable]
        public struct KeyInfo
        {
            public KeyCode[] KeyModes;
            public KeyButton Button;

            private ButtonRuntime fingerButton;
            private Collider _collider; // hackjob
            private GameObject faceIcon;

            public ButtonRuntime FButton { get { return fingerButton; }  set { fingerButton = value; } }
            public Collider collider { get { return _collider; } set { _collider = value; } } 
            public GameObject FaceIcon { get { return faceIcon; } set { faceIcon = value; } }
        }

		[SerializeField] Transform output;

		#region KeyInfo
		[Header("Key Info")]
        [SerializeField] KeyInfo[] topRow;
        [SerializeField] KeyInfo[] midRow;
        [SerializeField] KeyInfo[] bottomRow;
        [SerializeField] KeyInfo[] modifiers;

        [SerializeField] KeyInfo returnButton;
        [SerializeField] Image returnImage;

        [SerializeField] KeyInfo caseButton;
        [SerializeField] Image caseImage;

        [SerializeField] KeyInfo symbolButton;
        [SerializeField] GameObject symbolImage;

        [SerializeField] KeyInfo backspaceButton;
        [SerializeField] GameObject backspaceFace;
		#endregion

		[Header("Scaling Variables")]
		[SerializeField] AnimationCurve xScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField] AnimationCurve yScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField] AnimationCurve zScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        Coroutine showCoroutine;
        Coroutine hideCoroutine;

        int currentMode = 0;

        bool visible = false;

        private StringBuilder stringBuilder;

        [SerializeField] UnityEngine.UI.InputField textInput;

        public UnityEngine.UI.InputField TextInput { get { return textInput; } }
		public Transform Output { get { return output; } }

        public bool Visible { get { return visible; } }

        public Text CharacterPrefabText; // duplicate this for each button, anchor it to the button face.

        [Header("Audio Tuning")]
        float volumeAdjust = 1;
        float timeSinceLastKey = float.PositiveInfinity;
        [SerializeField] float volumeKeyTimeReset = 1.5f;
        [SerializeField] float timeTillMinVolume = 4f;
        [Range(0.3f, 0.9f)]
        [SerializeField] float minVolume;
        float timeSinceLastReset;

        public float VolumeAdjust { get { return volumeAdjust; } }    

        [Header("Debug Variables")]
        public string Contents = "";

        /// <summary> List of keys that cannot be converted straight to a character.</summary>
        private static KeyCode[] NonCharKeys =
        {
        #region KeyArray
        KeyCode.Backspace,
        KeyCode.Break,
        KeyCode.CapsLock,
        KeyCode.Delete,
        KeyCode.DownArrow,
        KeyCode.End,
        KeyCode.Escape,
        KeyCode.F1,
        KeyCode.F2,
        KeyCode.F3,
        KeyCode.F4,
        KeyCode.F5,
        KeyCode.F6,
        KeyCode.F7,
        KeyCode.F8,
        KeyCode.F9,
        KeyCode.F10,
        KeyCode.F11,
        KeyCode.F12,
        KeyCode.Home,
        KeyCode.Insert,
        KeyCode.JoystickButton0,
        KeyCode.JoystickButton1,
        KeyCode.JoystickButton2,
        KeyCode.JoystickButton3,
        KeyCode.JoystickButton4,
        KeyCode.JoystickButton5,
        KeyCode.JoystickButton6,
        KeyCode.JoystickButton7,
        KeyCode.JoystickButton8,
        KeyCode.JoystickButton9,
        KeyCode.JoystickButton10,
        KeyCode.JoystickButton11,
        KeyCode.JoystickButton12,
        KeyCode.JoystickButton13,
        KeyCode.JoystickButton14,
        KeyCode.JoystickButton15,
        KeyCode.JoystickButton16,
        KeyCode.JoystickButton17,
        KeyCode.JoystickButton18,
        KeyCode.JoystickButton19,
        KeyCode.Joystick1Button0,
        KeyCode.Joystick1Button1,
        KeyCode.Joystick1Button2,
        KeyCode.Joystick1Button3,
        KeyCode.Joystick1Button4,
        KeyCode.Joystick1Button5,
        KeyCode.Joystick1Button6,
        KeyCode.Joystick1Button7,
        KeyCode.Joystick1Button8,
        KeyCode.Joystick1Button9,
        KeyCode.Joystick1Button10,
        KeyCode.Joystick1Button11,
        KeyCode.Joystick1Button12,
        KeyCode.Joystick1Button13,
        KeyCode.Joystick1Button14,
        KeyCode.Joystick1Button15,
        KeyCode.Joystick1Button16,
        KeyCode.Joystick1Button17,
        KeyCode.Joystick1Button18,
        KeyCode.Joystick1Button19,
        KeyCode.Joystick2Button0,
        KeyCode.Joystick2Button1,
        KeyCode.Joystick2Button2,
        KeyCode.Joystick2Button3,
        KeyCode.Joystick2Button4,
        KeyCode.Joystick2Button5,
        KeyCode.Joystick2Button6,
        KeyCode.Joystick2Button7,
        KeyCode.Joystick2Button8,
        KeyCode.Joystick2Button9,
        KeyCode.Joystick2Button10,
        KeyCode.Joystick2Button11,
        KeyCode.Joystick2Button12,
        KeyCode.Joystick2Button13,
        KeyCode.Joystick2Button14,
        KeyCode.Joystick2Button15,
        KeyCode.Joystick2Button16,
        KeyCode.Joystick2Button17,
        KeyCode.Joystick2Button18,
        KeyCode.Joystick2Button19,
        KeyCode.Joystick3Button0,
        KeyCode.Joystick3Button1,
        KeyCode.Joystick3Button2,
        KeyCode.Joystick3Button3,
        KeyCode.Joystick3Button4,
        KeyCode.Joystick3Button5,
        KeyCode.Joystick3Button6,
        KeyCode.Joystick3Button7,
        KeyCode.Joystick3Button8,
        KeyCode.Joystick3Button9,
        KeyCode.Joystick3Button10,
        KeyCode.Joystick3Button11,
        KeyCode.Joystick3Button12,
        KeyCode.Joystick3Button13,
        KeyCode.Joystick3Button14,
        KeyCode.Joystick3Button15,
        KeyCode.Joystick3Button16,
        KeyCode.Joystick3Button17,
        KeyCode.Joystick3Button18,
        KeyCode.Joystick3Button19,
        KeyCode.Joystick4Button0,
        KeyCode.Joystick4Button1,
        KeyCode.Joystick4Button2,
        KeyCode.Joystick4Button3,
        KeyCode.Joystick4Button4,
        KeyCode.Joystick4Button5,
        KeyCode.Joystick4Button6,
        KeyCode.Joystick4Button7,
        KeyCode.Joystick4Button8,
        KeyCode.Joystick4Button9,
        KeyCode.Joystick4Button10,
        KeyCode.Joystick4Button11,
        KeyCode.Joystick4Button12,
        KeyCode.Joystick4Button13,
        KeyCode.Joystick4Button14,
        KeyCode.Joystick4Button15,
        KeyCode.Joystick4Button16,
        KeyCode.Joystick4Button17,
        KeyCode.Joystick4Button18,
        KeyCode.Joystick4Button19,
        KeyCode.Joystick5Button0,
        KeyCode.Joystick5Button1,
        KeyCode.Joystick5Button2,
        KeyCode.Joystick5Button3,
        KeyCode.Joystick5Button4,
        KeyCode.Joystick5Button5,
        KeyCode.Joystick5Button6,
        KeyCode.Joystick5Button7,
        KeyCode.Joystick5Button8,
        KeyCode.Joystick5Button9,
        KeyCode.Joystick5Button10,
        KeyCode.Joystick5Button11,
        KeyCode.Joystick5Button12,
        KeyCode.Joystick5Button13,
        KeyCode.Joystick5Button14,
        KeyCode.Joystick5Button15,
        KeyCode.Joystick5Button16,
        KeyCode.Joystick5Button17,
        KeyCode.Joystick5Button18,
        KeyCode.Joystick5Button19,
        KeyCode.Joystick6Button0,
        KeyCode.Joystick6Button1,
        KeyCode.Joystick6Button2,
        KeyCode.Joystick6Button3,
        KeyCode.Joystick6Button4,
        KeyCode.Joystick6Button5,
        KeyCode.Joystick6Button6,
        KeyCode.Joystick6Button7,
        KeyCode.Joystick6Button8,
        KeyCode.Joystick6Button9,
        KeyCode.Joystick6Button10,
        KeyCode.Joystick6Button11,
        KeyCode.Joystick6Button12,
        KeyCode.Joystick6Button13,
        KeyCode.Joystick6Button14,
        KeyCode.Joystick6Button15,
        KeyCode.Joystick6Button16,
        KeyCode.Joystick6Button17,
        KeyCode.Joystick6Button18,
        KeyCode.Joystick6Button19,
        KeyCode.Joystick7Button0,
        KeyCode.Joystick7Button1,
        KeyCode.Joystick7Button2,
        KeyCode.Joystick7Button3,
        KeyCode.Joystick7Button4,
        KeyCode.Joystick7Button5,
        KeyCode.Joystick7Button6,
        KeyCode.Joystick7Button7,
        KeyCode.Joystick7Button8,
        KeyCode.Joystick7Button9,
        KeyCode.Joystick7Button10,
        KeyCode.Joystick7Button11,
        KeyCode.Joystick7Button12,
        KeyCode.Joystick7Button13,
        KeyCode.Joystick7Button14,
        KeyCode.Joystick7Button15,
        KeyCode.Joystick7Button16,
        KeyCode.Joystick7Button17,
        KeyCode.Joystick7Button18,
        KeyCode.Joystick7Button19,
        KeyCode.Joystick8Button0,
        KeyCode.Joystick8Button1,
        KeyCode.Joystick8Button2,
        KeyCode.Joystick8Button3,
        KeyCode.Joystick8Button4,
        KeyCode.Joystick8Button5,
        KeyCode.Joystick8Button6,
        KeyCode.Joystick8Button7,
        KeyCode.Joystick8Button8,
        KeyCode.Joystick8Button9,
        KeyCode.Joystick8Button10,
        KeyCode.Joystick8Button11,
        KeyCode.Joystick8Button12,
        KeyCode.Joystick8Button13,
        KeyCode.Joystick8Button14,
        KeyCode.Joystick8Button15,
        KeyCode.Joystick8Button16,
        KeyCode.Joystick8Button17,
        KeyCode.Joystick8Button18,
        KeyCode.Joystick8Button19,
        KeyCode.KeypadEnter,
        KeyCode.LeftAlt,
        KeyCode.LeftApple,
        KeyCode.LeftArrow,
        KeyCode.LeftCommand,
        KeyCode.LeftControl,
        KeyCode.LeftShift,
        KeyCode.LeftWindows,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.Mouse2,
        KeyCode.Mouse3,
        KeyCode.Mouse4,
        KeyCode.Mouse5,
        KeyCode.Mouse6,
        KeyCode.None,
        KeyCode.Numlock,
        KeyCode.PageDown,
        KeyCode.PageUp,
        KeyCode.Pause,
        KeyCode.Print,
        KeyCode.Return,
        KeyCode.RightAlt,
        KeyCode.RightApple,
        KeyCode.RightArrow,
        KeyCode.RightCommand,
        KeyCode.RightControl,
        KeyCode.RightShift,
        KeyCode.RightWindows,
        KeyCode.ScrollLock,
        KeyCode.Space,
        KeyCode.SysReq,
        KeyCode.Tab,
        KeyCode.UpArrow
        #endregion
        };

        #region Initialization
        void Awake()
        {
            stringBuilder = new StringBuilder();

            // populate the button faces with text component

            ButtonInit(topRow, true);
            ButtonInit(midRow, true);
            ButtonInit(bottomRow, true);
            ButtonInit(modifiers, true);

            returnButton = ButtonInit(returnButton, false);
            returnButton.FaceIcon = returnImage.gameObject;
            returnImage.gameObject.SetActive(false);

            caseButton = ButtonInit(caseButton, false);
            caseButton.FaceIcon = caseImage.gameObject;
            caseImage.gameObject.SetActive(false);

            symbolButton = ButtonInit(symbolButton, false);
            symbolButton.FaceIcon = symbolImage.gameObject;
            symbolImage.SetActive(false);

            backspaceButton = ButtonInit(backspaceButton, false);
            backspaceButton.FaceIcon = backspaceFace;
            backspaceFace.SetActive(false);

            symbolButton.FButton.ButtonActivated += ModeButton_ButtonActivated;

            //textInput.ActivateInputField();
        }

		private void Start()
		{
			transform.localScale = Vector3.zero;
		}

		private void ModeButton_ButtonActivated(ButtonRuntime sender)
        {
            ChangeMode();
        }

        void ButtonInit(KeyInfo[] buttons, bool textFace)
        {
            for(int i=0; i < buttons.Length; i++)
            {
                buttons[i] = ButtonInit(buttons[i], textFace);
				buttons[i].Button.gameObject.SetActive(false);
            }
        }

        KeyInfo ButtonInit(KeyInfo button, bool textFace)
        {
            button.FButton = button.Button.GetComponent<ButtonRuntime>();
            if (textFace)
            {
                Transform buttonFace = button.FButton.ButtonFace;
                GameObject newTextObject = GameObject.Instantiate(CharacterPrefabText.gameObject, CharacterPrefabText.transform.parent);

                TransformAnchor textAnchor = newTextObject.gameObject.AddComponent<TransformAnchor>();
				textAnchor.Offset = new Vector3(0, 0.012f, 0);
				textAnchor.Anchor = buttonFace;
				textAnchor.AnchorForward = Vector3.down;
				textAnchor.AnchorUp = Vector3.forward;

				button.Button.CharacterText = newTextObject.GetComponent<Text>();

                button.FaceIcon = newTextObject;
            }

            button.collider = button.FButton.GetComponent<Collider>();
            if(button.KeyModes.Length > 0) button.Button.Key = button.KeyModes[0];

            AddKeyEvents(button);

            return button;
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
			if (visible)
			{
				DoVolumeAdjustments();
			}
        }

        void DoVolumeAdjustments()
        {
            if(timeSinceLastKey != float.PositiveInfinity) timeSinceLastKey += Time.deltaTime;

            timeSinceLastReset += Time.deltaTime;

            if(timeSinceLastKey > volumeKeyTimeReset)
            {
                timeSinceLastReset = 0;
            }

            volumeAdjust = Mathf.Lerp(minVolume, 1, 1 - Mathf.InverseLerp(0, timeTillMinVolume, timeSinceLastReset));
        }

        [ExposeMethodInEditor]
        void DPressKeyTimer()
        {
            timeSinceLastKey = 0;
        }

        public void AddKeyEvents(KeyInfo button)
        {
            button.Button.Activated += button_Activated;
        }

        public void RemoveKeyEvents(KeyButton button)
        {
            button.Activated -= button_Activated;
        }

        void ChangeCase()
        {
            // change all case flags in buttons
            ChangeCase(topRow);
            ChangeCase(midRow);
            ChangeCase(bottomRow);
        }

        void ChangeCase(KeyInfo[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Button.UpperCase =
                    !buttons[i].Button.UpperCase;
            }
        }

        void ChangeMode()
        {
            currentMode++;

            Debug.Log("Changing mode.");

            ChangeMode(topRow);
            ChangeMode(midRow);
            ChangeMode(bottomRow);
        }

        void ChangeMode(KeyInfo[] keyInfo)
        {
            for (int i = 0; i < keyInfo.Length; i++) ChangeMode(keyInfo[i]);
        }

        void ChangeMode(KeyInfo keyInfo)
        {
            int newMode = (int)Mathf.Repeat(currentMode, keyInfo.KeyModes.Length);

            keyInfo.Button.Key = keyInfo.KeyModes[newMode];

            Debug.Log("new mode:" + newMode);
        }

        public string GetText()
        {
            return textInput.text;
        }

        public void ClearText()
        {
            textInput.text = "";
            textInput.selectionAnchorPosition = 0;
        }

		[ExposeMethodInEditor]
        public void Show()
        {
			gameObject.SetActive(true);
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            showCoroutine = StartCoroutine(ShowCoroutine());
        }

		void SetButtonEnable(KeyInfo key, bool enable)
		{
			key.Button.gameObject.SetActive(enable);
			key.FaceIcon.gameObject.SetActive(enable);
		}

		void SetButtonRowEnable(KeyInfo[] row, bool enable)
		{
			foreach(KeyInfo key in row)
			{
				SetButtonEnable(key, enable);
			}
		}

		public void LookAtPoint(Vector3 point, Vector3 up)
		{
			transform.LookAt(point, up);
			transform.Rotate(new Vector3(0, 180, 0), Space.Self);
			transform.Rotate(new Vector3(-15, 0, 0), Space.Self);
		}

		IEnumerator ShowCoroutine()
        {
			textInput.enabled = false;

			textInput.gameObject.transform.parent.gameObject.SetActive(true);

			// enable all of our components!
			SetButtonRowEnable(topRow, true);
			SetButtonRowEnable(midRow, true);
			SetButtonRowEnable(bottomRow, true);
			SetButtonEnable(returnButton, true);
			SetButtonEnable(caseButton, true);
			SetButtonEnable(symbolButton, true);
			SetButtonEnable(backspaceButton, true);

			float time = 0;
			float duration = 0.25f;

			while(time < duration)
			{
				float tValue = Mathf.InverseLerp(0, duration, time);

				Vector3 newScale = new Vector3(
					Mathf.LerpUnclamped(0, 1, xScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, yScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, zScaleCurve.Evaluate(tValue)));

				transform.localScale = newScale;

				time += Time.deltaTime;
				yield return null;
			}

			transform.localScale = Vector3.one;

			yield return null;
			textInput.enabled = true;

            visible = true;

			/*textInput.text = "";
			textInput.ActivateInputField();*/ // can't focus input field because of this.

			yield break;
        }

        [ExposeMethodInEditor]
        public void Hide()
        {
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }

            hideCoroutine = StartCoroutine(HideCoroutine());
        }

        IEnumerator HideCoroutine()
        {
			float time = 0;
			float duration = 0.25f;

			while (time < duration)
			{
				float tValue = 1 - Mathf.InverseLerp(0, duration, time);

				Vector3 newScale = new Vector3(
					Mathf.LerpUnclamped(0, 1, xScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, yScaleCurve.Evaluate(tValue)),
					Mathf.LerpUnclamped(0, 1, zScaleCurve.Evaluate(tValue)));

				transform.localScale = newScale;

				time += Time.deltaTime;
				yield return null;
			}

			transform.localScale = Vector3.zero;

			SetButtonRowEnable(topRow, false);
			SetButtonRowEnable(midRow, false);
			SetButtonRowEnable(bottomRow, false);
			SetButtonEnable(returnButton, false);
			SetButtonEnable(caseButton, false);
			SetButtonEnable(symbolButton, false);
			SetButtonEnable(backspaceButton, false);

			visible = false;

            textInput.gameObject.transform.parent.gameObject.SetActive(false);

            yield break;
        }

        #region Key Button Event Methods
        private bool KeyIsChar(KeyCode suspect)
        {
            for (int i = 0; i < NonCharKeys.Length; i++)
            {
                if (suspect == NonCharKeys[i])
                {
                    return false;
                }
            }

            return true;
        }

        void button_Activated(KeyButton sender)
        {
            Debug.Log("Key pressed: " + sender.Key.ToString());
            if (KeyIsChar(sender.Key))
            {
                if (sender.UpperCase) textInput.text = textInput.text + ConvertCodeToChar(sender.Key.ToString()).ToUpper();
                else textInput.text = textInput.text + ConvertCodeToChar(sender.Key.ToString()).ToLower();

                textInput.selectionAnchorPosition++;
				textInput.selectionFocusPosition = textInput.selectionAnchorPosition;
            }
            else
            {
                // look for our special case strings!
                if (sender.Key == KeyCode.Backspace)
                {
                    // re-do this when we implement selection moving
                    if ((textInput.text.Length > 0) &&
                        textInput.selectionFocusPosition - textInput.selectionAnchorPosition > 0)
                    {
                        int position = textInput.selectionAnchorPosition - 1;
                        int length = textInput.selectionFocusPosition - textInput.selectionAnchorPosition;

                        textInput.text = textInput.text.Remove(position,
                        length);
                    }
                    else if(textInput.text.Length > 0)
                    {
                        textInput.text = textInput.text.Remove(textInput.selectionAnchorPosition - 1, 1);
                    }
                }
                else if (sender.Key == KeyCode.Space)
                {
                    textInput.text+=(" ");
                    textInput.selectionAnchorPosition++;
					textInput.selectionFocusPosition = textInput.selectionAnchorPosition;
				}
                else if ((sender.Key == KeyCode.LeftShift) || (sender.Key == KeyCode.RightShift))
                {
                    ChangeCase();
                }
                else if (sender.Key == KeyCode.Return)
                {
                    OnReturn.Invoke();
                }
            }

            Contents = stringBuilder.ToString();

            timeSinceLastKey = 0;
        }
        #endregion

        public static string ConvertCodeToChar(string input)
        {
            string retString = input;

            int alphaIndx = retString.IndexOf("Alpha");

            retString = (alphaIndx < 0) ? retString : retString.Remove(alphaIndx, "Alpha".Length);

            if (retString == KeyCode.Exclaim.ToString()) retString = "!";
            else if (retString == KeyCode.At.ToString()) retString = "@";
            else if (retString == KeyCode.Hash.ToString()) retString = "#";
            else if (retString == KeyCode.Dollar.ToString()) retString = "$";
            else if (retString == KeyCode.Caret.ToString()) retString = "^";
            else if (retString == KeyCode.Ampersand.ToString()) retString = "&";
            else if (retString == KeyCode.Asterisk.ToString()) retString = "*";
            else if (retString == KeyCode.LeftParen.ToString()) retString = "(";
            else if (retString == KeyCode.RightParen.ToString()) retString = ")";
            else if (retString == KeyCode.Minus.ToString()) retString = "-";
            else if (retString == KeyCode.Plus.ToString()) retString = "+";
            else if (retString == KeyCode.LeftBracket.ToString()) retString = "[";
            else if (retString == KeyCode.RightBracket.ToString()) retString = "]";
            else if (retString == KeyCode.Equals.ToString()) retString = "=";
            else if (retString == KeyCode.Semicolon.ToString()) retString = ";";
            else if (retString == KeyCode.Colon.ToString()) retString = ":";
            else if (retString == KeyCode.Quote.ToString()) retString = "'";
            else if (retString == KeyCode.DoubleQuote.ToString()) retString = "\"";
            else if (retString == KeyCode.Comma.ToString()) retString = ",";
            else if (retString == KeyCode.Less.ToString()) retString = "<";
            else if (retString == KeyCode.Greater.ToString()) retString = ">";
            else if (retString == KeyCode.Slash.ToString()) retString = "/";
            else if (retString == KeyCode.Question.ToString()) retString = "?";
            else if (retString == KeyCode.Backslash.ToString()) retString = "\\";
            else if (retString == KeyCode.BackQuote.ToString()) retString = "`";
            else if (retString == KeyCode.Underscore.ToString()) retString = "_";
            else if (retString == KeyCode.Period.ToString()) retString = ".";
            else if (retString.ToLowerInvariant() == KeyCode.Space.ToString().ToLowerInvariant()) retString = " ";
            else if (retString.ToLowerInvariant() == KeyCode.None.ToString().ToLowerInvariant()) retString = "";

            return retString;
        }
    }
}
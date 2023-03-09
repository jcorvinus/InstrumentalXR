using UnityEngine;
using System.Collections;

namespace VRKeyboard
{
    /// <summary>
    /// Connects to a button and relays those events to
    /// a virtual keyboard.
    /// </summary>
    [RequireComponent(typeof(FingerButton))]
    public class KeyButton : MonoBehaviour
    {
        public delegate void KeyButtonEventHandler(KeyButton sender);
        public event KeyButtonEventHandler Activated;
        public event KeyButtonEventHandler HoverGained;
        public event KeyButtonEventHandler HoverLost;

        private Keyboard keyboard;
        private FingerButton button;

        private bool upperCase = false;
        public bool UpperCase { get { return upperCase; } set { upperCase = value; SetText(); } }

        private KeyCode key;
        public KeyCode Key
        {
            get { return key; }
            set { key = value; SetText(); }
        }

        private UnityEngine.UI.Text characterText;
        public string Text
        {
            get { return characterText.text; }
        }
        public UnityEngine.UI.Text CharacterText
        {
            get { return characterText; }
            set { characterText = value; }
        }

        void Awake()
        {
            button = GetComponent<FingerButton>();
            keyboard = GetComponentInParent<Keyboard>();

            if(keyboard == null)
            {
                enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            SetText();

            button.ButtonActivated += OnButtonActivated;
            button.ButtonHovered += OnButtonHoverGained;
            button.ButtonHoverEnded += OnButtonHoverLost;
        }

        private void SetText()
        {
            if (characterText != null)
            {
                if (!UpperCase)
                {
                    characterText.text = Keyboard.ConvertCodeToChar(key.ToString()).ToLower();
                }
                else
                {
                    characterText.text = Keyboard.ConvertCodeToChar(key.ToString());
                }
            }
        }

        void OnDisable()
        {
            button.ButtonActivated -= OnButtonActivated;
            button.ButtonHovered -= OnButtonHoverGained;
            button.ButtonHoverEnded -= OnButtonHoverLost;
        }

        private void Update()
        {
            button.VolumeModifier = keyboard.VolumeAdjust;
        }

        #region Event Methods
        private void OnButtonActivated(FingerButton sender)
        {
            if (Activated != null) Activated(this);
        }

        private void OnButtonHoverGained(FingerButton sender)
        {
            if (HoverGained != null) HoverGained(this);
        }

        private void OnButtonHoverLost(FingerButton sender)
        {
            if (HoverLost != null) HoverLost(this);
        }
        #endregion
    }
}
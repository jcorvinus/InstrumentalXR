using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Instrumental
{
	[System.Serializable]
    public struct ControlPrefabReference
	{
        public Controls.ControlType Type;
        public GameObject Prefab;
    }
	
    [CreateAssetMenu(fileName = "UICommon", menuName = "Instrumental/UICommonElements", order = 1)]
    public class UICommonElements : ScriptableObject
    {
        [SerializeField] AudioClip grabClip;
        [SerializeField] AudioClip itemPlaceClip;
        [SerializeField] AudioClip errorClip;
        [SerializeField] AudioClip alertClip;
        [SerializeField] AudioMixerGroup masterGroup;

        // control accessors
        [SerializeField]
        ControlPrefabReference[] controlPrefabs;

        public AudioClip GrabClip { get { return grabClip; } }
        public AudioClip ItemPlaceClip { get { return itemPlaceClip; } }
        public AudioClip ErrorClip { get { return errorClip; } } 
        public AudioClip AlertClip { get { return alertClip; } }

        public AudioMixerGroup MasterGroup { get { return masterGroup; } }
    }
}
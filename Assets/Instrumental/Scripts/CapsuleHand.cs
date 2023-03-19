using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace Instrumental
{
    public class CapsuleHand : MonoBehaviour
    {
        public struct BoneInfo
		{
            public GameObject StartEpiphysis;
            public GameObject Diaphysis;
            public GameObject EndEpiphysis;
		}

        SteamVR_Behaviour_Pose pose;
        SteamVR_Behaviour_Skeleton skeleton;

        BoneInfo[] proximals;
        BoneInfo[] medials;
        BoneInfo[] distals;

        [Range(0, 0.1f)]
        [SerializeField] float radius = 0.001f;

		private void Awake()
		{
            skeleton = GetComponent<SteamVR_Behaviour_Skeleton>();
            pose = GetComponentInParent<SteamVR_Behaviour_Pose>();

            proximals = GenerateBones(false);
            medials = GenerateBones(false);
            distals = GenerateBones(true);
		}

        BoneInfo[] GenerateBones(bool addEndpoint)
		{
            BoneInfo[] bones = new BoneInfo[5];

            for(int i=0; i < 5; i++)
			{
                GameObject startEpiphysis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                startEpiphysis.transform.parent = transform;
                startEpiphysis.transform.localScale = Vector3.one * radius;
                SphereCollider startCollider = startEpiphysis.GetComponent<SphereCollider>();
                Destroy(startCollider);

                // bone forward axis is up for this gameobject
                GameObject diaphysis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                diaphysis.transform.parent = transform;
                diaphysis.transform.localScale = new Vector3(radius, radius * 2, radius);
                CapsuleCollider diaphysisCollider = diaphysis.GetComponent<CapsuleCollider>();
                Destroy(diaphysisCollider);

                GameObject endEpiphysis = null;
                if(addEndpoint)
				{
                    endEpiphysis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    endEpiphysis.transform.parent = transform;
                    endEpiphysis.transform.localScale = Vector3.one * radius;
                    SphereCollider endCollider = endEpiphysis.GetComponent<SphereCollider>();
                    Destroy(endCollider);
				}

                BoneInfo bone = new BoneInfo()
                {
                    StartEpiphysis = startEpiphysis,
                    Diaphysis = diaphysis,
                    EndEpiphysis = endEpiphysis
                };

                bones[i] = bone;
			}

			return bones;
		}

		private void OnDisable()
		{
            SetBoneEnable(proximals, false);
            SetBoneEnable(medials, false);
            SetBoneEnable(distals, false);
        }

		// Start is called before the first frame update
		void Start()
        {

        }

        void SetBoneEnable(BoneInfo[] bones, bool state)
		{
            for(int i=0; i < bones.Length; i++)
			{
                BoneInfo bone = bones[i];
                if (bone.StartEpiphysis) bone.StartEpiphysis.SetActive(state);
                if (bone.Diaphysis) bone.Diaphysis.SetActive(state);
                if (bone.EndEpiphysis) bone.EndEpiphysis.SetActive(state);
			}
		}

        // Update is called once per frame
        void Update()
        {
            if(pose && pose.isValid)
			{
                for(int i=0; i < proximals.Length; i++)
				{
                    BoneInfo proximalBone = proximals[i];
                    Transform proxTransform = skeleton.proximals[i];
                    Transform medialTransform = skeleton.middles[i];

                    // set the startEpiphysis to the proximal location
                    proximalBone.StartEpiphysis.transform.position = proxTransform.position;
                    Vector3 center = (proxTransform.position + medialTransform.position) * 0.5f;
                    Vector3 offset = proxTransform.position - medialTransform.position;
                    float length = offset.magnitude;
                    proximalBone.Diaphysis.transform.position = center;
                    proximalBone.Diaphysis.transform.rotation = 
                        Quaternion.LookRotation(proxTransform.up, proxTransform.right);
                    proximalBone.Diaphysis.transform.localScale = new Vector3(radius, length * 0.5f, radius);

                    proximalBone.StartEpiphysis.transform.localScale = Vector3.one * radius;
				}

                for (int i = 0; i < medials.Length; i++)
                {
                    BoneInfo medialBone = medials[i];
                    Transform medialTransform = skeleton.middles[i];
                    Transform distalTransform = skeleton.distals[i];

                    // set the startEpiphysis to the proximal location
                    medialBone.StartEpiphysis.transform.position = medialTransform.position;
                    Vector3 center = (medialTransform.position + distalTransform.position) * 0.5f;
                    Vector3 offset = medialTransform.position - distalTransform.position;
                    float length = offset.magnitude;
                    medialBone.Diaphysis.transform.position = center;
                    medialBone.Diaphysis.transform.rotation = 
                        Quaternion.LookRotation(medialTransform.up, medialTransform.right);
                    medialBone.Diaphysis.transform.localScale = new Vector3(radius, length * 0.5f, radius);

                    medialBone.StartEpiphysis.transform.localScale = Vector3.one * radius;
                }

                for (int i = 0; i < distals.Length; i++)
                {
                    BoneInfo distalBone = distals[i];
                    Transform distalTransform = skeleton.distals[i];
                    Transform tipTransform = skeleton.tips[i];

                    // set the startEpiphysis to the proximal location
                    distalBone.StartEpiphysis.transform.position = distalTransform.position;
                    Vector3 center = (distalTransform.position + tipTransform.position) * 0.5f;
                    Vector3 offset = distalTransform.position - tipTransform.position;
                    float length = offset.magnitude;
                    distalBone.Diaphysis.transform.position = center;
                    distalBone.Diaphysis.transform.rotation =
                        Quaternion.LookRotation(distalTransform.up, distalTransform.right);
                    distalBone.Diaphysis.transform.localScale = new Vector3(radius, length * 0.5f, radius);

                    distalBone.StartEpiphysis.transform.localScale = Vector3.one * radius;
                    distalBone.EndEpiphysis.transform.localScale = Vector3.one * radius;
                    distalBone.EndEpiphysis.transform.position = tipTransform.position;
                }

                SetBoneEnable(proximals, true);
                SetBoneEnable(medials, true);
                SetBoneEnable(distals, true);
            }
            else
			{
                SetBoneEnable(proximals, false);
                SetBoneEnable(medials, false);
                SetBoneEnable(distals, false);
			}
        }
    }
}
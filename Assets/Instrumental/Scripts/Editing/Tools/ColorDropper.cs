using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.Interaction;

namespace Instrumental.Editing.Tools
{
    public class ColorDropper : MonoBehaviour
    {
        [System.Serializable]
        struct ColorZoneInfo
        {
            public bool isValid;
            public ColorZoneRelay relay;
            public ColorZone zone;
            public Collider collider;
            public Vector3 closestPointOnCollider;
        }

        Color currentColor;
        bool hasColor;

        [SerializeField]
        SkinnedMeshRenderer topRenderer;

        [SerializeField]
        MeshRenderer colorRenderer;
        InteractionBehaviour interaction;

        [SerializeField]
        MeshRenderer signifierRenderer;

        int colorHash;
        int signifierTextureHash;

        [SerializeField]
        float topPinchHeight = 0.1f;

        [SerializeField]
        float pinchActivateDist = 0.02f;

        [SerializeField]
        float pinchDeActivateDist = 0.04f;

        PinchDetector leftPinch;
        PinchDetector rightPinch;

        [Range(0, 0.2f)]
        [SerializeField]
        float colorTipHeightOffset = 0.02f;
        ColorZone currentZone;
        Vector2 sampleTextureScale = new Vector2(1, 1);
        Vector2 dropTextureScale = new Vector2(1, -1);

        [SerializeField]
        AudioSource dropSource;

        [SerializeField]
        AudioSource pickSource;

        [SerializeField]
        AudioSource errorSource;

        [SerializeField]
        Vector3 tipZoneSize = new Vector3(0.04f, 0.06176521f, 0.04f);

        [SerializeField]
        LayerMask colorZoneLayers;
        ColorZoneInfo[] colorZoneInfo = new ColorZoneInfo[4];
        Collider[] colorZoneColliders = new Collider[4];
        float timeBetweenTipChecks = 0.11f;
        float checkTimer=0;

        public bool IsGrasped { get { return interaction.isGrasped; } }

        private void Awake()
        {
            colorHash = Shader.PropertyToID("_Color");
            signifierTextureHash = Shader.PropertyToID("_MainTex");

            interaction = GetComponent<InteractionBehaviour>();

            PinchDetector[] allPinchers = GetComponents<PinchDetector>();

            leftPinch = allPinchers.First(item => item.HandModel.Handedness == Chirality.Left);
            leftPinch.ActivateDistance = pinchActivateDist;
            leftPinch.DeactivateDistance = pinchDeActivateDist;
            rightPinch = allPinchers.First(item => item.HandModel.Handedness == Chirality.Right);
            rightPinch.ActivateDistance = pinchActivateDist;
            rightPinch.DeactivateDistance = pinchDeActivateDist;
        }

        // Use this for initialization
        void Start()
        {
            colorRenderer.enabled = false;

            leftPinch.OnActivate.AddListener(Use);
            rightPinch.OnActivate.AddListener(Use);
        }

        bool EitherPinchValid()
        {
            Vector3 topPosition = GetTopPosition();
            float leftDistance = Vector3.Distance(leftPinch.Position, topPosition);
            float rightDistance = Vector3.Distance(rightPinch.Position, topPosition);

            return leftDistance < pinchDeActivateDist || rightDistance < pinchDeActivateDist;
        }

        PinchDetector ClosestPinch()
        {
            Vector3 topPosition = GetTopPosition();
            float leftDistance = Vector3.Distance(leftPinch.Position, topPosition);
            float rightDistance = Vector3.Distance(rightPinch.Position, topPosition);

            return (leftDistance < rightDistance) ? leftPinch : rightPinch;
        }

        Vector3 GetTopPosition()
        {
            return transform.TransformPoint(Vector3.up * topPinchHeight);
        }

        void DoColorZoneUpdate()
        {
            checkTimer -= Time.deltaTime;

            if(checkTimer <= 0)
            {
                int numValid = Physics.OverlapBoxNonAlloc(GetTipPosition(), tipZoneSize, 
                    colorZoneColliders, transform.rotation, colorZoneLayers, QueryTriggerInteraction.Collide);

                if (numValid == 0)
                {
                    for (int i = 0; i < colorZoneColliders.Length; i++)
                    {
                        colorZoneColliders[i] = null;
                        colorZoneInfo[i].collider = null;
                        colorZoneInfo[i].isValid = false;
                        colorZoneInfo[i].zone = null;
                        colorZoneInfo[i].relay = null;
                    }
                }

                for (int i=0; i < colorZoneColliders.Length; i++)
                {
                    if(i < numValid)
                    {
                        colorZoneInfo[i].collider = colorZoneColliders[i];

                        // check to see if collider is a valid colorzone

                        ColorZone[] zones = colorZoneInfo[i].collider.GetComponents<ColorZone>();
                        if (zones.Any(item => item.enabled)) colorZoneInfo[i].zone = zones.First(item => item.enabled);
                        else colorZoneInfo[i].zone = null;
                        colorZoneInfo[i].relay = colorZoneInfo[i].collider.GetComponent<ColorZoneRelay>();

                        colorZoneInfo[i].isValid = colorZoneInfo[i].zone != null || colorZoneInfo[i].relay != null;
                        colorZoneInfo[i].closestPointOnCollider = colorZoneInfo[i].collider.ClosestPointOnBounds(GetTipPosition());
                    }
                    else
                    {
                        colorZoneInfo[i].collider = null;
                        colorZoneInfo[i].isValid = false;
                        colorZoneInfo[i].zone = null;
                        colorZoneInfo[i].relay = null;
                    }
                }

                // if any colliders available, find the best candidate
                int smallestDistIndx=-1;
                float smallestDist = float.PositiveInfinity;

                for(int i=0; i < colorZoneInfo.Length; i++)
                {
                    if (colorZoneInfo[i].isValid)
                    {
                        float dist = Vector3.Distance(GetTipPosition(), colorZoneInfo[i].closestPointOnCollider);
                        if(dist < smallestDist)
                        {
                            smallestDist = dist;
                            smallestDistIndx = i;
                        }
                    }
                }

                // update the current color zone
                if(smallestDistIndx == -1)
                {
                    currentZone = null;
                }
                else
                {
                    if (colorZoneInfo[smallestDistIndx].zone != null) currentZone = colorZoneInfo[smallestDistIndx].zone;
                    else currentZone = colorZoneInfo[smallestDistIndx].relay.Zone;
                }

                checkTimer = timeBetweenTipChecks;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // check for color zones
            DoColorZoneUpdate();

            if(currentZone)
            {
                signifierRenderer.enabled = true;
                if (!currentZone.CanDrop)
                {
                    // always allow sampling
                    // set preview color
                    signifierRenderer.material.SetColor(colorHash, currentZone.GetColorAtPoint(GetTipPosition()));
                    signifierRenderer.material.SetTextureScale(signifierTextureHash, sampleTextureScale);
                }
                else
                {
                    // allow sampling dependent upon whether or not there is
                    // ink in the dropper
                    if(hasColor)
                    {
                        signifierRenderer.material.SetColor(colorHash, currentColor);
                        signifierRenderer.material.SetTextureScale(signifierTextureHash, dropTextureScale);
                    }
                    else
                    {
                        signifierRenderer.material.SetColor(colorHash, currentZone.GetColorAtPoint(GetTipPosition()));
                        signifierRenderer.material.SetTextureScale(signifierTextureHash, sampleTextureScale);
                    }
                }
            }
            else
            {
                signifierRenderer.enabled = false;
            }

            if (EitherPinchValid())
            {
                PinchDetector closestPinch = ClosestPinch();

                float distanceTValue = 1 - Mathf.InverseLerp(pinchActivateDist, pinchDeActivateDist * 1.5f, closestPinch.Distance);
                topRenderer.SetBlendShapeWeight(0, distanceTValue * 100);
            }
            else
            {
                topRenderer.SetBlendShapeWeight(0, 0);
            }
        }

        void Use()
        {
            if (EitherPinchValid())
            {
                if (currentZone)
                {
                    if (!currentZone.CanDrop)
                    {
                        // sample
                        currentColor = currentZone.GetColorAtPoint(GetTipPosition());
                        hasColor = true;

                        pickSource.Play();
                    }
                    else
                    {
                        if (hasColor)
                        {
                            currentZone.SetColorAtPoint(currentColor, GetTipPosition());
                            hasColor = false;
                            dropSource.Play();
                        }
                        else
                        {
                            currentColor = currentZone.GetColorAtPoint(GetTipPosition());
                            hasColor = true;
                            pickSource.Play();
                        }
                    }
                }
                else
                {
                    // give some kind of negative feedback
                    errorSource.Play();
                }

                colorRenderer.enabled = hasColor;
                colorRenderer.material.SetColor(colorHash, currentColor);
            }
        }

        public Vector3 GetTipPosition()
        {
            return transform.TransformPoint(Vector3.down * colorTipHeightOffset);
        }

        public void SetColorZone(ColorZone colorZone)
        {
            currentZone = colorZone;
        }
        
        public void ClearColorZone()
        {
            currentZone = null;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 top = GetTopPosition();

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(top, pinchActivateDist);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(top, pinchDeActivateDist);

            //Gizmos.DrawWireSphere(GetTipPosition(), 0.02f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.down * colorTipHeightOffset, tipZoneSize);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
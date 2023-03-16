Shader "Instrumental/UIShader"
{
    /*
    - Touch Start
    - Touch complete
    - Hover (far - interaction point is outside of the bounds but nearby)
    - Hover (near - interaction point is inside of the bounds)
    - Press
    - Grasp

    Material property block data for:
    - Touch Amount
    - IsPressing
    - IsGrasping

    Global shader values for:
    - interaction positions (left/right)
    */

    Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _TouchMinColor ("Touch Min", Color) = (0, 0, 0, 1)
        _TouchMaxColor ("Touch Max", Color) = (0, 0, 0, 1)
        _HoverNearColor ("Hover Near", Color) = (0, 0, 0, 1)
        _HoverFarColor ("Hover Far", Color) = (0, 0, 0, 1)
        _PressColor ("Press Color", Color) = (0, 0, 0, 1)

        // per renderer data variables (add these later), once we know all behaviors work properly.
        [PerRendererData] _GlowAmount ("Contextual Glow Amount", Float) = 0
        [PerRendererData] _IsPressing ("IsPressing", Integer) = 0
        [PerRendererData] _IsTouching ("IsTouching", Integer) = 0
        [PerRendererData] _IsGrasping ("IsGrasping", Integer) = 0
        [PerRendererData] _IsHovering ("IsHovering", Integer) = 0
        [PerRendererData] _UseDistanceGlow("Use Distance Glow", Integer) = 0
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noshadow nolightmap noforwardadd vertex:vert exclude_path:deferred

        sampler2D _MainTex;

        fixed4 _GlobalLeftFingertip;
        fixed4 _GlobalRightFingertip;

        int _IsPressing;
        int _UseDistanceGlow;
        int _IsHovering;
        int _IsTouching;
        fixed4 _PressColor;
        fixed4 _TouchMinColor;
        fixed4 _TouchMaxColor;
        fixed4 _HoverNearColor;
        fixed4 _HoverFarColor;
        float _GlowAmount;
        float _HandGlowMaxDistance;

        struct Input 
        {
            float2 uv_MainTex;
            float4 vertColor;
            float3 worldPos;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.vertColor = v.color;
        }

        float InverseLerp(float low, float high, float value)
		{
			return (value - low) / (high - low);
		}

        void surf (Input IN, inout SurfaceOutput o) 
        {
            float leftDistance = distance(_GlobalLeftFingertip, IN.worldPos);
            float rightDistance = distance(_GlobalRightFingertip, IN.worldPos);
            float lowestDistance = min(leftDistance, rightDistance);

            float glowValue = 1 - InverseLerp(0, _HandGlowMaxDistance, lowestDistance);
            glowValue = saturate(glowValue);

            fixed4 emission = fixed4(0,0,0,1);
            if(_UseDistanceGlow == 1)
            {
                if(_IsPressing == 1)
                {
                    emission = _PressColor;
                }
                else if (_IsTouching == 1)
                {
                    emission = lerp(_TouchMinColor, _TouchMaxColor, _GlowAmount);
                }
                else if(_IsHovering == 1)
                {
                    // if our touch amount is over 0, then we need to lerp touch colors
                   
                    // otherwise lerp hover colors
                    emission = _HoverNearColor * glowValue;
                }
                else 
                {
                    emission = _HoverFarColor * glowValue;
                }
            }

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.vertColor;
            o.Alpha = c.a;
            o.Emission = emission;
        }
        ENDCG
    }

Fallback "Mobile/VertexLit"
}

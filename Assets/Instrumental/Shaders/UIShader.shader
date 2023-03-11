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

        [PerRendererData] _TouchAmount ("Touch Amount", Float) = 0
        [PerRendererData] _IsPressing ("IsPressing", Integer) = 0
        [PerRendererData] _IsGrasping ("IsGrasping", Integer) = 0
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Lambert noforwardadd vertex:vert

        sampler2D _MainTex;

        struct Input 
        {
            float2 uv_MainTex;
            float4 vertColor;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.vertColor = v.color;
        }

        void surf (Input IN, inout SurfaceOutput o) 
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * IN.vertColor;
            o.Alpha = c.a;
        }
        ENDCG
    }

Fallback "Mobile/VertexLit"
}

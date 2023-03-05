Shader "Custom/TransparentFresnel" 
{
    Properties 
    {
		_InnerColor ("Inner Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
		_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
    }
    SubShader 
    {
		Tags { "RenderType" = "Transparent" "RenderQueue" = "Transparent" }
       
		// extra pass that renders to depth buffer only
		/*Pass 
		{
			ZWrite On
			ColorMask 0
		}*/

		Cull Back
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One
		ZWrite On
       
		CGPROGRAM
		#pragma surface surf Lambert
       
		struct Input 
		{
			float3 viewDir;
		};
       
		float4 _InnerColor;
		float4 _RimColor;
		float _RimPower;
       
		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Albedo = _InnerColor.rgb;
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);
			o.Alpha = 1;
		}
		ENDCG
    }
    Fallback "Diffuse"
}
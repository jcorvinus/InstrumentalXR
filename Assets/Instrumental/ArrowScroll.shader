Shader "Unlit/ArrowScroll"
{
	Properties
	{
		_Color ("Color", Color) = (0, 0, 0, 0)
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaTex ("AlphaTexture", 2D) = "white" {}
		_Alpha ("Alpha", float) = 1
		_ScrollSpeed ("ScrollSpeed", float) = 6
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _AlphaTex;
			float4 _AlphaTex_ST;
			float _Alpha;
			float _ScrollSpeed;

			float2 scroll;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _AlphaTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float2 uvMod = float2(0,0);
				uvMod.x = saturate(i.uv.x);
				uvMod.y	= i.uv.y + -_ScrollSpeed * _Time;

				fixed4 col = tex2D(_MainTex, uvMod);
				col *= _Color;
				col.a *= _Alpha;
				col.a *= tex2D(_AlphaTex, i.uv2).r;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

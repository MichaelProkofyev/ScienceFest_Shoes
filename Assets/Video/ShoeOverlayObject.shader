Shader "Unlit/ShoeOverlayObject"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_WhiteCutout ("White cutout", float) = 0.3
		_NoiseTexture ("Noise", 2D) = "white" {}
		_OffsetMultiplierX("Offset multiplier X", float) = 0
		_OffsetMultiplierY("Offset multiplier y", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTexture;
			float4 _MainTex_ST;
			float _WhiteCutout; 
			float _OffsetMultiplierX;
			float _OffsetMultiplierY;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 n = tex2D(_NoiseTexture, i.uv);
				float2 uvMorphed = float2(i.uv.x + n.x * _OffsetMultiplierX, i.uv.y + n.y * _OffsetMultiplierY);
				fixed4 t = tex2D(_MainTex, uvMorphed);
				
				fixed4 col = t;
				return col;
			}
			ENDCG
		}
	}
}

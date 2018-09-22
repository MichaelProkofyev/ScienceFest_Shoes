Shader "Unlit/ShoeOverlayTwoPairBlend"
{
	Properties
	{
		_MainTex1 ("Texture1", 2D) = "white" {}
		_MainTex2 ("Texture1", 2D) = "white" {}
		_BlendValue ("Blend", Range (0, 1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Overlay" }
		LOD 100

		ZTest Always
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex1;
			sampler2D _MainTex2;
			float4 _MainTex1_ST;
			float _BlendValue;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex1);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv = float2(i.uv.x, -i.uv.y);
				fixed4 t1 = tex2D(_MainTex1, i.uv) * (1 - _BlendValue);
				fixed4 t2 = tex2D(_MainTex2, i.uv) * _BlendValue;
				fixed4 col = t1 + t2;
				return col;
			}
			ENDCG
		}
	}
}

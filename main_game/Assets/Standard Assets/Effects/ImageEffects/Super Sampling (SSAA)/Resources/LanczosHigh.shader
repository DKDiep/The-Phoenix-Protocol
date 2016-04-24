Shader "SSAA/LanczosHigh" 
{
	Properties 
	{
	 _textureHeight ("TextureHeight", Float) = 0.0			// 1 / tex.width
	 _textureWidth ("TextureWidth", Float) = 0.0 			// 1 / tex.height
	 _MainTex ("Texture", 2D) = "" {} 
	 
	}
	SubShader { 
		Pass {
 			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _textureWidth;
			float _textureHeight;
			
			float4 _MainTex_ST;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord = v.texcoord.xy;

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				float _texelWidth = (1.0 / _textureWidth);
				float _texelHeight = (1.0 / _textureHeight);

				float texelHeight2 = _texelHeight * 2.0f;
				float texelHeight3 = _texelHeight * 3.0f;
				float texelHeight4 = _texelHeight * 4.0f;

				float4 res = tex2D(_MainTex, i.texcoord) * 0.38026f;

				res += tex2D(_MainTex, i.texcoord + float2(0.0f, -_texelHeight)) * 0.138335f;
				res += tex2D(_MainTex, i.texcoord + float2(0.0f, _texelHeight)) * 0.138335f;

				res += tex2D(_MainTex, i.texcoord + float2(0.0f, -texelHeight2)) * 0.04037f;
				res += tex2D(_MainTex, i.texcoord + float2(0.0f, texelHeight2)) * 0.04037f;

				res += tex2D(_MainTex, i.texcoord + float2(0.0f, -texelHeight3)) * -0.01306f;
				res += tex2D(_MainTex, i.texcoord + float2(0.0f, texelHeight3)) * -0.01306f;

				res += tex2D(_MainTex, i.texcoord + float2(0.0f, -texelHeight4)) * -0.010715f;
				res += tex2D(_MainTex, i.texcoord + float2(0.0f, texelHeight4)) * -0.010715f;

				res += tex2D(_MainTex, i.texcoord + float2(-_texelHeight, 0.0f)) * 0.138335f;
				res += tex2D(_MainTex, i.texcoord + float2(_texelHeight, 0.0f)) * 0.138335f;

				res += tex2D(_MainTex, i.texcoord + float2(-texelHeight2, 0.0f)) * 0.04037f;
				res += tex2D(_MainTex, i.texcoord + float2(texelHeight2, 0.0f)) * 0.04037f;

				res += tex2D(_MainTex, i.texcoord + float2(-texelHeight3, 0.0f)) * -0.01306f;
				res += tex2D(_MainTex, i.texcoord + float2(texelHeight3, 0.0f)) * -0.01306f;

				res += tex2D(_MainTex, i.texcoord + float2(-texelHeight4, 0.0f)) * -0.010715f;
				res += tex2D(_MainTex, i.texcoord + float2(texelHeight4, 0.0f)) * -0.010715f;

				return res;
			}
			ENDCG 

		}
	}
	Fallback Off 
}

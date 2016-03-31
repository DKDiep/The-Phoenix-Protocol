Shader "SSAA/BilinearHigh" 
{
	Properties 
	{
	 _textureHeight ("TextureHeight", Float) = 0.0
	 _textureWidth ("TextureWidth", Float) = 0.0
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
				float _texelWidth = (1.0f / _textureWidth);
				float _texelHeight = (1.0f / _textureHeight);
			
				float4 tt = tex2D(_MainTex, i.texcoord + float2(0.0f, -_texelHeight * 2.0f));
				float4 bb = tex2D(_MainTex, i.texcoord + float2(0.0f, _texelHeight * 2.0f));

				float4 ll = tex2D(_MainTex, i.texcoord + float2(-_texelWidth * 2.0f, 0.0f));
				float4 rr = tex2D(_MainTex, i.texcoord + float2(_texelWidth * 2.0f, 0.0f));

				float4 tl = tex2D(_MainTex, i.texcoord + float2(-_texelWidth, -_texelHeight));
				float4 tm = tex2D(_MainTex, i.texcoord + float2(0.0f, -_texelHeight));
				float4 tr = tex2D(_MainTex, i.texcoord + float2(_texelWidth, -_texelHeight));

				float4 ml = tex2D(_MainTex, i.texcoord + float2(-_texelWidth, 0.0f));
				float4 mm = tex2D(_MainTex, i.texcoord + float2(0.0f, 0.0f));
				float4 mr = tex2D(_MainTex, i.texcoord + float2(_texelWidth, 0.0f));

				float4 bl = tex2D(_MainTex, i.texcoord + float2(-_texelWidth, _texelHeight));
				float4 bm = tex2D(_MainTex, i.texcoord + float2(0.0f, _texelHeight));
				float4 br = tex2D(_MainTex, i.texcoord + float2(_texelWidth , _texelHeight));

				return (mm + mr + bl + ml + tl + tt + bb + ll + rr) / 9.0f;
			}
			ENDCG 

		}
	}
	Fallback Off 
}

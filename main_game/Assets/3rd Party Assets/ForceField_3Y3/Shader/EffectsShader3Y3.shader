// Shader created with Shader Forge v1.16 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.16;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,culm:2,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:2,rfrpo:True,rfrpn:Refraction,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:37240,y:32712,varname:node_1,prsc:2|emission-2476-OUT;n:type:ShaderForge.SFN_Multiply,id:890,x:36609,y:32726,varname:node_890,prsc:2|A-1876-RGB,B-1665-OUT;n:type:ShaderForge.SFN_Fresnel,id:893,x:34224,y:32606,varname:node_893,prsc:2|EXP-1173-OUT;n:type:ShaderForge.SFN_Multiply,id:895,x:35857,y:32688,varname:node_895,prsc:2|A-1316-R,B-893-OUT;n:type:ShaderForge.SFN_Tex2d,id:896,x:34535,y:32220,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_5931,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-923-UVOUT;n:type:ShaderForge.SFN_Panner,id:923,x:34341,y:32220,varname:node_923,prsc:2,spu:0,spv:0.1|DIST-2436-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1173,x:34064,y:32624,ptovrint:False,ptlb:Power,ptin:_Power,varname:node_5909,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Tex2d,id:1316,x:35611,y:32561,ptovrint:False,ptlb:Texture_Decay,ptin:_Texture_Decay,varname:node_4796,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:5e5f7fdec6ba5a24dbd26d977985f37c,ntxv:0,isnm:False|UVIN-1319-OUT;n:type:ShaderForge.SFN_Append,id:1319,x:35393,y:32561,varname:node_1319,prsc:2|A-1497-OUT,B-1359-OUT;n:type:ShaderForge.SFN_TexCoord,id:1336,x:34638,y:32718,varname:node_1336,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:1340,x:34816,y:32804,varname:node_1340,prsc:2|A-1336-V,B-1342-OUT;n:type:ShaderForge.SFN_Vector1,id:1342,x:34638,y:32866,varname:node_1342,prsc:2,v1:0;n:type:ShaderForge.SFN_Add,id:1359,x:34990,y:32906,varname:node_1359,prsc:2|A-1340-OUT,B-2056-OUT;n:type:ShaderForge.SFN_Add,id:1497,x:35055,y:32442,varname:node_1497,prsc:2|A-896-R,B-893-OUT;n:type:ShaderForge.SFN_Clamp,id:1665,x:36239,y:32784,varname:node_1665,prsc:2|IN-2450-OUT,MIN-1667-OUT,MAX-1666-OUT;n:type:ShaderForge.SFN_Vector1,id:1666,x:36075,y:32945,varname:node_1666,prsc:2,v1:0.95;n:type:ShaderForge.SFN_Vector1,id:1667,x:36075,y:32879,varname:node_1667,prsc:2,v1:0.05;n:type:ShaderForge.SFN_Vector1,id:1772,x:34224,y:32515,varname:node_1772,prsc:2,v1:0;n:type:ShaderForge.SFN_Color,id:1876,x:36356,y:32572,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_5534,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.1764706,c2:0.5229208,c3:1,c4:1;n:type:ShaderForge.SFN_Slider,id:2056,x:34659,y:32988,ptovrint:False,ptlb:Decay,ptin:_Decay,varname:node_2239,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.05,cur:0.223077,max:0.95;n:type:ShaderForge.SFN_Time,id:2434,x:34012,y:32217,varname:node_2434,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2436,x:34183,y:32241,varname:node_2436,prsc:2|A-2434-T,B-440-OUT;n:type:ShaderForge.SFN_Multiply,id:2450,x:36042,y:32636,varname:node_2450,prsc:2|A-895-OUT,B-2452-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2452,x:35857,y:32842,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:node_8106,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:2476,x:36981,y:32811,varname:node_2476,prsc:2|A-890-OUT,B-2477-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2477,x:36749,y:32873,ptovrint:False,ptlb:Brightness,ptin:_Brightness,varname:node_4855,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Slider,id:440,x:33855,y:32389,ptovrint:False,ptlb:Speed,ptin:_Speed,varname:node_440,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5,max:15;proporder:2477-2452-1876-896-1316-2056-1173-440;pass:END;sub:END;*/

Shader "Effects3y3/EffectsShader" {
    Properties {
        _Brightness ("Brightness", Float ) = 1
        _Intensity ("Intensity", Float ) = 1
        _Color ("Color", Color) = (0.1764706,0.5229208,1,1)
        _Texture ("Texture", 2D) = "white" {}
        _Texture_Decay ("Texture_Decay", 2D) = "white" {}
        _Decay ("Decay", Range(0.05, 0.95)) = 0.223077
        _Power ("Power", Float ) = 1
        _Speed ("Speed", Range(0, 15)) = 0.5
		_Offset("Mesh Offset", Float) = 0.02
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _Power;
            uniform sampler2D _Texture_Decay; uniform float4 _Texture_Decay_ST;
            uniform float4 _Color;
            uniform float _Decay;
            uniform float _Intensity;
            uniform float _Brightness;
            uniform float _Speed;
			uniform float _Offset;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
				v.vertex.xyz += v.normal * _Offset;
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(_Object2World, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                
                float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
                i.normalDir *= nSign;
                normalDirection *= nSign;
                
////// Lighting:
////// Emissive:
                float4 node_2434 = _Time + _TimeEditor;
                float2 node_923 = (i.uv0+(node_2434.g*_Speed)*float2(0,0.1));
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_923, _Texture));
                float node_893 = pow(1.0-max(0,dot(normalDirection, viewDirection)),_Power);
                float2 node_1319 = float2((_Texture_var.r+node_893),((i.uv0.g*0.0)+_Decay));
                float4 _Texture_Decay_var = tex2D(_Texture_Decay,TRANSFORM_TEX(node_1319, _Texture_Decay));
                float3 emissive = ((_Color.rgb*clamp(((_Texture_Decay_var.r*node_893)*_Intensity),0.05,0.95))*_Brightness);
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

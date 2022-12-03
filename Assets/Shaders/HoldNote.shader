﻿Shader "Gameplay/HoldNote"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_From ("From", Float) = 0
		_To ("To",Float) = 1
		_Alpha ("Alpha", Float) = 1
		_Highlight("Highlight", Int) = 0
		_Color ("Color", Color) = (1,1,1,1)
		_Shear("Shear", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true"  }
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
			};
			
			int _Highlight;
			float _From,_To,_Alpha;
			sampler2D _MainTex;
			float4 _Color;
			float4 _Shear;

			v2f vert (appdata v)
			{
				v2f o;
				float4 vertex = v.vertex; 
				float x = _Shear.x;
				float y = _Shear.y;
				float4x4 transformMatrix = float4x4(
                    1,0,x,0,
                    0,1,y,0,
                    0,0,1,0,
                    0,0,0,1);
				vertex = mul(transformMatrix, vertex);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				hsv.r += 0.4f;
				hsv.b += 0.25f;
				return half4(hsv2rgb(hsv),c.a);
			}
			
			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y > _To || i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				i.uv.y = (i.uv.y - 1) * _To/(_To-_From) + 1;
				half4 c = half4(tex2D(_MainTex,i.uv).rgb, _Alpha); 
				if(_Highlight == 1) 
				{
					c = Highlight(c);
				};
				return c * _Color;
			}
			ENDCG
		}
	}
}

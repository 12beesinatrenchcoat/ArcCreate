﻿Shader "Gameplay/Arc"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_ShadowColor ("ShadowColor", Color) = (1,1,1,1)
		_LowColor ("LowColor", Color) = (1,1,1,1)
		_From ("From", Float) = 0
		_Selected ("Selected", Int) = 0
		_ColorTG ("ColorTG", Color) = (1,1,1,1)
		_Shear("Shear", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "CanUseSpriteAtlas"="true"  }

        Cull Off
        Lighting Off
		ZWrite Off 
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
				float4 color    : COLOR;
				float2 uv : TEXCOORD0;
				float2 isShadowUV : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
				float2 uv : TEXCOORD0;
				float3 worldpos : TEXCOORD1;
				float2 isShadowUV : TEXCOORD2;
			};
			 
			int _Selected;
			float _From;
			float4 _Color, _LowColor, _ShadowColor, _ColorTG;
			float4 _Shear;
            float4 _MainTex_ST;
			sampler2D _MainTex;

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

				o.vertex = UnityObjectToClipPos(vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.isShadowUV = v.isShadowUV;

				o.worldpos = mul(unity_ObjectToWorld, vertex);
				if (v.isShadowUV.x > 0)
				{
					o.vertex.y -= o.worldpos.y;
				}
				return o;
			}
			
			half4 Highlight(half4 c)
			{
				fixed3 hsv = rgb2hsv(c.rgb);
				if(c.r<0.5) hsv.r += 0.1f;
				else hsv.g += 1.2f;
				return half4(hsv2rgb(hsv),c.a);
			}

			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.worldpos.z > 100 || i.worldpos.z < -100) return 0;
				float4 c = tex2D(_MainTex,i.uv); 

				bool isShadow = i.isShadowUV > 0;

				float4 inColor = isShadow ?
								 lerp(_LowColor, _Color, clamp((i.worldpos.y - 1) / 4.5f, 0, 1)) :
								 _ShadowColor;

				if(_Selected == 1) 
				{
					inColor = Highlight(inColor);
				}

				c *= inColor * _ColorTG;  
				return c;
			}
			ENDCG
		}
	}
}

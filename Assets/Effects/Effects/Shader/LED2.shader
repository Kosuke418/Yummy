Shader "Custom/LED_Display"
{
	Properties
	{
		[KeywordEnum(REAL_LED, FAKE_LED)]
		_IS_REAL("IS Real LED", Float) = 0

		_BaseColor("Base Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_BlendPower("Base Blend", Range(0.0, 1.0)) = 1.0

		_LedTex("Led Flame(RGBA)", 2D) = "white" {}

		_MainTex("Base map(RGBA)", 2D) = "white" {}
		_MainTex2("Base map2(RGBA)", 2D) = "white" {}

		_MainTex2_BlendPower("Map2 Blend", Range(0.0, 1.0)) = 1.0

		_LineBrightness("Line Brightness", Range(0.0, 1.0)) = 0.2
		_LineSpeed("Line Speed", Float) = 4
		_LineSpacing("Line Spacing", Float) = 4
		_LineSpacing2("Line Spacing2", Float) = 1

		_Near("Near", float) = 0.1
		_Far("Far", float) = 100.0
	}

		SubShader
		{
			Tags{ "Queue" = "Geometry" }
			Blend Off
			Lighting Off
			Fog{ Mode Off }
			ZWrite On
			Cull On


			Pass {

				CGPROGRAM

				#include "UnityCG.cginc"
				#pragma multi_compile _IS_REAL_REAL_LED _IS_REAL_FAKE_LED
				#pragma vertex vert
				#pragma fragment frag


				uniform half4 _BaseColor;
				uniform half _BlendPower;

				uniform sampler2D _LedTex;

				uniform sampler2D _MainTex;
				uniform half4 _MainTex_TexelSize;

				uniform sampler2D _MainTex2;
				uniform half4 _MainTex2_TexelSize;

				uniform half _MainTex2_BlendPower;

				uniform half _LineBrightness;
				uniform half _LineSpeed;
				uniform half _LineSpacing;
				uniform half _LineSpacing2;

				uniform half _Near;
				uniform half _Far;

				struct v2f
				{
					half4 pos : SV_POSITION;
					half2 uv : TEXCOORD0;
					half2 uv2 : TEXCOORD1;
					half2 celluv : TEXCOORD2;
					half depth : TEXCOORD3;
				};

				half4 _MainTex_ST;
				half4 _MainTex2_ST;
				half4 _CellTex3_ST;
				half4 _LedTex_ST;

				half4 col;
				half4 col_mt2;

				static const half2 _Texby1pt = half2(_MainTex_TexelSize.x , _MainTex_TexelSize.y);
				static const half2 _CellSizeXY = 1.0 / _LedTex_ST.xy;


				v2f vert(appdata_base v)
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MV, v.vertex);
					o.depth = o.pos.z;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex2);
					o.celluv = TRANSFORM_TEX(v.texcoord, _LedTex);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{

					#ifdef _IS_REAL_REAL_LED
					//Pixelate for MainTex
					half2 steppedUV = i.uv.xy + _CellSizeXY * 0.5;
					steppedUV /= _CellSizeXY.xy;
					steppedUV = round(steppedUV);
					steppedUV *= _CellSizeXY.xy;
					col = tex2D(_MainTex, steppedUV);

					//Pixelate for MainTex2
					steppedUV = i.uv2.xy + _CellSizeXY * 0.5;
					steppedUV /= _CellSizeXY.xy;
					steppedUV = round(steppedUV);
					steppedUV *= _CellSizeXY.xy;
					col_mt2 = tex2D(_MainTex2, steppedUV);

				#elif _IS_REAL_FAKE_LED
					//Shift Texture color by 1px
					col = tex2D(_MainTex, i.uv);
					col.g = tex2D(_MainTex, i.uv.xy + _Texby1pt.x).g;
					col.b = tex2D(_MainTex, i.uv.xy + _Texby1pt.x).b;

					//Shift Texture color by 1px
					col_mt2 = tex2D(_MainTex2, i.uv2);
					col_mt2.g = tex2D(_MainTex2, i.uv2.xy + _Texby1pt.x).g;
					col_mt2.b = tex2D(_MainTex2, i.uv2.xy + _Texby1pt.x).b;

				#endif

					//Blend Texture Main and Main2farPower
					col.rgb = lerp(col.rgb, col_mt2.rgb, _MainTex2_BlendPower);
					//Blend Base color and Textures.
					col.rgb = lerp(_BaseColor.rgb, col.rgb, _BlendPower);

					// create flame of LED valve.
					fixed4 led_col = tex2D(_LedTex, i.celluv);
					// calc far distance
					half farPower = saturate((-i.depth - _Near) / (_Far - _Near));
					col.rgb = lerp(col.rgb, col.rgb * led_col.rgb, (1 - farPower));

					// 走査線1　uv基準でスキャンラインを描画
					half scanLineColor = sin(_Time.y * _LineSpeed + i.uv.y * _LineSpacing);// / 2 + 0.5;

					// 走査線2　uv基準でスキャンラインを描画
					half scanLineColor2 = sin(_Time.y * _LineSpeed*0.5 + i.uv.y * _LineSpacing*0.1);/// 2 + 0.5;

					// 走査線3　uv基準で横スキャンラインを描画
					half scanLineColor3 = sin(_Time.y * _LineSpeed*0.25 + i.uv.x * _LineSpacing2); //; / 2 + 0.5;
					// スキャンラインを加算
					col += (clamp(scanLineColor, 0, 1.0) + clamp(scanLineColor2, 0, 1.0) + clamp(scanLineColor3, 0, 1)) * (_LineBrightness * (1 - farPower)) * _BlendPower;

					return saturate(col);
				}

				ENDCG
			}

		}

			FallBack "Diffuse"

}
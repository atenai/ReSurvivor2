Shader "Custom/MonoTone"
{
	// ===== インスペクターから設定できる値 =====
	Properties
	{
		// 表示する元のテクスチャ
		_MainTex ("MainTex", 2D) = "" { }
		// グレー化の強さ
		// 0 = 元の色
		// 1 = 完全にグレー
		_GrayAmount ("Gray Amount", Range(0, 1)) = 0.0
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		Pass
		{
			CGPROGRAM

			// Unity が用意している便利な構造体や関数を使う
			#include "UnityCG.cginc"

			// 使用する頂点シェーダーとフラグメントシェーダーを指定
			#pragma vertex vert_img
			#pragma fragment frag

			// ===== プロパティの受け取り =====

			// 元のテクスチャ
			sampler2D _MainTex;
			// グレー化の割合
			half _GrayAmount;
			
			// ===== 頂点シェーダー入力 =====
			// 頂点シェーダー構造体（シェーダーに必須のテンプレート構造体）
			// メッシュが持っている情報を受け取る構造体
			struct appdata
			{
				// 頂点位置（必須）
				float4 vertex : POSITION;
				// テクスチャUV座標
				// tex2Dでどこをサンプリングするかを決めるために必要
				float2 uv : TEXCOORD0;
			};

			// ===== 頂点 → フラグメントに渡すデータ =====
			// フラグメントシェーダー構造体（シェーダーに必須のテンプレート構造体）
			struct v2f
			{
				// 画面上の座標（必須）
				float4 pos : SV_POSITION;
				// フラグメントシェーダーで使うUV
				float2 uv : TEXCOORD0;
			};

			// ===== 頂点シェーダー =====
			// 頂点シェーダー関数（シェーダーに必須のテンプレート関数）
			v2f vert(appdata v)
			{
				v2f o;
				// オブジェクト空間 → クリップ空間へ変換
				o.pos = UnityObjectToClipPos(v.vertex);
				// UVをそのままフラグメントシェーダーへ渡す
				o.uv = v.uv;
				return o;
			}

			// UVをそのままフラグメントシェーダーへ渡す
			// フラグメントシェーダー関数（シェーダーに必須のテンプレート関数）
			fixed4 frag(v2f i) : SV_Target
			{
				// ➀テクスチャをUV座標でサンプリング
				// どのテクスチャを読むか → _MainTex
				// テクスチャのどこを読むか → UV座標
				fixed4 c = tex2D(_MainTex, i.uv);

				// ➁RGB からグレースケール値を計算
				// dot = 内積（r,g,b に重みを掛けて合計）
				half gray = dot(c.rgb, half3(0.3, 0.6, 0.1));
				
				// ➂グレー値を RGB に展開
				// gray.xxx は (gray, gray, gray) と同じ意味
				half3 grayRgb = gray.xxx;

				// ➃元の色とグレー色を補間
				// _GrayAmount = 0 → 元の色
				// _GrayAmount = 1 → 完全にグレー
				//c.rgb = lerp(c.rgb, grayRgb, _GrayAmount);

				c.rgb = grayRgb; // 完全グレー化テスト用
				
				// ➄最終的な色を返す
				return c;
			}
			ENDCG
		}
	}
}

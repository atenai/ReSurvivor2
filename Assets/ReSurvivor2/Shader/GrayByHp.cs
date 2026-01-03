using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HPに応じて画面をグレースケールにするシェーダー制御クラス 
/// </summary>
/// <remarks>
/// シェーダー側で_GrayAmountプロパティを用意しておくこと
/// カメラにアタッチすること
/// </remarks>
public class GrayByHp : MonoBehaviour
{
	[Tooltip("グレースケールにするマテリアル")]
	[SerializeField] Material monoTone;
	[SerializeField] float maxHp = 100f;
	[SerializeField] float hp = 100f;

	/// <summary>
	/// シェーダーのプロパティID（パフォーマンス向上のため事前に取得しておく）
	/// </summary>
	private static readonly int GrayAmountId = Shader.PropertyToID("_GrayAmount");

	void Update()
	{
		// 例：テストで減らす（不要なら消してOK）
		hp = Mathf.Max(0f, hp - Time.deltaTime * 10f);//テスト用

		float hp01 = Mathf.Clamp01(hp / maxHp);// 1=満タン, 0=ゼロ
		float grayAmount = Mathf.Pow(1f - hp01, 2f);// 2fを大きくすると後半で強くなる

		monoTone.SetFloat(GrayAmountId, grayAmount);// シェーダーに値をセット
	}

	/// <summary>
	/// 画面描画直前に呼ばれるUnity公式の関数
	/// </summary>
	/// <param name="src"></param>
	/// <param name="dest"></param>
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, monoTone);
	}

	/// <summary>
	/// 外部からHPの状態を更新する
	/// </summary>
	/// <param name="currentHp"></param>
	public void SetHp(float currentHp)
	{
		hp = currentHp;
	}
}

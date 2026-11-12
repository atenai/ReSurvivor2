using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// ミッションをクリアした時のリザルト画面の見た目
/// MVPパターンのView担当
/// </summary>
public class MissionResultView
{
	[Tooltip("表示している間のアルファ値（うっすらと表示する）")]
	const float ShowAlpha = 0.2f;
	[Tooltip("消えている時のアルファ値")]
	const float HideAlpha = 0.0f;
	[Tooltip("フェードイン・フェードアウトにかける時間（秒）")]
	const float FadeDuration = 0.5f;
	[Tooltip("フェードインしてから、フェードアウトし始めるまで表示し続ける時間（秒）")]
	const float StaySeconds = 1.0f;

	[Tooltip("リザルト画面")]
	readonly Image resultImage;

	public MissionResultView(Image resultImage)
	{
		this.resultImage = resultImage;
	}

	/// <summary>
	/// リザルト画面を、フェードイン → 表示し続ける → フェードアウトさせる（DOTween使用）
	/// </summary>
	/// <returns>フェードの流れ（シーケンス）。終わった時の処理は、呼んだ側がOnCompleteで決める</returns>
	public Sequence Play()
	{
		//前のフェードが残っていたら止める
		resultImage.DOKill();

		//表示して、透明から始める
		resultImage.gameObject.SetActive(true);
		resultImage.color = new Color(resultImage.color.r, resultImage.color.g, resultImage.color.b, HideAlpha);

		Sequence sequence = DOTween.Sequence();
		sequence.Append(resultImage.DOFade(ShowAlpha, FadeDuration));
		sequence.AppendInterval(StaySeconds);
		sequence.Append(resultImage.DOFade(HideAlpha, FadeDuration));
		sequence.Play();
		return sequence;
	}

	/// <summary>
	/// リザルト画面を非表示にする
	/// </summary>
	public void Hide()
	{
		resultImage.gameObject.SetActive(false);
	}
}

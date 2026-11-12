using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ポーズメニューの見た目
/// 開いた時に、全体をフェードインしながら、左のメニューと右の操作説明を外側からスライドさせて表示する
/// ※ポーズ中はTime.timeScaleが0なので、ポーズ中も進む時間（Time.unscaledDeltaTime）で動かす
/// </summary>
public class PauseMenuView : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("ポーズメニュー全体（CanvasGroupのアルファ値でまとめてフェードする）")]
	[SerializeField] CanvasGroup canvasGroup;
	[Tooltip("左側のメニュー（見出しと項目）")]
	[SerializeField] RectTransform menuColumn;
	[Tooltip("右側の操作説明")]
	[SerializeField] RectTransform controlsPanel;
	[Tooltip("メニューの項目（上から順に。ScreenUIManagerPresenterの選択番号と同じ順番）")]
	[SerializeField] PauseMenuItem[] items;

	[Header("開く時の動き")]
	[Tooltip("フェードインとスライドにかける時間（秒）")]
	[SerializeField] float openDuration;
	[Tooltip("スライドする距離（左のメニューは左から、右の操作説明は右から入ってくる）")]
	[SerializeField] float slideDistance;
	[Tooltip("右の操作説明が動き始めるまでの遅れ（秒）")]
	[SerializeField] float controlsPanelDelay;

	[Tooltip("開いた瞬間の時間")]
	const float OpenStartTime = 0.0f;
	[Tooltip("動きの終わりの割合")]
	const float EndRate = 1.0f;
	[Tooltip("動きの終わりをゆっくりにする強さ（3乗で減速する）")]
	const float EaseOutPower = 3.0f;

	/// <summary>メニューの項目の数</summary>
	public int ItemCount => items.Length;

	/// <summary>選択中の項目の番号</summary>
	int selectedIndex = 0;
	/// <summary>開いてからの時間（ポーズ中も進む時間）</summary>
	float openTime = OpenStartTime;
	/// <summary>左のメニューの、スライドし終わった時の位置</summary>
	Vector2 menuColumnBasePosition;
	/// <summary>右の操作説明の、スライドし終わった時の位置</summary>
	Vector2 controlsPanelBasePosition;
	/// <summary>スライドし終わった時の位置を覚えたか？</summary>
	bool isBasePositionSaved = false;

	/// <summary>
	/// メニューを開いた時の初期化（フェードインとスライドを最初から始める）
	/// </summary>
	/// <param name="selectedIndex">最初に選択しておく項目の番号</param>
	public void Open(int selectedIndex)
	{
		if (isBasePositionSaved == false)
		{
			menuColumnBasePosition = menuColumn.anchoredPosition;
			controlsPanelBasePosition = controlsPanel.anchoredPosition;
			isBasePositionSaved = true;
		}

		this.selectedIndex = selectedIndex;
		openTime = OpenStartTime;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].SetSelectedImmediately(i == selectedIndex);
		}
		ApplyOpenAnimation();
	}

	/// <summary>
	/// 選択中の項目を変える
	/// </summary>
	/// <param name="selectedIndex">選択する項目の番号</param>
	public void SetSelectedIndex(int selectedIndex)
	{
		this.selectedIndex = selectedIndex;
	}

	/// <summary>
	/// 見た目の更新（ポーズ中に毎フレーム呼ぶ）
	/// </summary>
	/// <param name="unscaledDeltaTime">前のフレームからの時間（ポーズ中も進む時間）</param>
	public void UpdateView(float unscaledDeltaTime)
	{
		openTime = openTime + unscaledDeltaTime;
		ApplyOpenAnimation();

		for (int i = 0; i < items.Length; i++)
		{
			items[i].UpdateHighlight(i == selectedIndex, unscaledDeltaTime);
		}
	}

	/// <summary>
	/// 開いてからの時間に合わせて、フェードインとスライドを反映する
	/// </summary>
	void ApplyOpenAnimation()
	{
		float menuRate = EaseOut(openTime / openDuration);
		float controlsRate = EaseOut((openTime - controlsPanelDelay) / openDuration);

		canvasGroup.alpha = menuRate;
		//左のメニューは左から、右の操作説明は右から、スライドし終わった位置へ入ってくる
		menuColumn.anchoredPosition = menuColumnBasePosition + Vector2.left * slideDistance * (EndRate - menuRate);
		controlsPanel.anchoredPosition = controlsPanelBasePosition + Vector2.right * slideDistance * (EndRate - controlsRate);
	}

	/// <summary>
	/// 動きの割合を、終わりに向かってゆっくりになる割合に変える
	/// </summary>
	/// <param name="rate">動きの割合（0〜1の外の値は0か1にそろえる）</param>
	/// <returns>終わりに向かってゆっくりになる割合（0〜1）</returns>
	float EaseOut(float rate)
	{
		//Mathf.Clamp01は、値を0〜1の間に収める
		float clampedRate = Mathf.Clamp01(rate);
		return EndRate - Mathf.Pow(EndRate - clampedRate, EaseOutPower);
	}
}

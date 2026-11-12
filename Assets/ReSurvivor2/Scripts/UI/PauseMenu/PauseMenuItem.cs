using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ポーズメニューの項目1つ分（「RESUME」「QUIT GAME」など）
/// 選択中かどうかで、背景・文字の色と、左の印・文字の位置をなめらかに切り替える
/// </summary>
public class PauseMenuItem : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("項目の背景")]
	[SerializeField] Image imageBackground;
	[Tooltip("選択中に左端に出す印")]
	[SerializeField] Image imageAccent;
	[Tooltip("項目名のテキスト")]
	[SerializeField] TextMeshProUGUI textLabel;

	[Header("色")]
	[Tooltip("選択されていない時の背景の色")]
	[SerializeField] Color normalBackgroundColor;
	[Tooltip("選択中の背景の色")]
	[SerializeField] Color selectedBackgroundColor;
	[Tooltip("選択されていない時の文字の色")]
	[SerializeField] Color normalLabelColor;
	[Tooltip("選択中の文字の色")]
	[SerializeField] Color selectedLabelColor;

	[Header("動き")]
	[Tooltip("選択中の時に、項目名を右へずらす距離")]
	[SerializeField] float selectedLabelOffset;
	[Tooltip("選択の切り替えの速さ（1秒間に変化する割合）")]
	[SerializeField] float highlightSpeed;

	[Tooltip("選択中の割合（選択されていない）")]
	const float NotSelectedRate = 0.0f;
	[Tooltip("選択中の割合（選択中）")]
	const float SelectedRate = 1.0f;

	/// <summary>今の選択中の割合（0＝選択されていない、1＝選択中。切り替えの途中は間の値になる）</summary>
	float highlightRate = NotSelectedRate;
	/// <summary>選択されていない時の項目名の位置</summary>
	Vector2 labelBasePosition;
	/// <summary>項目名の位置を覚えたか？</summary>
	bool isLabelBasePositionSaved = false;

	/// <summary>
	/// 選択中かどうかの見た目を、切り替えの途中を飛ばしてすぐに反映する（メニューを開いた時に使う）
	/// </summary>
	/// <param name="isSelected">選択中か？</param>
	public void SetSelectedImmediately(bool isSelected)
	{
		highlightRate = isSelected == true ? SelectedRate : NotSelectedRate;
		ApplyHighlight();
	}

	/// <summary>
	/// 選択中かどうかの見た目を、なめらかに切り替える（毎フレーム呼ぶ）
	/// </summary>
	/// <param name="isSelected">選択中か？</param>
	/// <param name="unscaledDeltaTime">前のフレームからの時間（ポーズ中も進む時間）</param>
	public void UpdateHighlight(bool isSelected, float unscaledDeltaTime)
	{
		float targetRate = isSelected == true ? SelectedRate : NotSelectedRate;
		highlightRate = Mathf.MoveTowards(highlightRate, targetRate, highlightSpeed * unscaledDeltaTime);
		ApplyHighlight();
	}

	/// <summary>
	/// 今の選択中の割合を、背景・文字の色と、印・文字の位置に反映する
	/// </summary>
	void ApplyHighlight()
	{
		if (isLabelBasePositionSaved == false)
		{
			labelBasePosition = textLabel.rectTransform.anchoredPosition;
			isLabelBasePositionSaved = true;
		}

		//選択されていない時の見た目と選択中の見た目を、選択中の割合で混ぜる
		imageBackground.color = Color.Lerp(normalBackgroundColor, selectedBackgroundColor, highlightRate);
		textLabel.color = Color.Lerp(normalLabelColor, selectedLabelColor, highlightRate);
		Color accentColor = imageAccent.color;
		accentColor.a = highlightRate;
		imageAccent.color = accentColor;
		textLabel.rectTransform.anchoredPosition = labelBasePosition + Vector2.right * selectedLabelOffset * highlightRate;
	}
}

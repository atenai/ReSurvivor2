using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コンピューターメニューの「このミッションを始めますか？」のYes/Noダイアログの見た目
/// MVPパターンのView担当
/// </summary>
public class YesNoDialogView
{
	[Tooltip("Yes/Noボタンのイメージリストの中の「いいえ」ボタンの番号")]
	const int NoButtonIndex = 0;
	[Tooltip("Yes/Noボタンのイメージリストの中の「はい」ボタンの番号")]
	const int YesButtonIndex = 1;
	[Tooltip("選んでいるボタンの背景のアルファ値")]
	const float SelectedButtonAlpha = 0.5f;
	[Tooltip("選んでいないボタンの背景のアルファ値")]
	const float NotSelectedButtonAlpha = 0.0f;

	[Tooltip("Yes/Noダイアログ")]
	readonly Image yesNoDialogImage;
	[Tooltip("Yes/Noボタンのイメージリスト（「いいえ」「はい」の順番）")]
	readonly Image[] yesNoButtonImages;

	public YesNoDialogView(Image yesNoDialogImage, Image[] yesNoButtonImages)
	{
		this.yesNoDialogImage = yesNoDialogImage;
		this.yesNoButtonImages = yesNoButtonImages;
	}

	/// <summary>
	/// Yes/Noダイアログを表示する
	/// </summary>
	public void Show()
	{
		yesNoDialogImage.gameObject.SetActive(true);
	}

	/// <summary>
	/// Yes/Noダイアログを非表示にする
	/// </summary>
	public void Hide()
	{
		yesNoDialogImage.gameObject.SetActive(false);
	}

	/// <summary>
	/// 選んでいるボタンだけ、背景を表示する
	/// </summary>
	/// <param name="isYesSelected">「はい」を選んでいるか？</param>
	public void SetSelected(bool isYesSelected)
	{
		SetButtonSelected(yesNoButtonImages[YesButtonIndex], isYesSelected);
		SetButtonSelected(yesNoButtonImages[NoButtonIndex], isYesSelected == false);
	}

	/// <summary>
	/// ボタン1つ分の背景を、選んでいるかどうかで切り替える
	/// </summary>
	/// <param name="buttonImage">ボタンのイメージ</param>
	/// <param name="isSelected">選んでいるか？</param>
	void SetButtonSelected(Image buttonImage, bool isSelected)
	{
		float alpha = NotSelectedButtonAlpha;
		if (isSelected == true)
		{
			alpha = SelectedButtonAlpha;
		}
		buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, alpha);
	}
}

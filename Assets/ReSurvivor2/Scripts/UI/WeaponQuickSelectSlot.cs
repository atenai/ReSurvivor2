using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 武器クイックセレクトUIの1マス分（十字キーの1方向）
/// </summary>
public class WeaponQuickSelectSlot : MonoBehaviour
{
	[Header("UI")]
	[Tooltip("スロットの背景")]
	[SerializeField] Image imageBackground;
	[Tooltip("スロットの枠")]
	[SerializeField] Image imageFrame;
	[Tooltip("弾数（地雷の場合は所持数）のテキスト")]
	[SerializeField] TextMeshProUGUI textAmmo;
	[Tooltip("このスロットに対応する十字キーの方向の画像")]
	[SerializeField] Image imageDPadDirection;

	[Header("色")]
	[Tooltip("選択されていない時の背景の色")]
	[SerializeField] Color normalBackgroundColor;
	[Tooltip("選択中の背景の色")]
	[SerializeField] Color selectedBackgroundColor;
	[Tooltip("選択されていない時の枠の色")]
	[SerializeField] Color normalFrameColor;
	[Tooltip("選択中の枠の色")]
	[SerializeField] Color selectedFrameColor;
	[Tooltip("選択されていない時の十字キーの方向の色")]
	[SerializeField] Color normalDPadDirectionColor;
	[Tooltip("選択中の十字キーの方向の色")]
	[SerializeField] Color selectedDPadDirectionColor;
	[Tooltip("マガジン内の弾数の文字の色（「7/35」の「7」の色）")]
	[SerializeField] Color magazineTextColor;

	/// <summary>
	/// 選択中かどうかで見た目を切り替える
	/// </summary>
	/// <param name="isSelected">選択中か？</param>
	public void SetSelected(bool isSelected)
	{
		if (isSelected == true)
		{
			imageBackground.color = selectedBackgroundColor;
			imageFrame.color = selectedFrameColor;
			imageDPadDirection.color = selectedDPadDirectionColor;
		}
		else
		{
			imageBackground.color = normalBackgroundColor;
			imageFrame.color = normalFrameColor;
			imageDPadDirection.color = normalDPadDirectionColor;
		}
	}

	/// <summary>
	/// 弾数テキストを「マガジン内の弾数/残弾数」の形で表示する
	/// </summary>
	/// <param name="currentMagazine">マガジン内の弾数</param>
	/// <param name="currentAmmo">残弾数</param>
	public void SetAmmoText(int currentMagazine, int currentAmmo)
	{
		//ColorUtility.ToHtmlStringRGBは色を「8FD18F」のような16進数の文字列に変換する
		//TextMeshProの<color=#8FD18F>～</color>で囲んだ部分だけ文字の色が変わる
		textAmmo.text = "<color=#" + ColorUtility.ToHtmlStringRGB(magazineTextColor) + ">" + currentMagazine + "</color>/" + currentAmmo;
	}

	/// <summary>
	/// 所持数テキストを表示する（地雷など弾数ではない物用）
	/// </summary>
	/// <param name="count">所持数</param>
	public void SetCountText(int count)
	{
		textAmmo.text = count.ToString();
	}
}

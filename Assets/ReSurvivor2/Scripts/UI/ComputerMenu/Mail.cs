using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// メール（コンピューターメニューのメール一覧の1通分。1通が1つのミッション）
/// </summary>
public class Mail : MonoBehaviour
{
	[Tooltip("選んでいるメールの背景の色")]
	static readonly Color32 SelectedBGColor = new Color32(0, 100, 255, 120);
	[Tooltip("選んでいないメールの背景の色")]
	static readonly Color32 NotSelectedBGColor = new Color32(100, 100, 100, 120);

	[SerializeField] Image imageMailBG;
	[SerializeField] Image imageClearCheckIcon;
	[SerializeField] Text textMissionName;

	int missionID;
	public int MissionID
	{
		get { return missionID; }
		set { missionID = value; }
	}

	public void Initialize(int missionID, string missionName)
	{
		this.missionID = missionID;
		ClearCheck();
		textMissionName.text = missionName;
	}

	/// <summary>
	/// 選んでいるかどうかで、背景の色を切り替える
	/// </summary>
	/// <param name="isSelected">選んでいるか？</param>
	public void SetSelected(bool isSelected)
	{
		if (isSelected == true)
		{
			imageMailBG.color = SelectedBGColor;
		}
		else
		{
			imageMailBG.color = NotSelectedBGColor;
		}
	}

	/// <summary>
	/// ミッションがクリアされているかどうかの判定をして、チェックマークの表示・非表示を切り替える
	/// </summary>
	void ClearCheck()
	{
		//ミッションがクリアされているかどうかの判定
		if (MissionManager.SingletonInstance.IsMissionIDClearCheck(this.missionID) == true)
		{
			//クリアされている場合はチェックマークを表示する
			imageClearCheckIcon.gameObject.SetActive(true);
		}
		else
		{
			//クリアされていない場合はチェックマークを非表示にする
			imageClearCheckIcon.gameObject.SetActive(false);
		}
	}
}
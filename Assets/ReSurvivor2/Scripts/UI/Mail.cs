using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// メール
/// </summary>
public class Mail : MonoBehaviour
{
	[SerializeField] Image imageMailBG;
	public Image ImageMailBG => imageMailBG;
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
	/// ミッションがクリアされているかどうかの判定をして、チェックマークの表示・非表示を切り替える
	/// </summary>
	void ClearCheck()
	{
		//ミッションがクリアされているかどうかの判定
		if (InGameManager.SingletonInstance.IsMissionIDClearCheck(this.missionID))
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
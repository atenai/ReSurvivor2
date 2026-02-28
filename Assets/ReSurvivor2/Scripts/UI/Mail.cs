using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// メール
/// </summary>
public class Mail : MonoBehaviour
{
	[SerializeField] Image image_MailBG;
	public Image Image_MailBG => image_MailBG;
	[SerializeField] Image image_ClearCheckIcon;
	[SerializeField] TextMeshProUGUI textMeshProUGUI;

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
		textMeshProUGUI.text = missionName;
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
			image_ClearCheckIcon.gameObject.SetActive(true);
		}
		else
		{
			//クリアされていない場合はチェックマークを非表示にする
			image_ClearCheckIcon.gameObject.SetActive(false);
		}
	}
}
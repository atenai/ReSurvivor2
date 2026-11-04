using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コンピューターメニューのメール一覧の見た目
/// MVPパターンのView担当
/// コンピューターメニューのパネル・メールの一覧・選んでいるメールの本文を表示する
/// </summary>
public class MailListView
{
	[Tooltip("ミッションが無い時に、メールのタイトルと本文に表示する文字")]
	const string NoMissionMessage = "-----";

	[Tooltip("コンピューターメニューのパネル（メール一覧とYes/Noダイアログが入っている）")]
	readonly Image panelComputerMenu;
	[Tooltip("メールを並べる親（スクロールビューのContent）")]
	readonly GameObject mailListContent;
	[Tooltip("メール1通分のプレハブ")]
	readonly GameObject mailPrefab;
	[Tooltip("選んでいるメールのタイトル")]
	readonly Text mailTitleMessage;
	[Tooltip("選んでいるメールの本文")]
	readonly Text mailMainMessage;
	[Tooltip("並べたメール（ミッションのリストと同じ順番）")]
	readonly List<Mail> mailList = new List<Mail>();

	public MailListView(Image panelComputerMenu, GameObject mailListContent, GameObject mailPrefab, Text mailTitleMessage, Text mailMainMessage)
	{
		this.panelComputerMenu = panelComputerMenu;
		this.mailListContent = mailListContent;
		this.mailPrefab = mailPrefab;
		this.mailTitleMessage = mailTitleMessage;
		this.mailMainMessage = mailMainMessage;
	}

	/// <summary>
	/// コンピューターメニューのパネルを表示する
	/// </summary>
	public void Show()
	{
		panelComputerMenu.gameObject.SetActive(true);
	}

	/// <summary>
	/// コンピューターメニューのパネルを非表示にする
	/// </summary>
	public void Hide()
	{
		panelComputerMenu.gameObject.SetActive(false);
	}

	/// <summary>
	/// メールを作り直す（今あるメールを全部消して、ミッション1つにつき1通のメールを並べる）
	/// </summary>
	/// <param name="missionList">ミッションのリスト</param>
	public void CreateMails(List<MasterMissionEntity> missionList)
	{
		//前のステージのメールを全部消す
		foreach (Transform mailContent in mailListContent.transform)
		{
			UnityEngine.Object.Destroy(mailContent.gameObject);
		}
		mailList.Clear();

		//プレハブをInstantiateしてContentの子オブジェクトに並べる
		foreach (MasterMissionEntity mission in missionList)
		{
			Mail mail = UnityEngine.Object.Instantiate(mailPrefab, Vector3.zero, Quaternion.identity, mailListContent.transform).GetComponent<Mail>();
			mail.Initialize(mission.MissionID, mission.MissionName);
			mailList.Add(mail);
		}
	}

	/// <summary>
	/// 選んでいるメールだけを、選択中の色にする
	/// </summary>
	/// <param name="selectedMailIndex">選んでいるメールの番号</param>
	public void SetSelectedMail(int selectedMailIndex)
	{
		for (int i = 0; i < mailList.Count; i++)
		{
			mailList[i].SetSelected(i == selectedMailIndex);
		}
	}

	/// <summary>
	/// メールのタイトルと本文を表示する
	/// </summary>
	/// <param name="mission">表示するメールのミッション</param>
	public void ShowMessage(MasterMissionEntity mission)
	{
		mailTitleMessage.text = mission.MailTitleMessage;
		mailMainMessage.text = mission.MailMainMessage;
	}

	/// <summary>
	/// ミッションが無い時の、空のタイトルと本文を表示する
	/// </summary>
	public void ShowNoMissionMessage()
	{
		mailTitleMessage.text = NoMissionMessage;
		mailMainMessage.text = NoMissionMessage;
	}
}

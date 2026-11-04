using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンピューターメニューのモデル
/// MVPパターンのModel担当
/// 今の画面・受けられるミッション・選んでいるメールとボタンを持つ
/// </summary>
public class ComputerMenuModel
{
	[Tooltip("一番上のメールの番号")]
	const int FirstMailIndex = 0;
	[Tooltip("番号は0から始まるので、一番下のメールの番号は、メールの数よりこの分だけ小さい")]
	const int LastMailIndexOffset = 1;

	[Tooltip("今の画面")]
	EnumManager.ComputerMenuStateTYPE state = EnumManager.ComputerMenuStateTYPE.Closed;
	public EnumManager.ComputerMenuStateTYPE State
	{
		get { return state; }
		set { state = value; }
	}

	[Tooltip("このステージのコンピューターで受けられるミッションのリスト（1つのミッションが1通のメールになる）")]
	readonly List<MasterMissionEntity> missionList = new List<MasterMissionEntity>();
	public List<MasterMissionEntity> MissionList => missionList;

	[Tooltip("メール一覧で選んでいるメールの番号")]
	int selectedMailIndex = FirstMailIndex;
	public int SelectedMailIndex => selectedMailIndex;

	[Tooltip("Yes/Noダイアログで「はい」を選んでいるか？（falseの時は「いいえ」を選んでいる）")]
	bool isYesSelected = false;
	public bool IsYesSelected => isYesSelected;

	/// <summary>
	/// 受けられるミッションがあるか？
	/// </summary>
	public bool HasMission => 0 < missionList.Count;

	/// <summary>
	/// 選んでいるメールのミッション
	/// </summary>
	public MasterMissionEntity SelectedMission => missionList[selectedMailIndex];

	/// <summary>
	/// 受けられるミッションのリストを入れ替える
	/// </summary>
	/// <param name="missionList">このステージのコンピューターで受けられるミッションのリスト</param>
	public void SetMissionList(List<MasterMissionEntity> missionList)
	{
		this.missionList.Clear();
		this.missionList.AddRange(missionList);
	}

	/// <summary>
	/// 一番上のメールを選ぶ
	/// </summary>
	public void SelectFirstMail()
	{
		selectedMailIndex = FirstMailIndex;
	}

	/// <summary>
	/// 1つ上のメールを選ぶ（一番上の時は、一番下のメールへ回り込む）
	/// </summary>
	public void SelectPreviousMail()
	{
		selectedMailIndex--;
		if (selectedMailIndex < FirstMailIndex)
		{
			selectedMailIndex = missionList.Count - LastMailIndexOffset;
		}
	}

	/// <summary>
	/// 1つ下のメールを選ぶ（一番下の時は、一番上のメールへ回り込む）
	/// </summary>
	public void SelectNextMail()
	{
		selectedMailIndex++;
		if (missionList.Count <= selectedMailIndex)
		{
			selectedMailIndex = FirstMailIndex;
		}
	}

	/// <summary>
	/// Yes/Noダイアログで「いいえ」を選ぶ（ダイアログを開いた時は、まず「いいえ」を選んでおく）
	/// </summary>
	public void SelectNo()
	{
		isYesSelected = false;
	}

	/// <summary>
	/// Yes/Noダイアログの「はい」と「いいえ」の選択を切り替える
	/// （ボタンは2つなので、左右どちらを押しても、もう片方のボタンに移る）
	/// </summary>
	public void ToggleYesNo()
	{
		if (isYesSelected == true)
		{
			isYesSelected = false;
		}
		else
		{
			isYesSelected = true;
		}
	}
}

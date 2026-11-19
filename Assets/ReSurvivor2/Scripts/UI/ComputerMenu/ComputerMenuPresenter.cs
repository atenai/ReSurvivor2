using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// コンピューターメニューのプレゼンター
/// MVPパターンのPresenter担当
/// コンピューターメニューの処理の流れ（どの画面からどの画面へ移るか）は、全部このクラスに書く
/// </summary>
/// <remarks>
/// 画面の流れ（EnumManager.ComputerMenuStateTYPE）
/// ・Closed（閉じている）
///     → コンピューターでFキー（ミッション中でない）：Open() → MailList
///     → ゴールのコンピューターでFキー：ShowMissionResult() → MissionResult
/// ・MissionResult（リザルト画面）
///     → リザルト画面が消えたら：Open() → MailList
/// ・MailList（メール一覧）
///     → 決定：OpenYesNoDialog() → YesNoDialog
///     → キャンセル：Close() → Closed
/// ・YesNoDialog（「このミッションを始めますか？」）
///     → 「はい」で決定：StartSelectedMission()（ミッションを始めて閉じる） → Closed
///     → 「いいえ」で決定：CloseYesNoDialog() → MailList
///     → キャンセル：Close() → Closed
/// </remarks>
public class ComputerMenuPresenter
{
	[Tooltip("コンピューターメニューのモデル（今の画面・ミッション・選んでいるメールとボタン）")]
	readonly ComputerMenuModel model = new ComputerMenuModel();
	[Tooltip("メール一覧の見た目")]
	readonly MailListView mailListView;
	[Tooltip("Yes/Noダイアログの見た目")]
	readonly YesNoDialogView yesNoDialogView;
	[Tooltip("リザルト画面の見た目")]
	readonly MissionResultView missionResultView;

	/// <summary>
	/// コンピューターメニューを使っているか？（リザルト画面を表示している間も含む）
	/// 使っている間は、プレイヤーを動かさない・コンピューターのFキーを受け付けない
	/// </summary>
	public bool IsOpen => model.State != EnumManager.ComputerMenuStateTYPE.Closed;

	public ComputerMenuPresenter(MailListView mailListView, YesNoDialogView yesNoDialogView, MissionResultView missionResultView)
	{
		this.mailListView = mailListView;
		this.yesNoDialogView = yesNoDialogView;
		this.missionResultView = missionResultView;
	}

	/// <summary>
	/// このステージのコンピューターで受けられるミッションを受け取って、メールを並べる（ステージの開始時に呼ぶ）
	/// </summary>
	/// <param name="missionList">このステージのコンピューターで受けられるミッションのリスト</param>
	public void SetMissionList(List<MasterMissionEntity> missionList)
	{
		model.SetMissionList(missionList);
		mailListView.CreateMails(model.MissionList);
	}

	/// <summary>
	/// メール一覧を開く（一番上のメールを選んだ状態で開く）
	/// </summary>
	public void Open()
	{
		model.State = EnumManager.ComputerMenuStateTYPE.MailList;
		model.SelectFirstMail();
		ShowSelectedMail();
		mailListView.Show();
	}

	/// <summary>
	/// コンピューターメニューを閉じる
	/// </summary>
	public void Close()
	{
		model.State = EnumManager.ComputerMenuStateTYPE.Closed;
		yesNoDialogView.Hide();
		mailListView.Hide();
	}

	/// <summary>
	/// ミッションのリザルト画面を表示して、消えたらメール一覧を開く（ミッションをクリアした時に呼ぶ）
	/// </summary>
	public void ShowMissionResult()
	{
		//リザルト画面を表示している間も、プレイヤーを止めておく
		model.State = EnumManager.ComputerMenuStateTYPE.MissionResult;

		Sequence resultSequence = missionResultView.Play();
		//リザルト画面が消えたら、続けて次のミッションを選べるように、メール一覧を開く
		resultSequence.OnComplete(() =>
		{
			missionResultView.Hide();
			Open();
		});
	}

	/// <summary>
	/// 今の画面に合わせて、入力を処理する（InGameManagerから毎フレーム呼ぶ）
	/// </summary>
	public void UpdateMenu()
	{
		//※1フレームで処理するのは、今の画面1つだけ
		//  （メール一覧で決定してYes/Noダイアログを開いたフレームに、同じ決定の入力で「いいえ」を決定しないようにするため）
		if (model.State == EnumManager.ComputerMenuStateTYPE.MailList)
		{
			UpdateMailList();
		}
		else if (model.State == EnumManager.ComputerMenuStateTYPE.YesNoDialog)
		{
			UpdateYesNoDialog();
		}
		//閉じている時と、リザルト画面を表示している時は、入力を受け付けない
	}

	/// <summary>
	/// メール一覧の入力（上下でメールを選ぶ・決定でYes/Noダイアログを開く・キャンセルで閉じる）
	/// </summary>
	void UpdateMailList()
	{
		if (ComputerMenuInput.IsCancelPressed() == true)
		{
			Close();
			return;
		}

		//ミッションが無い時は、閉じることしかできない
		if (model.HasMission == false)
		{
			return;
		}

		if (ComputerMenuInput.IsUpPressed() == true)
		{
			model.SelectPreviousMail();
			ShowSelectedMail();
		}
		else if (ComputerMenuInput.IsDownPressed() == true)
		{
			model.SelectNextMail();
			ShowSelectedMail();
		}

		if (ComputerMenuInput.IsDecidePressed() == true)
		{
			OpenYesNoDialog();
		}
	}

	/// <summary>
	/// Yes/Noダイアログの入力（左右で「はい」「いいえ」を選ぶ・決定・キャンセルで閉じる）
	/// </summary>
	void UpdateYesNoDialog()
	{
		if (ComputerMenuInput.IsCancelPressed() == true)
		{
			Close();
			return;
		}

		if (ComputerMenuInput.IsLeftPressed() == true || ComputerMenuInput.IsRightPressed() == true)
		{
			model.ToggleYesNo();
			yesNoDialogView.SetSelected(model.IsYesSelected);
		}

		if (ComputerMenuInput.IsDecidePressed() == true)
		{
			if (model.IsYesSelected == true)
			{
				StartSelectedMission();
			}
			else
			{
				CloseYesNoDialog();
			}
		}
	}

	/// <summary>
	/// 選んでいるメールの色を変えて、そのメールのタイトルと本文を表示する
	/// </summary>
	void ShowSelectedMail()
	{
		mailListView.SetSelectedMail(model.SelectedMailIndex);
		if (model.HasMission == true)
		{
			mailListView.ShowMessage(model.SelectedMission);
		}
		else
		{
			mailListView.ShowNoMissionMessage();
		}
	}

	/// <summary>
	/// Yes/Noダイアログを開く（「いいえ」を選んだ状態で開く）
	/// </summary>
	void OpenYesNoDialog()
	{
		model.State = EnumManager.ComputerMenuStateTYPE.YesNoDialog;
		model.SelectNo();
		yesNoDialogView.SetSelected(model.IsYesSelected);
		yesNoDialogView.Show();
	}

	/// <summary>
	/// Yes/Noダイアログを閉じて、メール一覧に戻る
	/// </summary>
	void CloseYesNoDialog()
	{
		model.State = EnumManager.ComputerMenuStateTYPE.MailList;
		yesNoDialogView.Hide();
	}

	/// <summary>
	/// 選んでいるメールのミッションを始めて、コンピューターメニューを閉じる
	/// </summary>
	void StartSelectedMission()
	{
		MissionManager.SingletonInstance.StartMission(model.SelectedMission);
		Close();
	}
}

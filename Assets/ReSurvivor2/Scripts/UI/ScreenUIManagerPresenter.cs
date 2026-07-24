using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using DG.Tweening;

/// <summary>
/// スクリーンUIを管理するマネージャークラス
/// MVPパターンのPresenter担当
/// </summary>
public class ScreenUIManagerPresenter : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static ScreenUIManagerPresenter singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static ScreenUIManagerPresenter SingletonInstance => singletonInstance;

	[SerializeField] ScreenUIView screenUIView;
	public ScreenUIView ScreenUIView => screenUIView;

	ScreenUIModel screenUIModel;
	public ScreenUIModel ScreenUIModel => screenUIModel;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		screenUIModel = new ScreenUIModel();
		HideYesNoDialog();
		FadeOut();
	}

	/// <summary>
	/// フェードアウト処理（画面を暗くする）
	/// </summary>
	public void FadeOut()
	{
		InGameManager.SingletonInstance.IsGamePlayReady = false;
		screenUIView.ImageFade.color = new Color(screenUIView.ImageFade.color.r, screenUIView.ImageFade.color.g, screenUIView.ImageFade.color.b, 1);
	}

	/// <summary>
	/// フェードイン処理（画面を透明にする）
	/// </summary>
	public IEnumerator FadeIn()
	{
		//現在のアルファ値を取得
		Color color = screenUIView.ImageFade.color;
		float alpha = color.a;

		//アルファ値が0になるまで徐々に減らす
		while (0 < alpha)
		{
			alpha -= Time.deltaTime * screenUIModel.FadeSpeed;
			screenUIView.ImageFade.color = new Color(color.r, color.g, color.b, alpha);
			yield return null;
		}

		//最後に完全に透明にする
		screenUIView.ImageFade.color = new Color(color.r, color.g, color.b, 0);

		InGameManager.SingletonInstance.IsGamePlayReady = true;
	}

	public void InputUpdate(bool isPause)
	{
		PauseMenuActive(isPause);
	}

	public void BeforeUpdate1()
	{
		UpdatePauseMenuSystem();
	}

	public void BeforeUpdate2()
	{
		UpdateComputerMenuSystem();
		if (screenUIModel.SkipYesNoDialogInput == true)
		{
			// 表示直後のフレームは入力を消費して無視する
			screenUIModel.SkipYesNoDialogInput = false;
		}
		else
		{
			UpdateYesNoDialog();
		}
	}

	public void AfterUpdate()
	{
		screenUIView.Crosshair(PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim, PlayerCameraManager.SingletonInstance.IsTargetHit);
		UpdateHitReticule();
		UpdateDamageEffect();
		UpdateHpHealEffect();
		UpdateStaminaHealEffect();

		if (Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("XInput Pause"))
		{
			screenUIView.MapUI.EnableMap();
		}
	}

	/// <summary>
	/// ヒットレティクル
	/// </summary> 
	public void UpdateHitReticule()
	{
		if (screenUIModel.IsHitReticule == true)
		{
			screenUIView.ImageHitReticule.color = new Color32(255, 0, 0, 150);
		}

		if (screenUIModel.IsHitReticule == false)
		{
			screenUIView.ImageHitReticule.color = Color.Lerp(screenUIView.ImageHitReticule.color, Color.clear, Time.deltaTime * screenUIModel.HitReticuleSpeed);
		}

		screenUIModel.IsHitReticule = false;
	}

	/// <summary>
	/// ダメージ画像エフェクト
	/// </summary> 
	void UpdateDamageEffect()
	{
		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsDamage == true)
		{
			screenUIView.ImageDamage.color = new Color(0.5f, 0f, 0f, 0.5f);
		}

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsDamage == false)
		{
			screenUIView.ImageDamage.color = Color.Lerp(screenUIView.ImageDamage.color, Color.clear, Time.deltaTime);
		}

		PlayerManagerPresenter.SingletonInstance.PlayerModel.IsDamage = false;
	}

	/// <summary>
	/// HP回復画像エフェクト
	/// </summary> 
	void UpdateHpHealEffect()
	{
		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsHpHeal == true)
		{
			screenUIView.ImageHpHeal.color = new Color(0f, 0.5f, 0f, 0.5f);
		}

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsHpHeal == false)
		{
			screenUIView.ImageHpHeal.color = Color.Lerp(screenUIView.ImageHpHeal.color, Color.clear, Time.deltaTime);
		}

		PlayerManagerPresenter.SingletonInstance.PlayerModel.IsHpHeal = false;
	}

	/// <summary>
	/// スタミナ回復画像エフェクト
	/// </summary> 
	void UpdateStaminaHealEffect()
	{
		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsStaminaHeal == true)
		{
			screenUIView.ImageStaminaHeal.color = new Color(0.5f, 0.5f, 0f, 0.5f);
		}

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsStaminaHeal == false)
		{
			screenUIView.ImageStaminaHeal.color = Color.Lerp(screenUIView.ImageStaminaHeal.color, Color.clear, Time.deltaTime);
		}

		PlayerManagerPresenter.SingletonInstance.PlayerModel.IsStaminaHeal = false;
	}

	/// <summary>
	/// ポーズメニューのオン/オフ
	/// </summary>
	public void PauseMenuActive(bool isPause)
	{
		if (isPause == true)
		{
			ShowPauseMenu();
		}
		else
		{
			HidePauseMenu();
		}
	}

	public void ShowPauseMenu()
	{
		screenUIView.PanelPauseMenu.gameObject.SetActive(true);
		Time.timeScale = 0f;

		//各種パラメーターの初期化処理
		screenUIModel.CurrentPauseMenuSelectedIndex = 0;
		ChangeColorPauseMenuImage();
	}

	public void HidePauseMenu()
	{
		screenUIView.PanelPauseMenu.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	/// <summary>
	/// ポーズメニュー画面の更新
	/// </summary> 
	public void UpdatePauseMenuSystem()
	{
		if (screenUIView.PanelPauseMenu.gameObject.activeSelf == false)
		{
			return;
		}

		//左右の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown)
		{
			screenUIModel.CurrentPauseMenuSelectedIndex--;
			if (screenUIModel.CurrentPauseMenuSelectedIndex < 0)
			{
				screenUIModel.CurrentPauseMenuSelectedIndex = screenUIView.PauseMenuOptions.Length - 1;
			}
			ChangeColorPauseMenuImage();
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown)
		{
			screenUIModel.CurrentPauseMenuSelectedIndex++;
			if (screenUIView.PauseMenuOptions.Length <= screenUIModel.CurrentPauseMenuSelectedIndex)
			{
				screenUIModel.CurrentPauseMenuSelectedIndex = 0;
			}
			ChangeColorPauseMenuImage();
		}

		//Enterキーで選択を確定
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
		{
			ExecutePauseMenuAction();
		}
	}

	/// <summary>
	/// ポーズメニュー
	/// </summary> 
	void ChangeColorPauseMenuImage()
	{
		//メニューの見た目を更新
		for (int i = 0; i < screenUIView.PauseMenuOptions.Length; i++)
		{
			if (i == screenUIModel.CurrentPauseMenuSelectedIndex)
			{
				//選択中の項目の色を変更
				screenUIView.PauseMenuOptions[i].color = new Color(screenUIView.PauseMenuOptions[i].color.r, screenUIView.PauseMenuOptions[i].color.g, screenUIView.PauseMenuOptions[i].color.b, 0.5f);
			}
			else
			{
				screenUIView.PauseMenuOptions[i].color = new Color(screenUIView.PauseMenuOptions[i].color.r, screenUIView.PauseMenuOptions[i].color.g, screenUIView.PauseMenuOptions[i].color.b, 0.0f);
			}
		}
	}

	/// <summary>
	/// ポーズメニューアクションの実行
	/// </summary>
	void ExecutePauseMenuAction()
	{
		switch (screenUIModel.CurrentPauseMenuSelectedIndex)
		{
			case 0:
				ReturnToGamePlay();
				break;
			case 1:
				ExitGamePlay();
				break;
		}
	}

	/// <summary>
	/// ゲームプレイ画面に戻る
	/// </summary>
	void ReturnToGamePlay()
	{
		InGameManager.SingletonInstance.IsPause = false;
		HidePauseMenu();
	}

	/// <summary>
	/// ゲーム終了
	/// </summary>
	void ExitGamePlay()
	{
		Quit();
	}

	/// <summary>
	/// ゲーム終了
	/// </summary>
	void Quit()
	{
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
	}

	/// <summary>
	/// 指定秒数だけフェードインしてその後フェードアウトする（DOTween使用）
	/// </summary>
	/// <param name="staySeconds">表示維持時間（秒）</param>
	/// <param name="fadeDuration">フェードイン/アウト時間（秒）</param>
	public void ShowSaveNowText(float staySeconds = 0.5f, float fadeDuration = 0.5f)
	{
		if (screenUIView.SaveNowText == null)
		{
			return;
		}

		// 既存のTweenを停止
		screenUIView.SaveNowText.DOKill();

		// 強制的に透明から開始
		screenUIView.SaveNowText.color = new Color(screenUIView.SaveNowText.color.r, screenUIView.SaveNowText.color.g, screenUIView.SaveNowText.color.b, 0f);

		// シーケンス: フェードイン -> 指定秒数待機 -> フェードアウト
		Sequence seq = DOTween.Sequence();
		seq.Append(screenUIView.SaveNowText.DOFade(1f, fadeDuration));
		seq.AppendInterval(staySeconds);
		seq.Append(screenUIView.SaveNowText.DOFade(0f, fadeDuration));
		seq.Play();
	}

	/// <summary>
	/// コンピューターメニューのミッションリストの初期化
	/// </summary>
	/// <param name="missionList">ミッションリスト</param>
	public void InitComputerMenuMissionList(List<MasterMissionEntity> missionList)
	{
		screenUIModel.MissionList.Clear();
		screenUIModel.MissionList.AddRange(missionList);
	}

	/// <summary>
	/// メールリストの全ての子オブジェクトコンテンツを破棄
	/// </summary>
	public void DestroyAllMailListContent()
	{
		foreach (Transform mailContent in screenUIView.MailListContent.transform)
		{
			UnityEngine.Object.Destroy(mailContent.gameObject);
		}
		screenUIModel.MailListContentList.Clear();
	}

	/// <summary>
	/// メールリストに子オブジェクトコンテンツを追加
	/// </summary>
	public void AddMailListContent()
	{
		for (int i = 0; i < screenUIModel.MissionList.Count; i++)
		{
			//プレハブをInstantiateしてContentの子オブジェクトに配置
			Mail mailGameObject = UnityEngine.Object.Instantiate(screenUIView.MailPrefab, new Vector3(0, 0, 0), Quaternion.identity, screenUIView.MailListContent.transform).GetComponent<Mail>();
			mailGameObject.Initialize(screenUIModel.MissionList[i].MissionID, screenUIModel.MissionList[i].MissionName);
			screenUIModel.MailListContentList.Add(mailGameObject);
		}
	}

	/// <summary>
	/// コンピューターメニューの表示
	/// </summary>
	public void ShowComputerMenu()
	{
		screenUIModel.CurrentMailListSelectedIndex = 0;
		ChangeColorMailListContentImage();

		if (screenUIModel.MissionList.Count != 0)
		{
			//メールメッセージ
			screenUIView.MailTitleMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailTitleMessage;
			screenUIView.MailMainMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailMainMessage;
		}
		else
		{
			screenUIView.MailTitleMessage.text = "-----";
			screenUIView.MailMainMessage.text = "-----";
		}

		screenUIModel.IsComputerMenuActive = true;
		screenUIView.PanelComputerMenu.gameObject.SetActive(true);
	}

	/// <summary>
	/// コンピューターメニューの非表示
	/// </summary>
	public void HideComputerMenu()
	{
		HideYesNoDialog();
		screenUIModel.IsComputerMenuActive = false;
		screenUIView.PanelComputerMenu.gameObject.SetActive(false);
	}

	/// <summary>
	/// コンピューターメニュー画面の更新
	/// </summary> 
	public void UpdateComputerMenuSystem()
	{
		if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("XInput B"))
		{
			//コンピューターメニューを閉じる
			if (screenUIModel.IsComputerMenuActive == true)
			{
				HideComputerMenu();
				return;
			}
		}

		if (screenUIModel.MissionList.Count == 0)
		{
			return;
		}

		if (screenUIModel.IsComputerMenuActive == false)
		{
			return;
		}

		if (screenUIView.YesNoDialogImage.gameObject.activeSelf == true)
		{
			return;
		}

		//上下の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || XInputManager.SingletonInstance.XInputDPadHandler.UpDown)
		{
			screenUIModel.CurrentMailListSelectedIndex--;
			if (screenUIModel.CurrentMailListSelectedIndex < 0)
			{
				screenUIModel.CurrentMailListSelectedIndex = screenUIModel.MissionList.Count - 1;
			}
			ChangeColorMailListContentImage();
			screenUIView.MailTitleMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailTitleMessage;
			screenUIView.MailMainMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailMainMessage;
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || XInputManager.SingletonInstance.XInputDPadHandler.DownDown)
		{
			screenUIModel.CurrentMailListSelectedIndex++;
			if (screenUIModel.MissionList.Count <= screenUIModel.CurrentMailListSelectedIndex)
			{
				screenUIModel.CurrentMailListSelectedIndex = 0;
			}
			ChangeColorMailListContentImage();
			screenUIView.MailTitleMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailTitleMessage;
			screenUIView.MailMainMessage.text = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex].MailMainMessage;
		}

		//Enterキーで選択を確定
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
		{
			ShowYesNoDialog();
		}
	}

	/// <summary>
	/// YesNoダイアログを表示する
	/// </summary>
	void ShowYesNoDialog()
	{
		screenUIModel.CurrentYesNoDialogSelectedIndex = 0;
		screenUIView.YesNoDialogImage.gameObject.SetActive(true);
		// ダイアログ表示直後のフレームでの決定入力を無視する
		screenUIModel.SkipYesNoDialogInput = true;
	}

	/// <summary>
	/// YesNoダイアログを非表示にする
	/// </summary>
	void HideYesNoDialog()
	{
		screenUIModel.CurrentYesNoDialogSelectedIndex = 0;
		screenUIView.YesNoDialogImage.gameObject.SetActive(false);
	}

	/// <summary>
	/// YesNoダイアログの更新
	/// </summary>
	public void UpdateYesNoDialog()
	{
		if (screenUIView.YesNoDialogImage.gameObject.activeSelf == false)
		{
			return;
		}

		//左右の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown)
		{
			screenUIModel.CurrentYesNoDialogSelectedIndex--;
			if (screenUIModel.CurrentYesNoDialogSelectedIndex < 0)
			{
				screenUIModel.CurrentYesNoDialogSelectedIndex = screenUIView.YesNoButtonImages.Length - 1;
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown)
		{
			screenUIModel.CurrentYesNoDialogSelectedIndex++;
			if (screenUIView.YesNoButtonImages.Length <= screenUIModel.CurrentYesNoDialogSelectedIndex)
			{
				screenUIModel.CurrentYesNoDialogSelectedIndex = 0;
			}
		}
		ChangeYesNoDialogButton();

		//Enterキーで選択を確定
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
		{
			ExecuteYesNoDialogAction();
		}
	}

	/// <summary>
	/// YesNoDialogのボタンの色を変える
	/// </summary> 
	void ChangeYesNoDialogButton()
	{
		//メニューの見た目を更新
		for (int i = 0; i < screenUIView.YesNoButtonImages.Length; i++)
		{
			if (i == screenUIModel.CurrentYesNoDialogSelectedIndex)
			{
				//選択中の項目の色を変更
				screenUIView.YesNoButtonImages[i].color = new Color(screenUIView.YesNoButtonImages[i].color.r, screenUIView.YesNoButtonImages[i].color.g, screenUIView.YesNoButtonImages[i].color.b, 0.5f);
			}
			else
			{
				screenUIView.YesNoButtonImages[i].color = new Color(screenUIView.YesNoButtonImages[i].color.r, screenUIView.YesNoButtonImages[i].color.g, screenUIView.YesNoButtonImages[i].color.b, 0.0f);
			}
		}
	}

	/// <summary>
	/// YesNoダイアログの決定
	/// </summary>
	void ExecuteYesNoDialogAction()
	{
		switch (screenUIModel.CurrentYesNoDialogSelectedIndex)
		{
			case 0:
				//No
				HideYesNoDialog();
				break;
			case 1:
				//Yes
				ExecuteComputerMenuAction();
				HideYesNoDialog();
				break;
		}
	}

	/// <summary>
	/// メールリストの色変更
	/// </summary> 
	void ChangeColorMailListContentImage()
	{
		//メニューの見た目を更新
		for (int i = 0; i < screenUIModel.MissionList.Count; i++)
		{
			if (i == screenUIModel.CurrentMailListSelectedIndex)
			{
				//選択中の項目の色を変更
				screenUIModel.MailListContentList[i].ImageMailBG.color = new Color32(0, 100, 255, 120);
			}
			else
			{
				//選択中でない項目の色を変更
				screenUIModel.MailListContentList[i].ImageMailBG.color = new Color32(100, 100, 100, 120);
			}
		}
	}

	/// <summary>
	/// コンピューターメニューアクションの実行
	/// </summary>
	void ExecuteComputerMenuAction()
	{
		ExecuteMission();
		HideComputerMenu();
	}

	/// <summary>
	/// ミッション実行
	/// </summary>
	void ExecuteMission()
	{
		//↓ここをミッションごとに変えるようにする必要がある
		//各ComputerTYPEに紐づいたミッションリストを取得して、そこから選択されたミッション内容を↓に反映すればいい
		MasterMissionEntity result = screenUIModel.MissionList[screenUIModel.CurrentMailListSelectedIndex];
		//ミッション開始
		if (result != null)
		{
			Debug.Log("<color=red>ミッション開始</color>");
			MissionManager.SingletonInstance.IsMissionActive = true;
			MissionManager.SingletonInstance.CurrentMissionID = result.MissionID;
			MissionManager.SingletonInstance.EndComputerStageNumber = result.EndComputerStageNumber;
			TimerManager.SingletonInstance.Minute = result.Minute;
			TimerManager.SingletonInstance.Seconds = result.Seconds;
			screenUIView.MapUI.SetEndComputerStageNumber((int)result.EndComputerStageNumber);
		}
	}

	/// <summary>
	/// リザルト画面を表示する
	/// </summary>
	public void ShowResult(float staySeconds = 1f, float fadeDuration = 0.5f)
	{
		screenUIModel.IsComputerMenuActive = true;

		// 既存のTweenを停止
		screenUIView.ResultImage.DOKill();

		// 有効化して透明から開始
		screenUIView.ResultImage.gameObject.SetActive(true);
		screenUIView.ResultImage.color = new Color(screenUIView.ResultImage.color.r, screenUIView.ResultImage.color.g, screenUIView.ResultImage.color.b, 0f);

		// シーケンス: フェードイン -> 指定秒数待機 -> フェードアウト -> 非表示 + コンピュータメニュー表示
		Sequence seq = DOTween.Sequence();
		seq.Append(screenUIView.ResultImage.DOFade(0.2f, fadeDuration));
		seq.AppendInterval(staySeconds);
		seq.Append(screenUIView.ResultImage.DOFade(0f, fadeDuration));
		seq.OnComplete(() =>
		{
			screenUIView.ResultImage.gameObject.SetActive(false);
			ShowComputerMenu();
		});
		seq.Play();
	}
}

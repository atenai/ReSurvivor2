using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using DG.Tweening;

/// <summary>
/// スクリーンUI管理クラス
/// </summary>
public class ScreenUI : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static ScreenUI singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static ScreenUI SingletonInstance => singletonInstance;

	[Header("照準")]

	[Tooltip("クロスヘアー")]
	[SerializeField] Image imageCrosshair;

	[Tooltip("ヒットレティクル")]
	[SerializeField] Image imageHitReticule;

	[Tooltip("ヒットレティクルが表示か非表示か")]
	bool isHitReticule = false;
	public bool IsHitReticule
	{
		get { return isHitReticule; }
		set { isHitReticule = value; }
	}
	/// <summary> ヒットレティクルが消失するスピード</summary>
	float hitReticuleSpeed = 10.0f;

	[Header("ロード")]
	[Tooltip("ロード画面")]
	[SerializeField] GameObject panelLoading;
	public GameObject PanelLoading => panelLoading;

	[Tooltip("ロードスライダー")]
	[SerializeField] Slider sliderLoading;
	public Slider SliderLoading
	{
		get { return sliderLoading; }
		set { sliderLoading = value; }
	}

	[Header("画面エフェクト")]
	[Tooltip("ダメージ画像エフェクト")]
	[SerializeField] Image imageDamage;

	[Tooltip("HP回復画像エフェクト")]
	[SerializeField] Image imageHpHeal;

	[Tooltip("スタミナ回復画像エフェクト")]
	[SerializeField] Image imageStaminaHeal;

	[Header("フェード")]
	[Tooltip("フェード用画像")]
	[SerializeField] Image imageFade;
	[Tooltip("フェードの速さ")]
	float fadeSpeed = 1.0f;

	[Header("ポーズUI")]
	[Tooltip("ポーズメニューパネル")]
	[SerializeField] GameObject panelPauseMenu;
	[Tooltip("ポーズメニューオプションのイメージリスト")]
	[SerializeField] Image[] pauseMenuOptions;
	/// <summary> 現在のポーズメニュー選択インデックス</summary>
	int currentPauseMenuSelectedIndex = 0;
	/// <summary> ポーズ中か？</summary>
	bool isPause = false;
	/// <summary> ポーズ中か？のプロパティ </summary>
	public bool IsPause => isPause;

	[Header("マップUI")]
	[Tooltip("マップイメージ")]
	[SerializeField] MapUI mapUI;
	public MapUI MapUI => mapUI;

	[Header("アイテム取得")]
	[Tooltip("アイテム取得ログ")]
	[SerializeField] ItemOutPutLog itemOutPutLog;
	public ItemOutPutLog ItemOutPutLog => itemOutPutLog;

	[Header("セーブ")]
	[Tooltip("セーブテキスト")]
	[SerializeField] TextMeshProUGUI saveNowText;

	[Header("コンピューターUI")]
	[Tooltip("コンピューターメニューパネル")]
	[SerializeField] GameObject panelComputerMenu;
	[Tooltip("メールリストコンテンツ")]
	[SerializeField] GameObject mailListContent;
	[Tooltip("メールプレハブ")]
	[SerializeField] GameObject mailPrefab;
	[Tooltip("メールメッセージタイトル")]
	[SerializeField] Text mailTitleMessage;
	[Tooltip("メールメッセージ本文")]
	[SerializeField] Text mailMainMessage;
	/// <summary> ミッションリスト </summary>
	List<MasterMissionEntity> missionList = new List<MasterMissionEntity>();
	/// <summary> メールリストのコンテンツリスト </summary>
	List<Mail> mailListContentList = new List<Mail>();
	/// <summary> 現在のメールリストの選択インデックス </summary>
	int currentMailListSelectedIndex = 0;
	/// <summary> コンピューターメニュー表示中か？ </summary>
	bool isComputerMenuActive = false;
	/// <summary> コンピューターメニュー表示中か？のプロパティ </summary>
	public bool IsComputerMenuActive => isComputerMenuActive;

	[Tooltip("Yes/Noダイアログ")]
	[SerializeField] Image yesnoDialogImage;
	[Tooltip("Yes/Noボタンのイメージリスト")]
	[SerializeField] Image[] yesnoButtonImages;
	/// <summary>  Yes/Noダイアログ表示直後の入力を1フレームだけ無視するフラグ</summary>
	bool skipYesNoDialogInput = false;
	/// <summary> 現在選択されているYes/Noダイアログのインデックス</summary>
	int currentYesNoDialogSelectedIndex = 0;

	[Tooltip("リザルト画面")]
	[SerializeField] Image resultImage;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、UIのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		imageHitReticule.color = Color.clear;

		//ダメージ画像エフェクト
		imageDamage.color = Color.clear;

		//HP回復画像エフェクト
		imageHpHeal.color = Color.clear;

		//スタミナ回復画像エフェクト
		imageStaminaHeal.color = Color.clear;

		//セーブ表示用テキストを透明にする
		saveNowText.color = new Color(saveNowText.color.r, saveNowText.color.g, saveNowText.color.b, 0f);

		panelComputerMenu.gameObject.SetActive(false);

		// リザルト画像は初期で非表示・透明にしておく
		resultImage.color = new Color(resultImage.color.r, resultImage.color.g, resultImage.color.b, 0f);
		resultImage.gameObject.SetActive(false);

		HideYesNoDialog();

		FadeOut();
	}

	void Update()
	{
		//ゲームクリアーシーンとゲームオーバーシーンに切り替えたら切り上げる
		if (InGameManager.SingletonInstance.IsGameClearAndGameOverSceneSwitched == true)
		{
			return;
		}

		//ゲームクリアーとゲームオーバーをトリガーのどちらかが起動したら切り上げる
		if (InGameManager.SingletonInstance.IsGameClearTriggered == true || InGameManager.SingletonInstance.IsGameOverTriggered == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		Crosshair();
		UpdateHitReticule();
		UpdateDamageEffect();
		UpdateHpHealEffect();
		UpdateStaminaHealEffect();

		if (Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("XInput Pause"))
		{
			mapUI.EnableMap();
		}

		UpdateComputerMenuSystem();
		if (skipYesNoDialogInput == true)
		{
			// 表示直後のフレームは入力を消費して無視する
			skipYesNoDialogInput = false;
		}
		else
		{
			UpdateYesNoDialog();
		}

		UpdatePauseMenuSystem();
	}

	/// <summary>
	/// クロスヘア（レティクル）の表示/非表示かつ色の切り替え
	/// </summary>
	void Crosshair()
	{
		if (Player.SingletonInstance.IsAim == false)
		{
			imageCrosshair.gameObject.SetActive(false);
		}
		else
		{
			imageCrosshair.gameObject.SetActive(true);

			if (PlayerCamera.SingletonInstance.IsTargethit == true)
			{
				imageCrosshair.color = new Color32(255, 0, 0, 150);
			}
			else
			{
				imageCrosshair.color = new Color32(255, 255, 255, 150);
			}
		}
	}

	/// <summary>
	/// ヒットレティクル
	/// </summary> 
	void UpdateHitReticule()
	{
		if (isHitReticule == true)
		{
			imageHitReticule.color = new Color32(255, 0, 0, 150);
		}

		if (isHitReticule == false)
		{
			imageHitReticule.color = Color.Lerp(imageHitReticule.color, Color.clear, Time.deltaTime * hitReticuleSpeed);
		}

		isHitReticule = false;
	}

	/// <summary>
	/// ダメージ画像エフェクト
	/// </summary> 
	void UpdateDamageEffect()
	{
		if (Player.SingletonInstance.IsDamage == true)
		{
			imageDamage.color = new Color(0.5f, 0f, 0f, 0.5f);
		}

		if (Player.SingletonInstance.IsDamage == false)
		{
			imageDamage.color = Color.Lerp(imageDamage.color, Color.clear, Time.deltaTime);
		}

		Player.SingletonInstance.IsDamage = false;
	}

	/// <summary>
	/// HP回復画像エフェクト
	/// </summary> 
	void UpdateHpHealEffect()
	{
		if (Player.SingletonInstance.IsHpHeal == true)
		{
			imageHpHeal.color = new Color(0f, 0.5f, 0f, 0.5f);
		}

		if (Player.SingletonInstance.IsHpHeal == false)
		{
			imageHpHeal.color = Color.Lerp(imageHpHeal.color, Color.clear, Time.deltaTime);
		}

		Player.SingletonInstance.IsHpHeal = false;
	}

	/// <summary>
	/// スタミナ回復画像エフェクト
	/// </summary> 
	void UpdateStaminaHealEffect()
	{
		if (Player.SingletonInstance.IsStaminaHeal == true)
		{
			imageStaminaHeal.color = new Color(0.5f, 0.5f, 0f, 0.5f);
		}

		if (Player.SingletonInstance.IsStaminaHeal == false)
		{
			imageStaminaHeal.color = Color.Lerp(imageStaminaHeal.color, Color.clear, Time.deltaTime);
		}

		Player.SingletonInstance.IsStaminaHeal = false;
	}

	/// <summary>
	/// フェードアウト処理（画面を暗くする）
	/// </summary>
	public void FadeOut()
	{
		InGameManager.SingletonInstance.IsGamePlayReady = false;
		imageFade.color = new Color(imageFade.color.r, imageFade.color.g, imageFade.color.b, 1);
	}

	/// <summary>
	/// フェードイン処理（画面を透明にする）
	/// </summary>
	public IEnumerator FadeIn()
	{
		//現在のアルファ値を取得
		Color color = imageFade.color;
		float alpha = color.a;

		//アルファ値が0になるまで徐々に減らす
		while (0 < alpha)
		{
			alpha -= Time.deltaTime * fadeSpeed;
			imageFade.color = new Color(color.r, color.g, color.b, alpha);
			yield return null;
		}

		//最後に完全に透明にする
		imageFade.color = new Color(color.r, color.g, color.b, 0);

		InGameManager.SingletonInstance.IsGamePlayReady = true;
	}

	/// <summary>
	/// ポーズメニュー画面の更新
	/// </summary> 
	void UpdatePauseMenuSystem()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("XInput Start"))
		{
			PauseMenuActive();
		}

		if (isPause == true)
		{
			//左右の矢印キーで選択を変更
			if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown)
			{
				currentPauseMenuSelectedIndex--;
				if (currentPauseMenuSelectedIndex < 0)
				{
					currentPauseMenuSelectedIndex = pauseMenuOptions.Length - 1;
				}
				ChangeColorPauseMenuImage();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown)
			{
				currentPauseMenuSelectedIndex++;
				if (pauseMenuOptions.Length <= currentPauseMenuSelectedIndex)
				{
					currentPauseMenuSelectedIndex = 0;
				}
				ChangeColorPauseMenuImage();
			}

			//Enterキーで選択を確定
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
			{
				ExecutePauseMenuAction();
			}
		}
	}

	/// <summary>
	/// ポーズメニューのオン/オフ
	/// </summary>
	void PauseMenuActive()
	{
		isPause = isPause ? false : true;

		if (isPause == true)
		{
			Time.timeScale = 0f;
			panelPauseMenu.SetActive(true);

			//各種パラメーターの初期化処理
			currentPauseMenuSelectedIndex = 0;
			ChangeColorPauseMenuImage();
		}
		else
		{
			Time.timeScale = 1f;
			panelPauseMenu.SetActive(false);
		}
	}

	/// <summary>
	/// ポーズメニュー
	/// </summary> 
	void ChangeColorPauseMenuImage()
	{
		//メニューの見た目を更新
		for (int i = 0; i < pauseMenuOptions.Length; i++)
		{
			if (i == currentPauseMenuSelectedIndex)
			{
				//選択中の項目の色を変更
				pauseMenuOptions[i].color = new Color(pauseMenuOptions[i].color.r, pauseMenuOptions[i].color.g, pauseMenuOptions[i].color.b, 0.5f);
			}
			else
			{
				pauseMenuOptions[i].color = new Color(pauseMenuOptions[i].color.r, pauseMenuOptions[i].color.g, pauseMenuOptions[i].color.b, 0.0f);
			}
		}
	}

	/// <summary>
	/// ポーズメニューアクションの実行
	/// </summary>
	void ExecutePauseMenuAction()
	{
		switch (currentPauseMenuSelectedIndex)
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
		PauseMenuActive();
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
		if (saveNowText == null)
		{
			return;
		}

		// 既存のTweenを停止
		saveNowText.DOKill();

		// 強制的に透明から開始
		saveNowText.color = new Color(saveNowText.color.r, saveNowText.color.g, saveNowText.color.b, 0f);

		// シーケンス: フェードイン -> 指定秒数待機 -> フェードアウト
		Sequence seq = DOTween.Sequence();
		seq.Append(saveNowText.DOFade(1f, fadeDuration));
		seq.AppendInterval(staySeconds);
		seq.Append(saveNowText.DOFade(0f, fadeDuration));
		seq.Play();
	}

	/// <summary>
	/// コンピューターメニューのミッションリストの初期化
	/// </summary>
	/// <param name="missionList">ミッションリスト</param>
	public void InitComputerMenuMissionList(List<MasterMissionEntity> missionList)
	{
		this.missionList.Clear();
		this.missionList = missionList;
	}

	/// <summary>
	/// メールリストの全ての子オブジェクトコンテンツを破棄
	/// </summary>
	public void DestroyAllMailListContent()
	{
		foreach (Transform mailContent in mailListContent.transform)
		{
			Destroy(mailContent.gameObject);
		}
		mailListContentList.Clear();
	}

	/// <summary>
	/// メールリストに子オブジェクトコンテンツを追加
	/// </summary>
	public void AddMailListContent()
	{
		for (int i = 0; i < this.missionList.Count; i++)
		{
			//プレハブをInstantiateしてContentの子オブジェクトに配置
			Mail mailGameObject = Instantiate(mailPrefab, new Vector3(0, 0, 0), Quaternion.identity, mailListContent.transform).GetComponent<Mail>();
			mailGameObject.Initialize(this.missionList[i].MissionID, this.missionList[i].MissionName);
			mailListContentList.Add(mailGameObject);
		}
	}

	/// <summary>
	/// コンピューターメニューの表示
	/// </summary>
	public void ShowComputerMenu()
	{
		currentMailListSelectedIndex = 0;
		ChangeColorMailListContentImage();

		if (this.missionList.Count != 0)
		{
			//メールメッセージ
			mailTitleMessage.text = this.missionList[currentMailListSelectedIndex].MailTitleMessage;
			mailMainMessage.text = this.missionList[currentMailListSelectedIndex].MailMainMessage;
		}
		else
		{
			mailTitleMessage.text = "-----";
			mailMainMessage.text = "-----";
		}

		isComputerMenuActive = true;
		panelComputerMenu.SetActive(true);
	}

	/// <summary>
	/// コンピューターメニューの非表示
	/// </summary>
	void HideComputerMenu()
	{
		HideYesNoDialog();
		isComputerMenuActive = false;
		panelComputerMenu.SetActive(false);
	}

	/// <summary>
	/// コンピューターメニュー画面の更新
	/// </summary> 
	void UpdateComputerMenuSystem()
	{
		if (isPause == true)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.B) || Input.GetButtonDown("XInput B"))
		{
			//コンピューターメニューを閉じる
			if (isComputerMenuActive == true)
			{
				HideComputerMenu();
				return;
			}
		}

		if (this.missionList.Count == 0)
		{
			return;
		}

		if (isComputerMenuActive == false)
		{
			return;
		}

		if (yesnoDialogImage.gameObject.activeSelf == true)
		{
			return;
		}

		//上下の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || XInputManager.SingletonInstance.XInputDPadHandler.UpDown)
		{
			currentMailListSelectedIndex--;
			if (currentMailListSelectedIndex < 0)
			{
				currentMailListSelectedIndex = this.missionList.Count - 1;
			}
			ChangeColorMailListContentImage();
			mailTitleMessage.text = this.missionList[currentMailListSelectedIndex].MailTitleMessage;
			mailMainMessage.text = this.missionList[currentMailListSelectedIndex].MailMainMessage;
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || XInputManager.SingletonInstance.XInputDPadHandler.DownDown)
		{
			currentMailListSelectedIndex++;
			if (this.missionList.Count <= currentMailListSelectedIndex)
			{
				currentMailListSelectedIndex = 0;
			}
			ChangeColorMailListContentImage();
			mailTitleMessage.text = this.missionList[currentMailListSelectedIndex].MailTitleMessage;
			mailMainMessage.text = this.missionList[currentMailListSelectedIndex].MailMainMessage;
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
		currentYesNoDialogSelectedIndex = 0;
		yesnoDialogImage.gameObject.SetActive(true);
		// ダイアログ表示直後のフレームでの決定入力を無視する
		skipYesNoDialogInput = true;
	}

	/// <summary>
	/// YesNoダイアログを非表示にする
	/// </summary>
	void HideYesNoDialog()
	{
		currentYesNoDialogSelectedIndex = 0;
		yesnoDialogImage.gameObject.SetActive(false);
	}

	/// <summary>
	/// YesNoダイアログの更新
	/// </summary>
	void UpdateYesNoDialog()
	{
		if (isPause == true)
		{
			return;
		}

		if (yesnoDialogImage.gameObject.activeSelf == false)
		{
			return;
		}

		//左右の矢印キーで選択を変更
		if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || XInputManager.SingletonInstance.XInputDPadHandler.LeftDown)
		{
			currentYesNoDialogSelectedIndex--;
			if (currentYesNoDialogSelectedIndex < 0)
			{
				currentYesNoDialogSelectedIndex = yesnoButtonImages.Length - 1;
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || XInputManager.SingletonInstance.XInputDPadHandler.RightDown)
		{
			currentYesNoDialogSelectedIndex++;
			if (yesnoButtonImages.Length <= currentYesNoDialogSelectedIndex)
			{
				currentYesNoDialogSelectedIndex = 0;
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
		for (int i = 0; i < yesnoButtonImages.Length; i++)
		{
			if (i == currentYesNoDialogSelectedIndex)
			{
				//選択中の項目の色を変更
				yesnoButtonImages[i].color = new Color(yesnoButtonImages[i].color.r, yesnoButtonImages[i].color.g, yesnoButtonImages[i].color.b, 0.5f);
			}
			else
			{
				yesnoButtonImages[i].color = new Color(yesnoButtonImages[i].color.r, yesnoButtonImages[i].color.g, yesnoButtonImages[i].color.b, 0.0f);
			}
		}
	}

	/// <summary>
	/// YesNoダイアログの決定
	/// </summary>
	void ExecuteYesNoDialogAction()
	{
		switch (currentYesNoDialogSelectedIndex)
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
		for (int i = 0; i < this.missionList.Count; i++)
		{
			if (i == currentMailListSelectedIndex)
			{
				//選択中の項目の色を変更
				mailListContentList[i].ImageMailBG.color = new Color32(0, 100, 255, 120);
			}
			else
			{
				//選択中でない項目の色を変更
				mailListContentList[i].ImageMailBG.color = new Color32(100, 100, 100, 120);
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
		MasterMissionEntity result = this.missionList[currentMailListSelectedIndex];
		//ミッション開始
		if (result != null)
		{
			Debug.Log("<color=red>ミッション開始</color>");
			InGameManager.SingletonInstance.IsMissionActive = true;
			InGameManager.SingletonInstance.CurrentMissionID = result.MissionID;
			InGameManager.SingletonInstance.EndComputerName = result.EndComputerName;
			TimerManager.SingletonInstance.Minute = result.Minute;
			TimerManager.SingletonInstance.Seconds = result.Seconds;
			MapUI.SetEndComputerStageNumber((int)result.EndComputerName);
		}
	}

	/// <summary>
	/// リザルト画面を表示する
	/// </summary>
	public void ShowResult(float staySeconds = 1f, float fadeDuration = 0.5f)
	{
		isComputerMenuActive = true;

		// 既存のTweenを停止
		resultImage.DOKill();

		// 有効化して透明から開始
		resultImage.gameObject.SetActive(true);
		resultImage.color = new Color(resultImage.color.r, resultImage.color.g, resultImage.color.b, 0f);

		// シーケンス: フェードイン -> 指定秒数待機 -> フェードアウト -> 非表示 + コンピュータメニュー表示
		Sequence seq = DOTween.Sequence();
		seq.Append(resultImage.DOFade(0.2f, fadeDuration));
		seq.AppendInterval(staySeconds);
		seq.Append(resultImage.DOFade(0f, fadeDuration));
		seq.OnComplete(() =>
		{
			resultImage.gameObject.SetActive(false);
			ShowComputerMenu();
		});
		seq.Play();
	}
}

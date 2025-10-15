using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Events;

public class UI : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static UI singletonInstance = null;
	public static UI SingletonInstance => singletonInstance;

	[Tooltip("クロスヘアー")]
	[SerializeField] Image imageCrosshair;
	[Tooltip("ヒットレティクル")]
	[SerializeField] Image imageHitReticule;
	public Image ImageHitReticule
	{
		get { return imageHitReticule; }
		set { imageHitReticule = value; }
	}

	[SerializeField] GameObject panelLoading;
	public GameObject PanelLoading
	{
		get { return panelLoading; }
		set { panelLoading = value; }
	}
	[SerializeField] Slider sliderLoading;
	public Slider SliderLoading
	{
		get { return sliderLoading; }
		set { sliderLoading = value; }
	}

	[Tooltip("ダメージ画像エフェクト")]
	[SerializeField] Image imageDamage;
	public Image ImageDamage
	{
		get { return imageDamage; }
		set { imageDamage = value; }
	}

	[Tooltip("HP回復画像エフェクト")]
	[SerializeField] Image imageHpHeal;
	public Image ImageHpHeal
	{
		get { return imageHpHeal; }
		set { imageHpHeal = value; }
	}

	[Tooltip("スタミナ回復画像エフェクト")]
	[SerializeField] Image imageStaminaHeal;
	public Image ImageStaminaHeal
	{
		get { return imageStaminaHeal; }
		set { imageStaminaHeal = value; }
	}

	[Tooltip("フェード用のUI Image")]
	[SerializeField] Image imageFade;
	[Tooltip("フェードの速さ")]
	float fadeSpeed = 1.0f;

	[Header("ポーズメニュー")]
	[Tooltip("ポーズメニューパネル")]
	[SerializeField] GameObject panelPauseMenu;
	[Tooltip("ポーズメニューオプションのイメージリスト")]
	[SerializeField] Image[] pauseMenuOptions;
	/// <summary>
	/// 現在のポーズメニュー選択インデックス
	/// </summary>
	int currentPauseMenuSelectedIndex = 0;
	/// <summary>
	/// ポーズ中か？
	/// </summary>
	bool isPause = false;
	/// <summary>
	/// ポーズ中か？のプロパティ
	/// </summary>
	public bool IsPause => isPause;

	/// <summary>
	/// XInputのDPad管理クラス
	/// </summary>
	XInputDPadHandler xInputDPadHandler = new XInputDPadHandler();

	[Header("コンピューターメニュー")]
	[Tooltip("コンピューターメニューパネル")]
	[SerializeField] GameObject panelComputerMenu;
	[Tooltip("コンピューターメニューコンテンツ")]
	[SerializeField] GameObject computerMenuContent;
	[Tooltip("コンピューターメニュープレハブ")]
	[SerializeField] GameObject computerMenuPrefab;
	/// <summary>
	/// コンピューターメニューのコンテンツリスト
	/// </summary>
	List<ComputerMenu> computerMenuContentList = new List<ComputerMenu>();
	/// <summary>
	/// コンピューターメニューのミッションリスト
	/// </summary>
	List<MasterMissionEntity> missionList = new List<MasterMissionEntity>();
	/// <summary>
	/// 現在のコンピューターメニュー選択インデックス
	/// </summary>
	int currentComputerMenuSelectedIndex = 0;
	/// <summary>
	/// コンピューターメニュー表示中か？
	/// </summary>
	bool isComputerMenuActive = false;
	/// <summary>
	/// コンピューターメニュー表示中か？のプロパティ
	/// </summary>
	public bool IsComputerMenuActive => isComputerMenuActive;

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
		//ダメージ画像エフェクト
		imageDamage.color = Color.clear;

		//HP回復画像エフェクト
		imageHpHeal.color = Color.clear;

		//スタミナ回復画像エフェクト
		imageStaminaHeal.color = Color.clear;

		InitFadeColor();

		imageHitReticule.color = Color.clear;
	}

	void Update()
	{
		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		Crosshair();
		UpdatePauseMenuSystem();
		UpdateComputerMenuSystem();

		UpdateXInputDPad();
	}

	/// <summary>
	/// クロスヘア（レティクル）のオン/オフかつ色の切り替え
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
	/// フェードの色を初期化
	/// </summary>
	public void InitFadeColor()
	{
		InGameManager.SingletonInstance.IsGamePlayReady = false;
		imageFade.color = new Color(imageFade.color.r, imageFade.color.g, imageFade.color.b, 1);
	}

	/// <summary>
	/// フェードアウト処理（不透明にする）
	/// </summary>
	public IEnumerator FadeOut()
	{
		InGameManager.SingletonInstance.IsGamePlayReady = false;

		//現在のアルファ値を取得
		Color color = imageFade.color;
		float alpha = color.a;

		//アルファ値が1になるまで徐々に増やす
		while (alpha < 1)
		{
			alpha += Time.deltaTime * fadeSpeed;
			imageFade.color = new Color(color.r, color.g, color.b, alpha);
			yield return null;
		}

		//最後に完全に不透明にする
		imageFade.color = new Color(color.r, color.g, color.b, 1);
	}

	/// <summary>
	/// フェードイン処理（透明にする）
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
			if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || xInputDPadHandler.LeftDown)
			{
				currentPauseMenuSelectedIndex--;
				if (currentPauseMenuSelectedIndex < 0)
				{
					currentPauseMenuSelectedIndex = pauseMenuOptions.Length - 1;
				}
				ChangeColorPauseMenuImage();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || xInputDPadHandler.RightDown)
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
	/// XInputのDPadの状態更新
	/// </summary>
	void UpdateXInputDPad()
	{
		// DPad軸を取得（InputManagerで設定済み or 軸番号で直接）
		float dpadX = Input.GetAxis("XInput DPad Left&Right");
		float dpadY = Input.GetAxis("XInput DPad Up&Down");

		// 状態更新
		xInputDPadHandler.Update(dpadX, dpadY);
	}

	/// <summary>
	/// コンピューターメニューの表示
	/// </summary>
	public void ShowComputerMenu(InGameManager.ComputerTYPE computerTYPE)
	{
		isComputerMenuActive = true;
		Time.timeScale = 0f;

		//各種パラメーターの初期化処理
		missionList.Clear();
		missionList = InGameManager.SingletonInstance.MissionSerchList(computerTYPE);
		if (missionList.Count == 0)
		{
			Debug.Log("<color=red>ミッションがありません</color>");
			HideComputerMenu();
			return;
		}

		//全ての子オブジェクトを破棄
		foreach (Transform computerMenuTransform in computerMenuContent.transform)
		{
			Destroy(computerMenuTransform.gameObject);
		}

		computerMenuContentList.Clear();
		for (int i = 0; i < missionList.Count; i++)
		{
			// プレハブをInstantiateしてContentの子オブジェクトに配置
			GameObject computerMenuGameObject = Instantiate(computerMenuPrefab, new Vector3(0, 0, 0), Quaternion.identity, computerMenuContent.transform);
			computerMenuGameObject.GetComponent<ComputerMenu>().Initialize(missionList[i].MissionName);
			computerMenuContentList.Add(computerMenuGameObject.GetComponent<ComputerMenu>());
		}

		currentComputerMenuSelectedIndex = 0;
		ChangeColorComputerMenuContentImage();

		panelComputerMenu.SetActive(true);
	}

	/// <summary>
	/// コンピューターメニューの非表示
	/// </summary>
	public void HideComputerMenu()
	{
		isComputerMenuActive = false;
		Time.timeScale = 1f;
		panelComputerMenu.SetActive(false);
	}

	/// <summary>
	/// コンピューターメニュー画面の更新
	/// </summary> 
	void UpdateComputerMenuSystem()
	{
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("XInput Start") || Input.GetButtonDown("XInput B"))
		{
			//コンピューターメニューを閉じる
			if (isComputerMenuActive == true)
			{
				HideComputerMenu();
				return;
			}
		}

		if (isComputerMenuActive == true)
		{
			//上下の矢印キーで選択を変更
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || xInputDPadHandler.UpDown)
			{
				currentComputerMenuSelectedIndex--;
				if (currentComputerMenuSelectedIndex < 0)
				{
					currentComputerMenuSelectedIndex = missionList.Count - 1;
				}
				ChangeColorComputerMenuContentImage();
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || xInputDPadHandler.DownDown)
			{
				currentComputerMenuSelectedIndex++;
				if (missionList.Count <= currentComputerMenuSelectedIndex)
				{
					currentComputerMenuSelectedIndex = 0;
				}
				ChangeColorComputerMenuContentImage();
			}

			//Enterキーで選択を確定
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XInput A"))
			{
				ExecuteComputerMenuAction();
			}
		}
	}

	/// <summary>
	/// コンピューターメニュー
	/// </summary> 
	void ChangeColorComputerMenuContentImage()
	{
		//メニューの見た目を更新
		for (int i = 0; i < missionList.Count; i++)
		{
			if (i == currentComputerMenuSelectedIndex)
			{
				//選択中の項目の色を変更
				computerMenuContentList[i].Image.color = new Color(computerMenuContentList[i].Image.color.r, computerMenuContentList[i].Image.color.g, computerMenuContentList[i].Image.color.b, 120.0f / 255.0f);
			}
			else
			{
				computerMenuContentList[i].Image.color = new Color(computerMenuContentList[i].Image.color.r, computerMenuContentList[i].Image.color.g, computerMenuContentList[i].Image.color.b, 0.0f);
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
		var result = missionList[currentComputerMenuSelectedIndex];
		//ミッション開始
		if (result != null)
		{
			Debug.Log("<color=red>ミッション開始</color>");
			InGameManager.SingletonInstance.IsMissionActive = true;
			InGameManager.SingletonInstance.TargetComputerName = result.EndComputerName;
			Player.SingletonInstance.Minute = result.Minute;
			Player.SingletonInstance.Seconds = result.Seconds;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	ScreenUIModel screenUIModel = new ScreenUIModel();
	public ScreenUIModel ScreenUIModel => screenUIModel;

	/// <summary>コンピューターメニューのプレゼンター（コンピューターメニューの処理は全部ここ）</summary>
	ComputerMenuPresenter computerMenuPresenter;
	public ComputerMenuPresenter ComputerMenuPresenter => computerMenuPresenter;

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

		//コンピューターメニューを作る（使うUIは、ScreenUIViewにインスペクターで設定してあるものを渡す）
		//※CurrentStageManagerのStart()でミッションを渡されるので、それより先のAwake()で作っておく
		computerMenuPresenter = new ComputerMenuPresenter(
			new MailListView(screenUIView.PanelComputerMenu, screenUIView.MailListContent, screenUIView.MailPrefab, screenUIView.MailTitleMessage, screenUIView.MailMainMessage),
			new YesNoDialogView(screenUIView.YesNoDialogImage, screenUIView.YesNoButtonImages),
			new MissionResultView(screenUIView.ResultImage));
	}

	void Start()
	{
		computerMenuPresenter.Close();
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

	public void AfterUpdate()
	{
		screenUIView.Crosshair(PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim, PlayerCameraManager.SingletonInstance.IsTargetHit);
		UpdateHitReticule();
		UpdateDamageEffect();
		UpdateHpHealEffect();
		UpdateStaminaHealEffect();
		UpdateWeaponQuickSelect();

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
	/// 武器クイックセレクトUI（十字キーの武器アイコンと弾数）を一時的に表示する
	/// 押している間ずっと呼ばれても、表示時間をリセットするだけなので表示が途切れない
	/// </summary>
	public void ShowWeaponQuickSelect()
	{
		screenUIModel.WeaponQuickSelectStayTimer = screenUIModel.WeaponQuickSelectStayTime;
	}

	/// <summary>
	/// 武器クイックセレクトUIの表示処理
	/// </summary>
	void UpdateWeaponQuickSelect()
	{
		//表示中のアルファ値
		const float Show_Alpha = 1.0f;
		//非表示のアルファ値
		const float Hide_Alpha = 0.0f;

		if (0.0f < screenUIModel.WeaponQuickSelectStayTimer)
		{
			//表示時間中はフェードインして表示し続ける
			screenUIModel.WeaponQuickSelectStayTimer = screenUIModel.WeaponQuickSelectStayTimer - Time.deltaTime;
			screenUIView.WeaponQuickSelect.alpha = Mathf.MoveTowards(screenUIView.WeaponQuickSelect.alpha, Show_Alpha, screenUIModel.WeaponQuickSelectFadeInSpeed * Time.deltaTime);
		}
		else
		{
			//表示時間が終わったらフェードアウトする
			screenUIView.WeaponQuickSelect.alpha = Mathf.MoveTowards(screenUIView.WeaponQuickSelect.alpha, Hide_Alpha, screenUIModel.WeaponQuickSelectFadeOutSpeed * Time.deltaTime);
		}

		//見えていない時は中身を更新しない
		if (screenUIView.WeaponQuickSelect.alpha <= Hide_Alpha)
		{
			return;
		}

		//銃のスロット：今持っている銃を選択中にして、弾数を表示する
		UpdateWeaponQuickSelectGunSlot(screenUIView.HandGunSlot, PlayerCameraManager.SingletonInstance.GunModelFacade, EnumManager.GunTYPE.HandGun);
		UpdateWeaponQuickSelectGunSlot(screenUIView.AssaultRifleSlot, PlayerCameraManager.SingletonInstance.GunModelFacade, EnumManager.GunTYPE.AssaultRifle);
		UpdateWeaponQuickSelectGunSlot(screenUIView.ShotGunSlot, PlayerCameraManager.SingletonInstance.GunModelFacade, EnumManager.GunTYPE.ShotGun);

		//地雷のスロット：選択する物ではないので、所持数だけを表示する
		screenUIView.MineSlot.SetSelected(false);
		screenUIView.MineSlot.SetCountText(PlayerManagerPresenter.SingletonInstance.PlayerModel.CurrentMine);
	}

	/// <summary>
	/// 武器クイックセレクトUIの銃のスロット1つ分を更新する
	/// </summary>
	/// <param name="slot">更新するスロット</param>
	/// <param name="gunModelFacade">ガンモデルファサード</param>
	/// <param name="gunTYPE">このスロットの銃のタイプ</param>
	void UpdateWeaponQuickSelectGunSlot(WeaponQuickSelectSlot slot, GunModelFacade gunModelFacade, EnumManager.GunTYPE gunTYPE)
	{
		GunModelBase gunModelBase = gunModelFacade.FindGunBase(gunTYPE);
		//※武器を切り替えている間は、切り替える先の銃を選択中にする
		slot.SetSelected(gunModelFacade.SelectedGunTYPE == gunTYPE);
		slot.SetAmmoText(gunModelBase.CurrentMagazine, gunModelBase.CurrentAmmo);
	}

	/// <summary>
	/// ロードUIを表示する
	/// </summary>
	public void ShowLoadingPanel()
	{
		//不透明にする
		ScreenUIManagerPresenter.SingletonInstance.FadeOut();
		//スライダーの値を最低にする
		ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.SliderLoading.value = float.MinValue;
		//ロードUIをOnにする
		ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.PanelLoading.gameObject.SetActive(true);
	}

	/// <summary>
	/// ロードUIを非表示にする
	/// </summary>
	public void HideLoadingPanel()
	{
		ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.PanelLoading.gameObject.SetActive(false);
	}

	/// <summary>
	/// ロード数値をスライダーに反映する
	/// </summary>
	public void SetLoadingSliderValue(float progress)
	{
		ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.SliderLoading.value = progress;
	}
}

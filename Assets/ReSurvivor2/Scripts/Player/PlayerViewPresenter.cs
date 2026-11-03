using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤービュープレゼンター（プレイヤーの状態と知らせに合わせて、プレイヤーの見た目を動かす）
/// MVPパターンのPresenter担当
/// ※プレイヤーのロジック（PlayerManagerPresenter・PlayerModel・銃・ミッションの制限時間）は、このクラスとビューを知らない。
///   このコンポーネントとビュー（キャラクターのモデル・プレイヤーUI）を外しても、プレイヤーのロジックだけでゲームは動く
///   （キャラクターのアニメーション：PlayerCharacterView、プレイヤーUI：PlayerUIView、銃の見た目と音：GunViewPresenter）
/// </summary>
public class PlayerViewPresenter : MonoBehaviour
{
	[Tooltip("プレイヤーのロジック（知らせと、プレイヤーの状態を受け取る）")]
	[SerializeField] PlayerManagerPresenter playerManagerPresenter;

	[Tooltip("プレイヤーキャラクタービュー")]
	[SerializeField] PlayerCharacterView playerCharacterView;

	[Tooltip("プレイヤーUIビュー")]
	[SerializeField] PlayerUIView playerUIView;

	[Tooltip("ガンビュープレゼンター（銃の知らせを受け取って、銃のビューに見た目と音を出させる）")]
	readonly GunViewPresenter gunViewPresenter = new GunViewPresenter();

	[Tooltip("知らせを受け取っているプレイヤーのモデル（つないでいない時はnull）")]
	PlayerModel playerModel;

	[Tooltip("知らせを受け取っている、ミッションの制限時間（つないでいない時はnull）")]
	TimerManager timerManager;

	[Tooltip("地雷を置いていない時の、地雷設置イメージのフィルアマウント")]
	const float NoMinePlacingFillAmount = 0.0f;

	void Start()
	{
		playerModel = playerManagerPresenter.PlayerModel;
		GunModelFacade gunModelFacade = PlayerCameraManager.SingletonInstance.GunModelFacade;
		//プレイヤーのアニメーションは、銃の状態を見て動かすので、銃の状態を読むための窓口を渡しておく
		playerCharacterView.Initialize(gunModelFacade, PlayerCameraManager.SingletonInstance.transform);
		//銃の見せ方・マズルフラッシュ・音は、銃の知らせを受けて出す
		gunViewPresenter.Bind(gunModelFacade, playerCharacterView.GunViewFacade);

		playerManagerPresenter.OnAfterUpdate += AfterUpdate;
		playerManagerPresenter.OnAfterGunUpdate += AfterGunUpdate;
		playerManagerPresenter.OnLateUpdate += UpdateAimBones;
		playerManagerPresenter.OnMoveReset += ResetMove;
		playerModel.OnDamaged += UpdateHP;
		playerModel.OnHealed += UpdateHP;
		playerModel.OnStaminaConsumed += UpdateStamina;
		playerModel.OnStaminaRestored += UpdateStamina;
		playerModel.OnArmorPlateChanged += playerUIView.SetTextArmorPlate;
		playerModel.OnFoodChanged += playerUIView.SetTextFood;
		playerModel.OnMineCountChanged += SetTextMine;
		timerManager = TimerManager.SingletonInstance;
		timerManager.OnTimerUpdated += UpdateTimer;

		//今のプレイヤーの状態を表示する
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
		playerUIView.InitHP(playerModel.HP.CurrentHp);
		playerUIView.InitStamina(playerModel.Stamina.CurrentStamina);
		playerUIView.StartTextArmorPlate(playerModel.CurrentArmorPlate);
		playerUIView.StartTextFood(playerModel.CurrentFood);
		SetTextMine(playerModel.CurrentMine);
		playerUIView.SetTextMagazine(gunModelFacade.GunModelBase.CurrentMagazine, gunModelFacade.GunModelBase.IsMagazineLow);
		playerUIView.SetTextAmmo(gunModelFacade.GunModelBase.CurrentAmmo, gunModelFacade.GunModelBase.IsAmmoLow);
		playerUIView.UpdateUITransform(playerModel.IsAim);
	}

	void OnDestroy()
	{
		//破棄した後のビューに、プレイヤーと銃の知らせが届かないようにする
		gunViewPresenter.Unbind();
		if (playerModel == null)
		{
			return;
		}

		playerManagerPresenter.OnAfterUpdate -= AfterUpdate;
		playerManagerPresenter.OnAfterGunUpdate -= AfterGunUpdate;
		playerManagerPresenter.OnLateUpdate -= UpdateAimBones;
		playerManagerPresenter.OnMoveReset -= ResetMove;
		playerModel.OnDamaged -= UpdateHP;
		playerModel.OnHealed -= UpdateHP;
		playerModel.OnStaminaConsumed -= UpdateStamina;
		playerModel.OnStaminaRestored -= UpdateStamina;
		playerModel.OnArmorPlateChanged -= playerUIView.SetTextArmorPlate;
		playerModel.OnFoodChanged -= playerUIView.SetTextFood;
		playerModel.OnMineCountChanged -= SetTextMine;
		timerManager.OnTimerUpdated -= UpdateTimer;
		playerModel = null;
		timerManager = null;
	}

	/// <summary>
	/// プレイヤーの更新処理の後に、移動・構え・地雷を置くアニメーションと、プレイヤーUIを動かす
	/// </summary>
	void AfterUpdate()
	{
		NormalMoveAnimation();
		GunModelFacade gunModelFacade = PlayerCameraManager.SingletonInstance.GunModelFacade;
		playerUIView.UpdateUITransform(playerModel.IsAim);
		playerUIView.UpdateImageReload(gunModelFacade.GunModelBase.IsReloadTimeActive);
		playerUIView.UpdateImageSideArm(gunModelFacade.IsSideArmDrawWindowOpen);
		playerUIView.SetTextMagazine(gunModelFacade.GunModelBase.CurrentMagazine, gunModelFacade.GunModelBase.IsMagazineLow);
		playerUIView.SetTextAmmo(gunModelFacade.GunModelBase.CurrentAmmo, gunModelFacade.GunModelBase.IsAmmoLow);
		playerUIView.SetMinePlacingFillAmount(GetMinePlacingFillAmount());
	}

	/// <summary>
	/// 移動アニメーション（プレイヤーの状態を、プレイヤーキャラクタービューに渡す）
	/// </summary>
	void NormalMoveAnimation()
	{
		//移動のアニメーションには、移動の向きと速さの割合を渡す（移動スティックを少しだけ倒した時は、歩くアニメーションになる）
		Vector2 move = playerModel.MoveInputDirection * playerModel.MoveRate;
		playerCharacterView.UpdateMoveAnimation(move.x, move.y, playerModel.IsDash, playerModel.IsPlacingMine);
		//※敵のマネージャーがないシーンでは、警戒体制の敵はいないことにする
		bool isEnemyAlert = EnemyManager.SingletonInstance != null && EnemyManager.SingletonInstance.IsAnyEnemyAlert() == true;
		playerCharacterView.UpdateAimAnimation(playerModel.IsAim, isEnemyAlert, PlayerCameraManager.SingletonInstance.IsEnemyClose);
		//地雷を置く時は、ボタンを押している間しゃがんで左手を地面へ下ろし、置いた後（置くのをやめた後）に立ち上がる
		playerCharacterView.UpdatePlaceMineAnimation(playerModel.MinePlacePhase, playerModel.MineSpawnCount / PlayerModel.Mine_Hold_Time, playerModel.MineStandUpCount / PlayerModel.Mine_Stand_Up_Time);
	}

	/// <summary>
	/// 地雷設置イメージのフィルアマウントを求める（しゃがんでいる間は、長押しした時間の割合。それ以外は0）
	/// </summary>
	/// <returns>地雷設置イメージのフィルアマウント</returns>
	float GetMinePlacingFillAmount()
	{
		if (playerModel.MinePlacePhase != EnumManager.MinePlacePhaseTYPE.Crouch)
		{
			return NoMinePlacingFillAmount;
		}

		return playerModel.MineSpawnCount / PlayerModel.Mine_Hold_Time;
	}

	/// <summary>
	/// 銃の更新処理の後に、銃の状態に合わせて、プレイヤーのリロードのアニメーションを動かす
	/// </summary>
	void AfterGunUpdate()
	{
		playerCharacterView.UpdateReloadAnimation();
	}

	/// <summary>
	/// アニメーションで骨が動いた後に、構えに合わせて背骨を曲げる
	/// </summary>
	void UpdateAimBones()
	{
		playerCharacterView.UpdateAimBones(playerModel.IsAim);
	}

	/// <summary>
	/// 移動の状態をリセットした時に、移動のアニメーションと、地雷設置イメージを元に戻す
	/// </summary>
	void ResetMove()
	{
		playerCharacterView.ResetMoveAnimation();
		playerUIView.SetMinePlacingFillAmount(NoMinePlacingFillAmount);
	}

	/// <summary>
	/// HPのゲージと、HPに合わせた画面の色を更新する（ダメージを受けた時・回復した時）
	/// </summary>
	void UpdateHP()
	{
		playerUIView.SliderHp.value = playerModel.HP.CurrentHp / HitPoint.Max_Hp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
	}

	/// <summary>
	/// スタミナのゲージを更新する（スタミナを使った時・回復した時）
	/// </summary>
	void UpdateStamina()
	{
		playerUIView.SliderStamina.value = playerModel.Stamina.CurrentStamina / Stamina.Max_Stamina;
	}

	/// <summary>
	/// 地雷の数を表示する
	/// </summary>
	/// <param name="currentMine">地雷の数</param>
	void SetTextMine(int currentMine)
	{
		playerUIView.TextMine.text = currentMine.ToString();
	}

	/// <summary>
	/// ミッションの制限時間を表示する（ミッション中でない時は「--:--」。残り時間が少ない時は赤くする）
	/// </summary>
	void UpdateTimer()
	{
		if (timerManager.IsTimerActive == false)
		{
			playerUIView.TextTimer.text = "--" + ":" + "--";
			playerUIView.SetTextTimerColor(false);
			return;
		}

		//残り時間が少なくなったら、タイマーの文字を赤くする
		playerUIView.SetTextTimerColor(timerManager.IsTimeLow);

		if (timerManager.IsTimeUp == true)
		{
			playerUIView.TextTimer.text = "00" + ":" + "00";
			return;
		}

		playerUIView.TextTimer.text = timerManager.Minute.ToString("00") + ":" + ((int)timerManager.Seconds).ToString("00");
	}
}

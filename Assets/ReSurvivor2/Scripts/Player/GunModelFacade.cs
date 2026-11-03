using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ガンモデルファサード
/// MVPパターンのModel担当
/// </summary>
[System.Serializable]
public class GunModelFacade
{
	[Tooltip("ハンドガン")]
	[SerializeField] HandGunModel handGunModel = new HandGunModel();

	[Tooltip("アサルトライフル")]
	[SerializeField] AssaultRifleModel assaultRifleModel = new AssaultRifleModel();

	[Tooltip("ショットガン")]
	[SerializeField] ShotGunModel shotGunModel = new ShotGunModel();

	GunModelBase gunModelBase;
	public GunModelBase GunModelBase => gunModelBase;

	List<GunModelBase> gunModelBaseList = new List<GunModelBase>();

	EnumManager.GunTYPE gunTYPE;
	public EnumManager.GunTYPE GetGunTYPE => gunTYPE;

	//※アサルトライフル・ショットガンのマガジンを撃ち尽くして、オートリロードが始まる前に撃つボタンを押し直すと、
	//  メインの銃を左手に持ち替えて、右手でハンドガンを抜いて撃つ（持ち替えている間は、今持っている銃をハンドガンにする）。
	//  構えをやめた時・ハンドガンの弾も切れた時・リロードボタンを押した時は、ハンドガンをしまってメインの銃を持ち直し、すぐにリロードする
	[Tooltip("ハンドガンに持ち替える動きの段階")]
	EnumManager.SideArmPhaseTYPE sideArmPhase = EnumManager.SideArmPhaseTYPE.None;
	public EnumManager.SideArmPhaseTYPE SideArmPhase => sideArmPhase;
	[Tooltip("ハンドガンに持ち替える動きの、今の段階になってからの時間（秒）")]
	float sideArmPhaseTimer = 0.0f;
	[Tooltip("ハンドガンに持ち替える前に持っていた、メインの銃（アサルトライフル・ショットガン）")]
	EnumManager.GunTYPE sideArmMainGunTYPE;
	public EnumManager.GunTYPE SideArmMainGunTYPE => sideArmMainGunTYPE;
	[Tooltip("メインの銃を左手に持ち替えて、右手でハンドガンを抜き終わるまでの時間（秒）")]
	const float SideArmDrawTime = 0.5f;
	[Tooltip("ハンドガンをしまって、メインの銃を右手に持ち直し終わるまでの時間（秒）")]
	const float SideArmHolsterTime = 0.5f;
	[Tooltip("ハンドガンを抜く動き・しまう動きのうち、右手がホルスターに着く割合（0〜1）（ここでハンドガンを手に持つ・ホルスターに戻す。しまう時は、ここから今持っている銃をメインの銃に戻す）")]
	public const float SideArmHolsterReachRate = 0.5f;
	[Tooltip("ハンドガンを抜く動き・しまう動きの、始まりの進み具合")]
	const float SideArmStartProgress = 0.0f;

	/// <summary>
	/// ハンドガンを抜く動き・しまう動きの進み具合（0が始まり、1が終わり。それ以外の段階では0）
	/// </summary>
	public float SideArmPhaseProgress
	{
		get
		{
			switch (sideArmPhase)
			{
				case EnumManager.SideArmPhaseTYPE.Draw:
					return Mathf.Clamp01(sideArmPhaseTimer / SideArmDrawTime);
				case EnumManager.SideArmPhaseTYPE.Holster:
					return Mathf.Clamp01(sideArmPhaseTimer / SideArmHolsterTime);
			}
			return SideArmStartProgress;
		}
	}

	/// <summary>
	/// ハンドガンに持ち替えている間か？（抜いている間・狙っている間・しまっている間）
	/// </summary>
	public bool IsSideArmActive => sideArmPhase != EnumManager.SideArmPhaseTYPE.None;

	//※武器の切り替えボタンを押すと、今持っている銃を体のしまう場所へしまってから、切り替える先の銃を取り出して持つ。
	//  しまい終わった時に、今持っている銃を切り替える先の銃にする（切り替えている間は、撃つこともリロードすることもできない）
	[Tooltip("武器を切り替える動きの段階")]
	EnumManager.WeaponSwitchPhaseTYPE weaponSwitchPhase = EnumManager.WeaponSwitchPhaseTYPE.None;
	public EnumManager.WeaponSwitchPhaseTYPE WeaponSwitchPhase => weaponSwitchPhase;
	[Tooltip("武器を切り替える動きの、今の段階になってからの時間（秒）")]
	float weaponSwitchPhaseTimer = 0.0f;
	[Tooltip("切り替える先の銃")]
	EnumManager.GunTYPE weaponSwitchTargetGunTYPE;
	[Tooltip("今持っている銃を、体のしまう場所へしまい終わるまでの時間（秒）（ハンドガン・アサルトライフル）")]
	const float WeaponPutAwayTime = 0.35f;
	[Tooltip("切り替える先の銃を、体のしまう場所から取り出して持ち終わるまでの時間（秒）（ハンドガン・アサルトライフル）")]
	const float WeaponTakeOutTime = 0.45f;
	//※ショットガンは背中にしまうので、右手で銃の前側に持ち替えて、右脇の下を通して背中との間を動かす分だけ、ほかの銃より長くかける
	[Tooltip("ショットガンを、背中へしまい終わるまでの時間（秒）")]
	const float ShotGunPutAwayTime = 0.5f;
	[Tooltip("ショットガンを、背中から取り出して持ち終わるまでの時間（秒）")]
	const float ShotGunTakeOutTime = 0.6f;
	[Tooltip("銃をしまう動き・取り出す動きの、始まりの進み具合")]
	const float WeaponSwitchStartProgress = 0.0f;

	/// <summary>
	/// 今持っている銃を、体のしまう場所へしまい終わるまでの時間（秒）（しまっている間は、今持っている銃がしまう銃）
	/// </summary>
	float CurrentWeaponPutAwayTime => gunTYPE == EnumManager.GunTYPE.ShotGun ? ShotGunPutAwayTime : WeaponPutAwayTime;

	/// <summary>
	/// 切り替える先の銃を、体のしまう場所から取り出して持ち終わるまでの時間（秒）（取り出している間は、今持っている銃が取り出す銃）
	/// </summary>
	float CurrentWeaponTakeOutTime => gunTYPE == EnumManager.GunTYPE.ShotGun ? ShotGunTakeOutTime : WeaponTakeOutTime;

	/// <summary>
	/// 銃をしまう動き・取り出す動きの進み具合（0が始まり、1が終わり。切り替えていない時は0）
	/// </summary>
	public float WeaponSwitchPhaseProgress
	{
		get
		{
			switch (weaponSwitchPhase)
			{
				case EnumManager.WeaponSwitchPhaseTYPE.PutAway:
					return Mathf.Clamp01(weaponSwitchPhaseTimer / CurrentWeaponPutAwayTime);
				case EnumManager.WeaponSwitchPhaseTYPE.TakeOut:
					return Mathf.Clamp01(weaponSwitchPhaseTimer / CurrentWeaponTakeOutTime);
			}
			return WeaponSwitchStartProgress;
		}
	}

	/// <summary>
	/// 武器を切り替えている間か？（しまっている間・取り出している間）
	/// </summary>
	public bool IsWeaponSwitching => weaponSwitchPhase != EnumManager.WeaponSwitchPhaseTYPE.None;

	/// <summary>
	/// 選んでいる銃（武器を切り替えている間は、切り替える先の銃。十字キーの武器アイコンで選択中にする銃）
	/// </summary>
	public EnumManager.GunTYPE SelectedGunTYPE => IsWeaponSwitching == true ? weaponSwitchTargetGunTYPE : gunTYPE;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public GunModelFacade()
	{
		gunModelBaseList.Add(handGunModel);
		gunModelBaseList.Add(assaultRifleModel);
		gunModelBaseList.Add(shotGunModel);
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		UnityEngine.Debug.Log("<color=cyan>ガンファサードセーブ</color>");
		foreach (var gun in gunModelBaseList)
		{
			gun.Save();
		}
		//ハンドガンに持ち替えている間は、持ち替える前のメインの銃を、武器を切り替えている間は、切り替える先の銃を、今持っている銃として残す
		ES3.Save<EnumManager.GunTYPE>("CurrentGunType", IsSideArmActive == true ? this.sideArmMainGunTYPE : SelectedGunTYPE);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		//UnityEngine.Debug.Log("<color=purple>ガンファサードロード</color>");
		foreach (var gun in gunModelBaseList)
		{
			gun.Load();
		}
		this.gunTYPE = ES3.Load<EnumManager.GunTYPE>("CurrentGunType", EnumManager.GunTYPE.HandGun);
		SwitchWeapon(this.gunTYPE);
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
	}

	/// <summary>
	/// 銃の更新処理
	/// </summary>
	public void UpdateGun(bool isAim)
	{
		if (IsSideArmActive == true)
		{
			UpdateSideArm(isAim);
			return;
		}

		SwitchWeaponTrigger();
		//武器を切り替えている間は、撃つこともリロードすることもできない
		if (IsWeaponSwitching == true)
		{
			UpdateWeaponSwitch();
			return;
		}

		if (CanDrawSideArm(isAim) == true)
		{
			StartSideArmDraw();
			return;
		}
		gunModelBase.AllSystem(isAim);
	}

	/// <summary>
	/// 武器の切り替えトリガー
	/// </summary>
	void SwitchWeaponTrigger()
	{
		//リロード中と、地雷を置いている間は、武器を切り替えない（地雷を置くしゃがむ動きと、銃をしまう動きが重ならないようにする）
		if (gunModelBase.IsReloadTimeActive == true || PlayerManagerPresenter.SingletonInstance.PlayerModel.MinePlacePhase != EnumManager.MinePlacePhaseTYPE.None)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1) || 0.5 < Input.GetAxisRaw("XInput DPad Left&Right"))
		{
			//Debug.Log("ハンドガン");
			ChangeWeapon(EnumManager.GunTYPE.HandGun);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			ChangeWeapon(EnumManager.GunTYPE.AssaultRifle);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			ChangeWeapon(EnumManager.GunTYPE.ShotGun);
		}
	}

	/// <summary>
	/// 武器を持ち替える（武器の切り替えボタンを押した時）
	/// </summary>
	/// <param name="gunTYPE">持ち替える銃のタイプ</param>
	void ChangeWeapon(EnumManager.GunTYPE gunTYPE)
	{
		//十字キーの武器アイコンと弾数を一時的に表示する
		ScreenUIManagerPresenter.SingletonInstance.ShowWeaponQuickSelect();

		//ハンドガンに持ち替えている時は、持ち替えをやめて、すぐに選んだ銃を持つ（左手に持っていたメインの銃は、ビューが元の位置に戻す）
		if (IsSideArmActive == true)
		{
			ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE.None);
			this.gunTYPE = gunTYPE;
			SwitchWeapon(this.gunTYPE);
			PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
			return;
		}

		switch (weaponSwitchPhase)
		{
			case EnumManager.WeaponSwitchPhaseTYPE.None:
				//今持っている銃を選んだ時は、切り替えない
				if (gunTYPE == this.gunTYPE)
				{
					return;
				}

				weaponSwitchTargetGunTYPE = gunTYPE;
				ChangeWeaponSwitchPhase(EnumManager.WeaponSwitchPhaseTYPE.PutAway);
				break;
			case EnumManager.WeaponSwitchPhaseTYPE.PutAway:
				//しまっている間に別の銃を選んだ時は、取り出す銃だけを変える
				weaponSwitchTargetGunTYPE = gunTYPE;
				break;
			case EnumManager.WeaponSwitchPhaseTYPE.TakeOut:
				//取り出している間は、選び直せない（取り出し終わってから、もう一度選ぶ）
				break;
		}
	}

	/// <summary>
	/// 武器を切り替えている間の処理（今持っている銃をしまう → 切り替える先の銃を取り出す）
	/// </summary>
	void UpdateWeaponSwitch()
	{
		weaponSwitchPhaseTimer = weaponSwitchPhaseTimer + Time.deltaTime;
		switch (weaponSwitchPhase)
		{
			case EnumManager.WeaponSwitchPhaseTYPE.PutAway:
				//しまい終わったら、今持っている銃を切り替える先の銃にして、取り出し始める
				//（しまった銃は体のしまう場所に見せて、取り出す銃は手に持つ方を見せる。取り出す銃の位置・向きは、ビューがしまう場所から動かす）
				if (CurrentWeaponPutAwayTime <= weaponSwitchPhaseTimer)
				{
					gunTYPE = weaponSwitchTargetGunTYPE;
					SwitchWeapon(gunTYPE);
					PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(gunTYPE);
					ChangeWeaponSwitchPhase(EnumManager.WeaponSwitchPhaseTYPE.TakeOut);
				}
				break;
			case EnumManager.WeaponSwitchPhaseTYPE.TakeOut:
				if (CurrentWeaponTakeOutTime <= weaponSwitchPhaseTimer)
				{
					ChangeWeaponSwitchPhase(EnumManager.WeaponSwitchPhaseTYPE.None);
				}
				break;
		}
	}

	/// <summary>
	/// 武器を切り替える動きの段階を変える
	/// </summary>
	/// <param name="phase">次の段階</param>
	void ChangeWeaponSwitchPhase(EnumManager.WeaponSwitchPhaseTYPE phase)
	{
		weaponSwitchPhase = phase;
		weaponSwitchPhaseTimer = 0.0f;
	}

	/// <summary>
	/// 武器の切り替えをすぐに終わらせて、切り替える先の銃を持っている状態にする（ステージを移る時などに呼ぶ）
	/// </summary>
	public void CancelWeaponSwitch()
	{
		if (IsWeaponSwitching == false)
		{
			return;
		}

		ChangeWeaponSwitchPhase(EnumManager.WeaponSwitchPhaseTYPE.None);
		gunTYPE = weaponSwitchTargetGunTYPE;
		SwitchWeapon(gunTYPE);
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(gunTYPE);
	}

	/// <summary>
	/// 武器の切り替え初期化処理
	/// </summary>
	void SwitchWeapon(EnumManager.GunTYPE gunTYPE)
	{
		foreach (GunModelBase gun in gunModelBaseList)
		{
			if (gun.GetGunType == gunTYPE)
			{
				gunModelBase = gun;
			}
		}
	}

	/// <summary>
	/// ハンドガンに持ち替えられる間か？（構えて撃つボタンを押し直すと持ち替える）
	/// （アサルトライフル・ショットガンのマガジンが空で、オートリロードが始まる前）
	/// ※予備の弾が0の時はオートリロードが始まらないので、いつでも持ち替えられる
	/// ※プレイヤーUIの、ハンドガンに持ち替えられることを表すアイコンにも使う
	/// </summary>
	public bool IsSideArmDrawWindowOpen
	{
		get
		{
			//武器を切り替えている間は、持ち替えない
			if (gunTYPE == EnumManager.GunTYPE.HandGun || IsWeaponSwitching == true)
			{
				return false;
			}

			if (0 < gunModelBase.CurrentMagazine || gunModelBase.IsReloadTimeActive == true)
			{
				return false;
			}

			//ハンドガンのマガジンも空の時は、持ち替えても撃てない（持ち替えている間はリロードできない）ので持ち替えない
			if (handGunModel.CurrentMagazine <= 0)
			{
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// ハンドガンに持ち替えられるか？
	/// （ハンドガンに持ち替えられる間に、構えたまま撃つボタンを押し直した時）
	/// </summary>
	/// <param name="isAim">構えているか？</param>
	bool CanDrawSideArm(bool isAim)
	{
		if (isAim == false || IsSideArmDrawWindowOpen == false)
		{
			return false;
		}

		return GunModelBase.IsFireButtonDown();
	}

	/// <summary>
	/// メインの銃を左手に持ち替えて、右手でハンドガンを抜き始める（今持っている銃をハンドガンにする）
	/// </summary>
	void StartSideArmDraw()
	{
		sideArmMainGunTYPE = gunTYPE;
		gunTYPE = EnumManager.GunTYPE.HandGun;
		SwitchWeapon(gunTYPE);
		sideArmPhase = EnumManager.SideArmPhaseTYPE.Draw;
		sideArmPhaseTimer = 0.0f;
	}

	/// <summary>
	/// ハンドガンに持ち替えている間の処理（抜く → 狙って撃つ → しまってメインの銃に戻す）
	/// </summary>
	/// <param name="isAim">構えているか？</param>
	void UpdateSideArm(bool isAim)
	{
		sideArmPhaseTimer = sideArmPhaseTimer + Time.deltaTime;
		switch (sideArmPhase)
		{
			case EnumManager.SideArmPhaseTYPE.Draw:
				if (SideArmDrawTime <= sideArmPhaseTimer)
				{
					//抜き終わったら、持ち替えるきっかけになった撃つボタンの分を撃つ
					ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE.Aim);
					handGunModel.SideArmSystem(isAim, true);
				}
				break;
			case EnumManager.SideArmPhaseTYPE.Aim:
				SwitchWeaponTrigger();
				if (IsSideArmActive == false)
				{
					return;
				}

				//構えをやめた時・ハンドガンの弾も切れた時・リロードボタンを押した時は、ハンドガンをしまってメインの銃に戻す
				if (isAim == false || handGunModel.CurrentMagazine <= 0 || GunModelBase.IsReloadButtonPressed() == true)
				{
					ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE.Holster);
					break;
				}
				handGunModel.SideArmSystem(isAim, false);
				break;
			case EnumManager.SideArmPhaseTYPE.Holster:
				//ハンドガンをホルスターに戻したら、今持っている銃をメインの銃に戻す（右手でメインの銃を持ち直す）
				if (gunTYPE == EnumManager.GunTYPE.HandGun && SideArmHolsterReachRate <= SideArmPhaseProgress)
				{
					gunTYPE = sideArmMainGunTYPE;
					SwitchWeapon(gunTYPE);
				}

				//持ち直したら、すぐにメインの銃をリロードする（オートリロードの待ち時間は飛ばす）
				if (SideArmHolsterTime <= sideArmPhaseTimer)
				{
					ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE.None);
					gunModelBase.SkipAutoReloadWait();
				}
				break;
		}
	}

	/// <summary>
	/// ハンドガンに持ち替える動きの段階を変える
	/// </summary>
	/// <param name="phase">次の段階</param>
	void ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE phase)
	{
		sideArmPhase = phase;
		sideArmPhaseTimer = 0.0f;
	}

	/// <summary>
	/// ハンドガンへの持ち替えをすぐにやめて、メインの銃を持っている状態に戻す（ステージを移る時に呼ぶ）
	/// </summary>
	public void CancelSideArm()
	{
		if (IsSideArmActive == false)
		{
			return;
		}

		ChangeSideArmPhase(EnumManager.SideArmPhaseTYPE.None);
		gunTYPE = sideArmMainGunTYPE;
		SwitchWeapon(gunTYPE);
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(gunTYPE);
	}

	/// <summary>
	/// 銃のタイプから銃を探す
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <returns>見つかった銃（見つからなかった場合はnull）</returns>
	public GunModelBase FindGunBase(EnumManager.GunTYPE gunTYPE)
	{
		foreach (GunModelBase gun in gunModelBaseList)
		{
			if (gun.GetGunType == gunTYPE)
			{
				return gun;
			}
		}
		return null;
	}

	/// <summary>
	/// 弾を取得
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public void AcquireAmmo(EnumManager.GunTYPE gunTYPE, int amount = 10, UnityAction unityAction = null)
	{
		foreach (var gun in gunModelBaseList)
		{
			if (gun.GetGunType == gunTYPE)
			{
				gun.AcquireAmmo(amount, unityAction);
			}
		}
	}
}

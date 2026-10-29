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
		//ハンドガンに持ち替えている間は、持ち替える前のメインの銃を、今持っている銃として残す
		ES3.Save<EnumManager.GunTYPE>("CurrentGunType", IsSideArmActive == true ? this.sideArmMainGunTYPE : this.gunTYPE);
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
		if (gunModelBase.IsReloadTimeActive == true)
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
		//ハンドガンに持ち替えている時は、持ち替えをやめる（左手に持っていたメインの銃は、ビューが元の位置に戻す）
		sideArmPhase = EnumManager.SideArmPhaseTYPE.None;
		sideArmPhaseTimer = 0.0f;
		this.gunTYPE = gunTYPE;
		SwitchWeapon(this.gunTYPE);
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
		//十字キーの武器アイコンと弾数を一時的に表示する
		ScreenUIManagerPresenter.SingletonInstance.ShowWeaponQuickSelect();
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
			if (gunTYPE == EnumManager.GunTYPE.HandGun)
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

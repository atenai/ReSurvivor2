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
		ES3.Save<EnumManager.GunTYPE>("CurrentGunType", this.gunTYPE);
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
		SwitchWeaponTrigger();
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
			this.gunTYPE = EnumManager.GunTYPE.HandGun;
			SwitchWeapon(this.gunTYPE);
			PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
			//十字キーの武器アイコンと弾数を一時的に表示する
			ScreenUIManagerPresenter.SingletonInstance.ShowWeaponQuickSelect();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			this.gunTYPE = EnumManager.GunTYPE.AssaultRifle;
			SwitchWeapon(this.gunTYPE);
			PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
			//十字キーの武器アイコンと弾数を一時的に表示する
			ScreenUIManagerPresenter.SingletonInstance.ShowWeaponQuickSelect();
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			this.gunTYPE = EnumManager.GunTYPE.ShotGun;
			SwitchWeapon(this.gunTYPE);
			PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.GunViewFacade.SwitchWeaponModel(this.gunTYPE);
			//十字キーの武器アイコンと弾数を一時的に表示する
			ScreenUIManagerPresenter.SingletonInstance.ShowWeaponQuickSelect();
		}
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

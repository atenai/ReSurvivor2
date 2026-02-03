using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ガンファサード
/// </summary>
[System.Serializable]
public class GunFacade
{
	[Tooltip("ハンドガン")]
	[SerializeField] HandGun handGun = new HandGun();

	[Tooltip("アサルトライフル")]
	[SerializeField] AssaultRifle assaultRifle = new AssaultRifle();

	[Tooltip("ショットガン")]
	[SerializeField] ShotGun shotGun = new ShotGun();

	GunBase gunBase;
	public GunBase GetGunBase => gunBase;

	List<GunBase> gunBaseList = new List<GunBase>();

	EnumManager.GunTYPE gunTYPE;
	public EnumManager.GunTYPE GetGunTYPE => gunTYPE;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public GunFacade()
	{
		gunBaseList.Add(handGun);
		gunBaseList.Add(assaultRifle);
		gunBaseList.Add(shotGun);
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		UnityEngine.Debug.Log("<color=cyan>ガンファサードセーブ</color>");
		foreach (var gun in gunBaseList)
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
		UnityEngine.Debug.Log("<color=purple>ガンファサードロード</color>");
		foreach (var gun in gunBaseList)
		{
			gun.Load();
		}
		this.gunTYPE = ES3.Load<EnumManager.GunTYPE>("CurrentGunType", EnumManager.GunTYPE.HandGun);
		SwitchWeapon(this.gunTYPE);
	}

	/// <summary>
	/// 銃の更新処理
	/// </summary>
	public void UpdateGun()
	{
		SwitchWeaponTrigger();
		gunBase.AllSystem();
	}

	/// <summary>
	/// 武器の切り替えトリガー
	/// </summary> 
	void SwitchWeaponTrigger()
	{
		if (gunBase.IsReloadTimeActive == true)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1) || 0.5 < Input.GetAxisRaw("XInput DPad Left&Right"))
		{
			//Debug.Log("ハンドガン");
			this.gunTYPE = EnumManager.GunTYPE.HandGun;
			SwitchWeapon(this.gunTYPE);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			this.gunTYPE = EnumManager.GunTYPE.AssaultRifle;
			SwitchWeapon(this.gunTYPE);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			this.gunTYPE = EnumManager.GunTYPE.ShotGun;
			SwitchWeapon(this.gunTYPE);
		}
	}

	/// <summary>
	/// 武器の切り替え初期化処理
	/// </summary> 
	void SwitchWeapon(EnumManager.GunTYPE gunTYPE)
	{
		foreach (var gun in gunBaseList)
		{
			if (gun.GetGunType == gunTYPE)
			{
				gunBase = gun;
			}
		}

		Player.SingletonInstance.GunModelFacade.SwitchWeaponModel(gunTYPE);
	}

	/// <summary>
	/// 弾を取得
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public void AcquireAmmo(EnumManager.GunTYPE gunTYPE, int amount = 10, UnityAction unityAction = null)
	{
		foreach (var gun in gunBaseList)
		{
			if (gun.GetGunType == gunTYPE)
			{
				gun.AcquireAmmo(amount, unityAction);
			}
		}
	}
}

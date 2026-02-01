using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンファサード
/// </summary>
[System.Serializable]
public class GunFacade
{
	public enum GunTYPE
	{
		HandGun = 1,
		AssaultRifle = 2,
		ShotGun = 3,
	}

	GunTYPE gunTYPE;
	public GunTYPE GetGunTYPE => gunTYPE;

	/// <summary>
	/// ハンドガン
	/// </summary>
	[SerializeField] HandGun handGun = new HandGun();

	/// <summary>
	/// アサルトライフル
	/// </summary>
	[SerializeField] AssaultRifle assaultRifle = new AssaultRifle();

	/// <summary>
	/// ショットガン
	/// </summary>
	[SerializeField] ShotGun shotGun = new ShotGun();

	GunBase gunBase;
	public GunBase GetGunBase => gunBase;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>ガンファサードセーブ</color>");
		handGun.Save();
		assaultRifle.Save();
		shotGun.Save();
		ES3.Save<GunTYPE>("CurrentGunType", gunTYPE);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=purple>ガンファサードロード</color>");
		handGun.Load();
		assaultRifle.Load();
		shotGun.Load();
		gunTYPE = ES3.Load<GunTYPE>("CurrentGunType", GunTYPE.HandGun);
	}

	/// <summary>
	/// 武器の切り替え初期化処理
	/// </summary> 
	public void InitSwitchWeapon()
	{
		switch (gunTYPE)
		{
			case GunTYPE.HandGun:
				gunBase = handGun;
				break;
			case GunTYPE.AssaultRifle:
				gunBase = assaultRifle;
				break;
			case GunTYPE.ShotGun:
				gunBase = shotGun;
				break;
		}
	}

	/// <summary>
	/// 武器の切り替え更新処理
	/// </summary> 
	public void UpdateSwitchWeapon()
	{
		if (gunBase.IsReloadTimeActive == true)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1) || 0.5 < Input.GetAxisRaw("XInput DPad Left&Right"))
		{
			//Debug.Log("ハンドガン");
			gunTYPE = GunTYPE.HandGun;
			gunBase = handGun;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			gunTYPE = GunTYPE.AssaultRifle;
			gunBase = assaultRifle;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			gunTYPE = GunTYPE.ShotGun;
			gunBase = shotGun;
		}
	}

	/// <summary>
	/// 武器の更新
	/// </summary>
	public void UpdateGun()
	{
		gunBase.UpdateGun();
	}

	/// <summary>
	/// ハンドガンの弾を取得
	/// </summary>
	public void AcquireHandGunAmmo()
	{
		if (handGun.MaxHandGunAmmo <= handGun.CurrentAmmo)
		{
			return;
		}

		handGun.AcquireAmmo();
	}

	/// <summary>
	/// アサルトライフルの弾を取得
	/// </summary>
	public void AcquireAssaultRifleAmmo()
	{
		if (assaultRifle.MaxAssaultRifleAmmo <= assaultRifle.CurrentAmmo)
		{
			return;
		}

		assaultRifle.AcquireAmmo();
	}

	/// <summary>
	/// ショットガンの弾を取得
	/// </summary>
	public void AcquireShotGunAmmo()
	{
		if (shotGun.MaxShotGunAmmo <= shotGun.CurrentAmmo)
		{
			return;
		}

		shotGun.AcquireAmmo();
	}
}

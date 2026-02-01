using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンファサード
/// </summary>
[System.Serializable]
public class GunFacade
{
	/// <summary>
	/// 銃のタイプ
	/// </summary>
	public enum GunTYPE
	{
		HandGun = 1,
		AssaultRifle = 2,
		ShotGun = 3,
	}

	GunTYPE gunTYPE;
	public GunTYPE GetGunTYPE => gunTYPE;

	[Tooltip("ハンドガン")]
	[SerializeField] HandGun handGun = new HandGun();
	/// <summary>ハンドガン </summary>
	public HandGun HandGun => handGun;

	[Tooltip("アサルトライフル")]
	[SerializeField] AssaultRifle assaultRifle = new AssaultRifle();
	/// <summary>アサルトライフル</summary>
	public AssaultRifle AssaultRifle => assaultRifle;

	[Tooltip("ショットガン")]
	[SerializeField] ShotGun shotGun = new ShotGun();
	/// <summary> ショットガン </summary>
	public ShotGun ShotGun => shotGun;

	GunBase gunBase;
	public GunBase GetGunBase => gunBase;

	List<GunBase> gunBaseList = new List<GunBase>();

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public GunFacade()
	{
		gunBaseList.Add(handGun);
		gunBaseList.Add(assaultRifle);
		gunBaseList.Add(ShotGun);
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>ガンファサードセーブ</color>");
		foreach (var gun in gunBaseList)
		{
			gun.Save();
		}
		ES3.Save<GunTYPE>("CurrentGunType", gunTYPE);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=purple>ガンファサードロード</color>");
		foreach (var gun in gunBaseList)
		{
			gun.Load();
		}
		gunTYPE = ES3.Load<GunTYPE>("CurrentGunType", GunTYPE.HandGun);
	}

	/// <summary>
	/// 武器の切り替え初期化処理
	/// </summary> 
	public void SwitchWeapon()
	{
		foreach (var gun in gunBaseList)
		{
			if (gunTYPE == gun.GetGunType)
			{
				gunBase = gun;
			}
		}
	}

	/// <summary>
	/// 武器の切り替えトリガー
	/// </summary> 
	public void SwitchWeaponTrigger()
	{
		if (gunBase.IsReloadTimeActive == true)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1) || 0.5 < Input.GetAxisRaw("XInput DPad Left&Right"))
		{
			//Debug.Log("ハンドガン");
			gunTYPE = GunTYPE.HandGun;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			gunTYPE = GunTYPE.AssaultRifle;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			gunTYPE = GunTYPE.ShotGun;
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
		handGun.AcquireAmmo();
	}

	/// <summary>
	/// アサルトライフルの弾を取得
	/// </summary>
	public void AcquireAssaultRifleAmmo()
	{
		assaultRifle.AcquireAmmo();
	}

	/// <summary>
	/// ショットガンの弾を取得
	/// </summary>
	public void AcquireShotGunAmmo()
	{
		shotGun.AcquireAmmo();
	}
}

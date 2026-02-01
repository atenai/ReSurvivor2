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
		UnityEngine.Debug.Log("<color=cyan>ガンファサードセーブ</color>");
		foreach (var gun in gunBaseList)
		{
			gun.Save();
		}
		ES3.Save<GunTYPE>("CurrentGunType", this.gunTYPE);
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
		this.gunTYPE = ES3.Load<GunTYPE>("CurrentGunType", GunTYPE.HandGun);
	}

	/// <summary>
	/// 武器の切り替え初期化処理
	/// </summary> 
	public void SwitchWeapon()
	{
		foreach (var gun in gunBaseList)
		{
			if (this.gunTYPE == gun.GetGunType)
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
			this.gunTYPE = GunTYPE.HandGun;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetAxisRaw("XInput DPad Left&Right") < -0.5f)
		{
			//Debug.Log("アサルトライフル");
			this.gunTYPE = GunTYPE.AssaultRifle;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetAxisRaw("XInput DPad Up&Down") < -0.5f)
		{
			//Debug.Log("ショットガン");
			this.gunTYPE = GunTYPE.ShotGun;
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
	/// 弾を取得
	/// </summary>
	/// <param name="gunTYPE">銃のタイプ</param>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public void AcquireAmmo(GunTYPE gunTYPE, int amount = 10, UnityAction unityAction = null)
	{
		foreach (var gun in gunBaseList)
		{
			if (gunTYPE == gun.GetGunType)
			{
				gun.AcquireAmmo(amount, unityAction);
			}
		}
	}
}

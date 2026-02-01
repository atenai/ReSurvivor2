using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 銃のベース
/// </summary>
public abstract class GunBase
{
	[Header("銃のベース")]
	protected GunFacade.GunTYPE gunType;
	public GunFacade.GunTYPE GetGunType => gunType;

	[Tooltip("銃のダメージ")]
	[SerializeField] protected float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す力")]
	[SerializeField] protected float impactForce = 30.0f;
	[Tooltip("何秒間隔で撃つか")]
	[SerializeField] protected float fireRate = 0.1f;
	[Tooltip("射撃間隔時間用のカウントタイマー")]
	protected float fireCountTimer = 0.0f;
	[Tooltip("現在のマガジンの弾数")]
	protected int currentMagazine;
	public int CurrentMagazine => currentMagazine;
	[Tooltip("現在の残弾数")]
	protected int currentAmmo = 40;
	public int CurrentAmmo => currentAmmo;

	[Tooltip("リロードのオン・オフ")]
	protected bool isReloadTimeActive = false;
	public bool IsReloadTimeActive => isReloadTimeActive;

	[Tooltip("リロード時間用のカウントタイマー")]
	protected float reloadCountTimer = 0.0f;

	/// <summary>
	/// セーブ
	/// </summary>
	public abstract void Save();

	/// <summary>
	/// ロード
	/// </summary>
	public abstract void Load();

	/// <summary>
	/// 銃の更新
	/// </summary>
	public abstract void UpdateGun();

	/// <summary>
	/// 射撃
	/// </summary> 
	protected void Shoot()
	{
		if (Player.SingletonInstance.IsAim == false)
		{
			return;
		}

		if (currentMagazine == 0)
		{
			return;
		}

		if (isReloadTimeActive == true)
		{
			return;
		}

		if (fireCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
		{
			currentMagazine = currentMagazine - 1;//現在のマガジンの弾数を-1する
			Fire();
			fireCountTimer = fireRate;//カウントタイマーに射撃を待つ時間を入れる
		}
	}

	/// <summary>
	/// 射撃カウントタイマーリセット
	/// </summary>
	protected void ResetFireCountTimer()
	{
		//カウントタイマーが0以上なら中身を実行する
		if (0.0f < fireCountTimer)
		{
			//カウントタイマーを減らす
			fireCountTimer = fireCountTimer - Time.deltaTime;
		}
	}

	/// <summary>
	/// オートリロード
	/// </summary> 
	protected void AutoReloadTrigger()
	{
		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		//残弾数が0以下なら
		if (currentMagazine <= 0)
		{
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// 手動リロード
	/// </summary>
	/// <param name="magazineCapacity">銃の最大残弾数</param>
	protected void ManualReloadTrigger(int magazineCapacity)
	{
		//残弾数が満タンなら切り上げ
		if (currentMagazine == magazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		isReloadTimeActive = true;//リロードのオン
	}

	/// <summary>
	/// リロード
	/// </summary> 
	protected abstract void ReloadSystem();

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected abstract void Fire();

	/// <summary>
	/// 弾を取得
	/// </summary>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public abstract void AcquireAmmo(int amount = 10, UnityAction unityAction = null);
}

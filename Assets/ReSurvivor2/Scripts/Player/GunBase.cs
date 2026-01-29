using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 銃のベース
/// </summary>
public abstract class GunBase
{
	[Header("銃のベース")]
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
	/// 射撃
	/// </summary> 
	public abstract void Shoot();

	/// <summary>
	/// 射撃カウントタイマーリセット
	/// </summary>
	public void ResetFireCountTimer()
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
	public void AutoReloadTrigger()
	{
		//残弾数が0かつの弾薬が1発以上あるとき
		if (currentMagazine == 0 && 0 < currentAmmo)
		{
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// 手動リロード
	/// </summary> 
	public abstract void ManualReloadTrigger();

	/// <summary>
	/// リロード
	/// </summary> 
	public abstract void ReloadSystem();

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected abstract void Fire();

	// <summary>
	/// 弾を取得
	/// </summary> 
	public abstract void AcquireAmmo(int amount = 10);
}

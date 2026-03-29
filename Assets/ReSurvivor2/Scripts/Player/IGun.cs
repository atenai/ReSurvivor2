using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 銃のベース
/// </summary>
public interface IGun
{
	/// <summary>
	/// 銃の種類を取得
	/// </summary>
	EnumManager.GunTYPE GetGunType { get; }

	/// <summary>
	/// 現在のマガジンの弾数を取得
	/// </summary>
	/// <returns></returns>
	int CurrentMagazine { get; }

	/// <summary>
	/// 現在の残弾数を取得
	/// </summary>
	/// <returns></returns>
	int CurrentAmmo { get; }

	/// <summary>
	/// リロードのオン・オフ
	/// </summary>
	/// <returns></returns>
	bool IsReloadTimeActive { get; }

	/// <summary>
	/// セーブ
	/// </summary>
	void Save();

	/// <summary>
	/// ロード
	/// </summary>
	void Load();

	/// <summary>
	/// 一連の全ての処理
	/// </summary>
	void AllSystem();

	/// <summary>
	/// 射撃
	/// </summary> 
	void Shoot();

	/// <summary>
	/// 射撃カウントタイマーリセット
	/// </summary>
	void ResetFireCountTimer();

	/// <summary>
	/// オートリロード
	/// </summary> 
	void AutoReloadTrigger();

	/// <summary>
	/// 手動リロード
	/// </summary>
	/// <param name="magazineCapacity">銃の最大残弾数</param>
	void ManualReloadTrigger(int magazineCapacity);

	/// <summary>
	/// リロード
	/// </summary> 
	void ReloadSystem();

	/// <summary>
	/// 弾を発射
	/// </summary> 
	void Fire();

	/// <summary>
	/// 弾を取得
	/// </summary>
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	void AcquireAmmo(int amount = 10, UnityAction unityAction = null);
}

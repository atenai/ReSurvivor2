using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 銃のモデルベース
/// MVPパターンのModel担当
/// </summary>
public abstract class GunModelBase
{
	[Header("銃のモデルベース")]
	protected EnumManager.GunTYPE gunType;
	public EnumManager.GunTYPE GetGunType => gunType;

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

	[Tooltip("マガジンが空になってから経った時間（秒）（オートリロードを始めるまでの待ち時間を数える）")]
	protected float emptyMagazineTimer = 0.0f;

	/// <summary>
	/// リロードにかかる時間（秒）
	/// </summary>
	protected abstract float ReloadTime { get; }

	/// <summary>
	/// マガジンが空になってから、オートリロードを始めるまでの待ち時間（秒）
	/// （アサルトライフル・ショットガンは、この間に撃つボタンを押し直すと、ハンドガンに持ち替えて撃てる）
	/// </summary>
	protected abstract float AutoReloadWaitTime { get; }

	/// <summary>
	/// リロードの進み具合（0がリロードの始まり、1が終わり）
	/// </summary>
	public float ReloadProgress => Mathf.Clamp01(reloadCountTimer / ReloadTime);

	/// <summary>
	/// セーブ
	/// </summary>
	public abstract void Save();

	/// <summary>
	/// ロード
	/// </summary>
	public abstract void Load();

	/// <summary>
	/// 一連の全ての処理
	/// </summary>
	public abstract void AllSystem(bool isAim);

	/// <summary>
	/// 射撃
	/// </summary> 
	protected void Shoot(bool isAim)
	{
		if (isAim == false)
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
	/// オートリロード（マガジンが空になってから、待ち時間が経ったらリロードを始める）
	/// </summary>
	protected void AutoReloadTrigger()
	{
		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		//マガジンに弾が残っている間は、待ち時間を数えない
		if (0 < currentMagazine)
		{
			emptyMagazineTimer = 0.0f;
			return;
		}

		emptyMagazineTimer = emptyMagazineTimer + Time.deltaTime;
		if (AutoReloadWaitTime <= emptyMagazineTimer)
		{
			emptyMagazineTimer = 0.0f;
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// オートリロードの待ち時間を飛ばす（ハンドガンからメインの銃に持ち直した時は、すぐにリロードを始める）
	/// </summary>
	public void SkipAutoReloadWait()
	{
		emptyMagazineTimer = AutoReloadWaitTime;
	}

	/// <summary>
	/// 撃つボタンを押した瞬間か？（左クリック・Enter・コントローラーのRT）
	/// </summary>
	public static bool IsFireButtonDown()
	{
		return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.DownRT;
	}

	/// <summary>
	/// リロードボタンを押しているか？（Rキー・コントローラーのX）
	/// </summary>
	public static bool IsReloadButtonPressed()
	{
		return Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X");
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

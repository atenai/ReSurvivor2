using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// ハンドガン
/// </summary>
[Serializable]
public class HandGun : GunBase
{
	[Header("ハンドガン")]

	[Tooltip("ハンドガンの最大マガジン数")]
	readonly int handGunMagazineCapacity = 7;
	public int HandGunMagazineCapacity => handGunMagazineCapacity;//SE Pool用にいるかも

	[Tooltip("ハンドガンの初期残弾数")]
	readonly int initCurrentHandGunAmmoDefine = 35;

	[Tooltip("ハンドガンの最大残弾数")]
	readonly int maxHandGunAmmo = 70;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxHandGunAmmo => maxHandGunAmmo;//SE Pool用にいるかも

	[Tooltip("ハンドガンのリロード時間")]
	readonly float handGunReloadTimeDefine = 1.5f;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public HandGun()
	{
		this.gunType = GunFacade.GunTYPE.HandGun;
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public override void Save()
	{
		Debug.Log("<color=red>ハンドガンセーブ</color>");
		ES3.Save<int>("HandGunCurrentMagazine", currentMagazine);
		ES3.Save<int>("CurrentHandGunAmmo", currentAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public override void Load()
	{
		Debug.Log("<color=red>ハンドガンロード</color>");

		currentMagazine = ES3.Load<int>("HandGunCurrentMagazine", handGunMagazineCapacity);
		Debug.Log("<color=purple>ハンドガンマガジン : " + currentMagazine + "</color>");

		currentAmmo = ES3.Load<int>("CurrentHandGunAmmo", initCurrentHandGunAmmoDefine);
		Debug.Log("<color=purple>ハンドガン残弾数 : " + currentAmmo + "</color>");
	}

	/// <summary>
	/// 銃の更新
	/// </summary>
	public override void UpdateGun()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.Down)//左クリックまたはEnterを押している場合に中身を実行する
		{
			Shoot();
		}
		ResetFireCountTimer();
		AutoReloadTrigger();
		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			ManualReloadTrigger(handGunMagazineCapacity);
		}
		ReloadSystem();
	}

	/// <summary>
	/// リロード
	/// </summary> 
	protected override void ReloadSystem()
	{
		if (isReloadTimeActive == false)
		{
			return;
		}

		if (reloadCountTimer == 0)
		{
			//ハンドガンのリロードアニメーションをオン
			Player.SingletonInstance.Animator.SetBool("b_isHandGunReload", true);

			HandGunReloadSE();
		}

		//リロード中画像
		reloadCountTimer += Time.deltaTime;//リロードタイムをプラス

		if (handGunReloadTimeDefine <= reloadCountTimer)//リロードタイムが10以上になったら
		{
			//弾リセット
			int oldMagazine = currentMagazine;
			int localMagazine = handGunMagazineCapacity - currentMagazine;
			int localAmmo = currentAmmo - localMagazine;
			if (localAmmo < 0)
			{
				if (currentAmmo + oldMagazine < handGunMagazineCapacity)
				{
					currentMagazine = currentAmmo + oldMagazine;
					currentAmmo = 0;
				}
				else
				{
					currentMagazine = handGunMagazineCapacity;
					int totalAmmo = currentAmmo + oldMagazine;
					int resultAmmo = totalAmmo - handGunMagazineCapacity;
					currentAmmo = resultAmmo;
				}
			}
			else
			{
				currentMagazine = handGunMagazineCapacity;
				currentAmmo = localAmmo;
			}

			reloadCountTimer = 0.0f;//リロードタイムをリセット
			isReloadTimeActive = false;//リロードのオフ

			//ハンドガンのリロードアニメーションをオフ
			Player.SingletonInstance.Animator.SetBool("b_isHandGunReload", false);
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected override void Fire()
	{
		HandGunBulletCasingSE();
		Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunMuzzleFlashAndShell();
		Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunSmoke();

		HandGunFireSE();

		Ray ray = new Ray(PlayerCamera.SingletonInstance.transform.position, PlayerCamera.SingletonInstance.transform.forward);
		Debug.DrawRay(ray.origin, ray.direction * PlayerCamera.SingletonInstance.RaycastRange, Color.red, 10.0f);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, PlayerCamera.SingletonInstance.RaycastRange) == true) // もしRayを投射して何らかのコライダーに衝突したら
		{
#if UNITY_EDITOR//Unityエディター上での処理
			PlayerCamera.SingletonInstance.HitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得
#endif //終了  
			if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy") || hit.collider.gameObject.CompareTag("Mine"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
			{
				//ダメージ
				Target target = hit.transform.GetComponent<Target>();
				if (target != null)
				{
					target.TakeDamage(damage);
				}

				//着弾した物体を後ろに押す
				if (hit.rigidbody != null)
				{
					if (hit.collider.gameObject.CompareTag("FlyingEnemy") == false && hit.collider.gameObject.CompareTag("GroundEnemy") == false)
					{
						hit.rigidbody.AddForce(-hit.normal * impactForce);
					}
				}

				//ヒットレティクルを表示
				ScreenUI.SingletonInstance.IsHitReticule = true;

				//ヒット音を再生
				SoundManager.SingletonInstance.HitSEPool.GetGameObject(PlayerCamera.SingletonInstance.transform);

				// GroundEnemy groundEnemy = hit.transform.GetComponent<GroundEnemy>();
				// if (groundEnemy != null)
				// {
				// 	//追跡開始
				// 	groundEnemy.ChaseOn();
				// }

				//追跡開始
				EnemyManager.SingletonInstance.AllChaseOn();

				//地雷を爆破
				Mine mine = hit.transform.GetComponent<Mine>();
				if (mine != null)
				{
					mine.Explosion();
				}
			}

			EffectManager.SingletonInstance.ImpactEffect(hit);
		}
	}

	/// <summary>
	/// ハンドガンの射撃SE
	/// </summary> 
	void HandGunFireSE()
	{
		SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunMuzzleTransform);
	}

	/// <summary>
	/// ハンドガンのリロードSE
	/// </summary> 
	void HandGunReloadSE()
	{
		SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	void HandGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// 弾を取得
	/// </summary> 
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public override void AcquireAmmo(int amount = 10, UnityAction unityAction = null)
	{
		if (maxHandGunAmmo <= currentAmmo)
		{
			return;
		}

		currentAmmo = currentAmmo + amount;
		if (maxHandGunAmmo <= currentAmmo)
		{
			currentAmmo = maxHandGunAmmo;
		}

		unityAction?.Invoke();
	}
}

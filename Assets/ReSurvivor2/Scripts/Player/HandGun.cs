using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ハンドガン
/// </summary>
[Serializable]
public class HandGun : GunBase
{
	[Header("ハンドガン")]


	[Tooltip("ハンドガンの最大マガジン数")]
	readonly int handGunMagazineCapacity = 7;
	public int HandGunMagazineCapacity => handGunMagazineCapacity;

	[Tooltip("ハンドガンの初期残弾数")]
	readonly int initCurrentHandGunAmmoDefine = 35;

	[Tooltip("ハンドガンの最大残弾数")]
	readonly int maxHandGunAmmo = 70;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxHandGunAmmo => maxHandGunAmmo;


	[Tooltip("ハンドガンのリロード時間")]
	readonly float handGunReloadTimeDefine = 1.5f;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=red>ハンドガンセーブ</color>");
		ES3.Save<int>("HandGunCurrentMagazine", currentMagazine);
		ES3.Save<int>("CurrentHandGunAmmo", currentAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=red>ハンドガンロード</color>");

		currentMagazine = ES3.Load<int>("HandGunCurrentMagazine", handGunMagazineCapacity);
		Debug.Log("<color=purple>ハンドガンマガジン : " + currentMagazine + "</color>");

		currentAmmo = ES3.Load<int>("CurrentHandGunAmmo", initCurrentHandGunAmmoDefine);
		Debug.Log("<color=purple>ハンドガン残弾数 : " + currentAmmo + "</color>");
	}

	/// <summary>
	/// ハンドガンで射撃
	/// </summary> 
	public void Shoot()
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

		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.Down)//左クリックまたはEnterを押している場合に中身を実行する
		{
			if (fireCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
			{
				currentMagazine = currentMagazine - 1;//現在のマガジンの弾数を-1する
				HandGunFire();
				fireCountTimer = fireRate;//カウントタイマーに射撃を待つ時間を入れる
			}
		}
	}

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
	/// ハンドガンのオートリロード
	/// </summary> 
	public void AutoReloadTrigger()
	{
		//残弾数が0かつ弾薬が1発以上あるとき
		if (currentMagazine == 0 && 0 < currentAmmo)
		{
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ハンドガンの手動リロード
	/// </summary> 
	public void ManualReloadTrigger()
	{
		//残弾数が満タンなら切り上げ
		if (currentMagazine == handGunMagazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentAmmo <= 0)
		{
			return;
		}

		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			isReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ハンドガンのリロード
	/// </summary> 
	public void ReloadSystem()
	{
		if (isReloadTimeActive == true)//リロードがオンになったら
		{
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
	}

	/// <summary>
	/// ハンドガンの弾を発射
	/// </summary> 
	void HandGunFire()
	{
		HandGunBulletCasingSE();
		Player.SingletonInstance.HandGunMuzzleFlashAndShell();
		Player.SingletonInstance.HandGunSmoke();

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
				UI.SingletonInstance.IsHitReticule = true;

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
		SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(Player.SingletonInstance.HandGunMuzzleTransform);
	}

	/// <summary>
	/// ハンドガンのリロードSE
	/// </summary> 
	void HandGunReloadSE()
	{
		SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(Player.SingletonInstance.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	void HandGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(Player.SingletonInstance.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの弾を取得
	/// </summary> 
	public void AcquireHandGunAmmo(int amount = 10)
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
	}
}

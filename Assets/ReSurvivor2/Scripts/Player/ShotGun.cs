using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ショットガン
/// </summary>
[Serializable]
public class ShotGun : GunBase
{
	[Header("ショットガン")]

	[Tooltip("ショットガンの散乱角度")]
	[SerializeField] float shotGunRandomAngle = 5.0f;

	[Tooltip("ショットガンが一度で出る弾の数")]
	[SerializeField] int shotGunBullet = 10;

	[Tooltip("ショットガンの最大マガジン数")]
	readonly int shotGunMagazineCapacity = 8;
	public int ShotGunMagazineCapacity => shotGunMagazineCapacity;//SE Pool用にいるかも

	[Tooltip("ショットガンの初期残弾数")]
	readonly int initCurrentShotGunAmmoDefine = 40;

	[Tooltip("ショットガンの最大残弾数")]
	readonly int maxShotGunAmmo = 80;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxShotGunAmmo => maxShotGunAmmo;//SE Pool用にいるかも

	[Tooltip("ショットガンのリロード時間")]
	readonly float shotGunReloadTimeDefine = 1.5f;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=green>ショットガンセーブ</color>");
		ES3.Save<int>("ShotGunCurrentMagazine", currentMagazine);
		ES3.Save<int>("CurrentShotGunAmmo", currentAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=green>ショットガンロード</color>");

		currentMagazine = ES3.Load<int>("ShotGunCurrentMagazine", shotGunMagazineCapacity);
		Debug.Log("<color=purple>ショットガンマガジン : " + currentMagazine + "</color>");

		currentAmmo = ES3.Load<int>("CurrentShotGunAmmo", initCurrentShotGunAmmoDefine);
		Debug.Log("<color=purple>ショットガン残弾数 : " + currentAmmo + "</color>");
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
			ManualReloadTrigger(shotGunMagazineCapacity);
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
			//ショットガンのリロードアニメーションをオン
			Player.SingletonInstance.Animator.SetBool("b_isShotGunReload", true);

			ShotGunReloadSE();
		}

		//リロード中画像
		reloadCountTimer += Time.deltaTime;//リロードタイムをプラス

		if (shotGunReloadTimeDefine <= reloadCountTimer)//リロードタイムが10以上になったら
		{
			//弾リセット
			int oldMagazine = currentMagazine;
			int localMagazine = shotGunMagazineCapacity - currentMagazine;
			int localAmmo = currentAmmo - localMagazine;
			if (localAmmo < 0)
			{
				if (currentAmmo + oldMagazine < shotGunMagazineCapacity)
				{
					currentMagazine = currentAmmo + oldMagazine;
					currentAmmo = 0;
				}
				else
				{
					currentMagazine = shotGunMagazineCapacity;
					int totalAmmo = currentAmmo + oldMagazine;
					int resultAmmo = totalAmmo - shotGunMagazineCapacity;
					currentAmmo = resultAmmo;
				}
			}
			else
			{
				currentMagazine = shotGunMagazineCapacity;
				currentAmmo = localAmmo;
			}

			reloadCountTimer = 0.0f;//リロードタイムをリセット
			isReloadTimeActive = false;//リロードのオフ

			//ショットガンのリロードアニメーションをオフ
			Player.SingletonInstance.Animator.SetBool("b_isShotGunReload", false);
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected override void Fire()
	{
		ShotGunBulletCasingSE();
		Player.SingletonInstance.ShotGunMuzzleFlashAndShell();
		Player.SingletonInstance.ShotGunSmoke();

		ShotGunFireSE();
		bool isOnceShotGunHitSE = false;

		for (int i = 0; i < shotGunBullet; i++)
		{
			Vector3 direction = PlayerCamera.SingletonInstance.transform.forward;
			direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCamera.SingletonInstance.transform.up) * direction;
			direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCamera.SingletonInstance.transform.right) * direction;

			Ray ray = new Ray(PlayerCamera.SingletonInstance.transform.position, direction);
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

					if (isOnceShotGunHitSE == false)
					{
						isOnceShotGunHitSE = true;
						//ヒット音を再生
						SoundManager.SingletonInstance.HitSEPool.GetGameObject(PlayerCamera.SingletonInstance.transform);
					}

					// GroundEnemy groundEnemy = hit.transform.GetComponent<GroundEnemy>();
					// if (groundEnemy != null)
					// {
					// 	//追跡開始
					// 	groundEnemy.ChaseOn();
					// }

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

		//追跡開始
		EnemyManager.SingletonInstance.AllChaseOn();
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary> 
	void ShotGunFireSE()
	{
		SoundManager.SingletonInstance.ShotGunShootSEPool.GetGameObject(Player.SingletonInstance.ShotGunMuzzleTransform);
	}

	/// <summary>
	/// ショットガンのリロードSE
	/// </summary> 
	void ShotGunReloadSE()
	{
		SoundManager.SingletonInstance.ShotGunReloadSEPool.GetGameObject(Player.SingletonInstance.ShotGunBulletCasingTransform);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	void ShotGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.ShotGunBulletCasingSEPool.GetGameObject(Player.SingletonInstance.ShotGunBulletCasingTransform);
	}

	/// <summary>
	/// 弾を取得
	/// </summary> 
	public override void AcquireAmmo(int amount = 10)
	{
		if (maxShotGunAmmo <= currentAmmo)
		{
			return;
		}

		currentAmmo = currentAmmo + amount;
		if (maxShotGunAmmo <= currentAmmo)
		{
			currentAmmo = maxShotGunAmmo;
		}
	}
}

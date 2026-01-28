using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ショットガン
/// </summary>
public class ShotGun
{
	[Header("ベース")]
	[Tooltip("レイの長さ")]
	[SerializeField] float raycastRange = 100.0f;
	[Tooltip("銃のダメージ")]
	[SerializeField] float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す")]
	[SerializeField] float impactForce = 30.0f;

	[Header("ショットガン")]
	[Tooltip("ショットガンを何秒間隔で撃つか")]
	[SerializeField] float shotGunFireRate = 0.1f;
	[Tooltip("ショットガンの射撃間隔の時間カウント用のタイマー")]
	float shotGunCountTimer = 0.0f;
	[Tooltip("ショットガンの散乱角度")]
	[SerializeField] float shotGunRandomAngle = 5.0f;
	[Tooltip("ショットガンが一度で出る弾の数")]
	[SerializeField] int shotGunBullet = 10;
	[Tooltip("ショットガンの現在のマガジンの弾数")]
	int shotGunCurrentMagazine;
	public int ShotGunCurrentMagazine => shotGunCurrentMagazine;
	[Tooltip("ショットガンの最大マガジン数")]
	readonly int shotGunMagazineCapacity = 8;
	public int ShotGunMagazineCapacity => shotGunMagazineCapacity;
	[Tooltip("ショットガンの現在の残弾数")]
	int currentShotGunAmmo = 40;
	public int CurrentShotGunAmmo => currentShotGunAmmo;
	[Tooltip("ショットガンの最大残弾数")]
	readonly int maxShotGunAmmo = 80;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxShotGunAmmo => maxShotGunAmmo;
	[Tooltip("ショットガンのリロードのオン・オフ")]
	bool isShotGunReloadTimeActive = false;
	public bool IsShotGunReloadTimeActive => isShotGunReloadTimeActive;
	float shotGunReloadTime = 0.0f;
	[Tooltip("ショットガンのリロード時間")]
	readonly float shotGunReloadTimeDefine = 1.5f;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=green>ショットガンセーブ</color>");
		ES3.Save<int>("ShotGunCurrentMagazine", shotGunCurrentMagazine);
		ES3.Save<int>("CurrentShotGunAmmo", currentShotGunAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=green>ショットガンロード</color>");
		shotGunCurrentMagazine = ES3.Load<int>("ShotGunCurrentMagazine", shotGunMagazineCapacity);
		Debug.Log("<color=purple>ショットガンマガジン : " + shotGunCurrentMagazine + "</color>");
		currentShotGunAmmo = ES3.Load<int>("CurrentShotGunAmmo", maxShotGunAmmo);
		Debug.Log("<color=purple>ショットガン残弾数 : " + currentShotGunAmmo + "</color>");
	}

	/// <summary>
	/// ショットガンで射撃
	/// </summary> 
	public void ShotGunShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.Down)//左クリックまたはEnterを押している場合に中身を実行する
			{
				if (shotGunCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
				{
					if (shotGunCurrentMagazine != 0)
					{
						if (isShotGunReloadTimeActive == false)
						{
							shotGunCurrentMagazine = shotGunCurrentMagazine - 1;//現在のマガジンの弾数を-1する
							ShotGunFire();
							shotGunCountTimer = shotGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
						}
					}
				}
			}
		}

		//カウントタイマーが0以上なら中身を実行する
		if (0.0f < shotGunCountTimer)
		{
			//カウントタイマーを減らす
			shotGunCountTimer = shotGunCountTimer - Time.deltaTime;
		}
	}

	/// <summary>
	/// ショットガンのオートリロード
	/// </summary> 
	public void ShotGunAutoReload()
	{
		//残弾数が0かつの弾薬が1発以上あるとき
		if (shotGunCurrentMagazine == 0 && 0 < currentShotGunAmmo)
		{
			isShotGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ショットガンの手動リロード
	/// </summary> 
	public void ShotGunManualReload()
	{
		//残弾数が満タンなら切り上げ
		if (shotGunCurrentMagazine == shotGunMagazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentShotGunAmmo <= 0)
		{
			return;
		}

		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			isShotGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ショットガンのリロード
	/// </summary> 
	public void ShotGunReload()
	{
		if (isShotGunReloadTimeActive == true)//リロードがオンになったら
		{
			if (shotGunReloadTime == 0)
			{
				//ショットガンのリロードアニメーションをオン
				Player.SingletonInstance.Animator.SetBool("b_isShotGunReload", true);

				ShotGunReloadSE();
			}

			//リロード中画像
			shotGunReloadTime += Time.deltaTime;//リロードタイムをプラス

			if (shotGunReloadTimeDefine <= shotGunReloadTime)//リロードタイムが10以上になったら
			{
				//弾リセット
				int oldMagazine = shotGunCurrentMagazine;
				int localMagazine = shotGunMagazineCapacity - shotGunCurrentMagazine;
				int localAmmo = currentShotGunAmmo - localMagazine;
				if (localAmmo < 0)
				{
					if (currentShotGunAmmo + oldMagazine < shotGunMagazineCapacity)
					{
						shotGunCurrentMagazine = currentShotGunAmmo + oldMagazine;
						currentShotGunAmmo = 0;
					}
					else
					{
						shotGunCurrentMagazine = shotGunMagazineCapacity;
						int totalAmmo = currentShotGunAmmo + oldMagazine;
						int resultAmmo = totalAmmo - shotGunMagazineCapacity;
						currentShotGunAmmo = resultAmmo;
					}
				}
				else
				{
					shotGunCurrentMagazine = shotGunMagazineCapacity;
					currentShotGunAmmo = localAmmo;
				}

				shotGunReloadTime = 0.0f;//リロードタイムをリセット
				isShotGunReloadTimeActive = false;//リロードのオフ
												  //ショットガンのリロードアニメーションをオフ
				Player.SingletonInstance.Animator.SetBool("b_isShotGunReload", false);
			}
		}
	}

	/// <summary>
	/// ショットガンの弾を発射
	/// </summary> 
	void ShotGunFire()
	{
		ShotGunBulletCasingSE();
		Player.SingletonInstance.ShotGunMuzzleFlashAndShell();
		Player.SingletonInstance.ShotGunSmoke();

		ShotGunFireSE();
		bool isOnceShotGunHitSE = false;

		for (int i = 0; i < shotGunBullet; i++)
		{
			Vector3 direction = PlayerCamera.SingletonInstance.transform.forward;
			direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCamera.SingletonInstance.transform.up) * direction;
			direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), PlayerCamera.SingletonInstance.transform.right) * direction;

			Ray ray = new Ray(PlayerCamera.SingletonInstance.transform.position, direction);
			Debug.DrawRay(ray.origin, ray.direction * raycastRange, Color.red, 10.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, raycastRange) == true) // もしRayを投射して何らかのコライダーに衝突したら
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
					PlayerCamera.SingletonInstance.IsHitReticule = true;

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

				PlayerCamera.SingletonInstance.ImpactEffect(hit);
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
	/// ショットガンの弾を取得
	/// </summary> 
	public void AcquireShotGunAmmo()
	{
		if (maxShotGunAmmo <= currentShotGunAmmo)
		{
			return;
		}

		currentShotGunAmmo = currentShotGunAmmo + 10;
		if (maxShotGunAmmo <= currentShotGunAmmo)
		{
			currentShotGunAmmo = maxShotGunAmmo;
		}
	}
}

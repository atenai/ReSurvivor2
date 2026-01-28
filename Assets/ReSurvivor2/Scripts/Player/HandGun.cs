using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガン
/// </summary>
public class HandGun
{
	[Header("ベース")]
	[Tooltip("レイの長さ")]
	[SerializeField] float raycastRange = 100.0f;
	[Tooltip("銃のダメージ")]
	[SerializeField] float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す")]
	[SerializeField] float impactForce = 30.0f;

	[Header("ハンドガン")]
	[Tooltip("ハンドガンを何秒間隔で撃つか")]
	[SerializeField] float handGunFireRate = 0.1f;
	[Tooltip("ハンドガンの射撃間隔の時間カウント用のタイマー")]
	float handGunCountTimer = 0.0f;
	[Tooltip("ハンドガンの現在のマガジンの弾数")]
	int handGunCurrentMagazine;
	public int HandGunCurrentMagazine => handGunCurrentMagazine;
	[Tooltip("ハンドガンの最大マガジン数")]
	readonly int handGunMagazineCapacity = 7;
	public int HandGunMagazineCapacity => handGunMagazineCapacity;
	[Tooltip("ハンドガンの現在の残弾数")]
	int currentHandGunAmmo = 35;
	public int CurrentHandGunAmmo => currentHandGunAmmo;
	[Tooltip("ハンドガンの最大残弾数")]
	readonly int maxHandGunAmmo = 70;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxHandGunAmmo => maxHandGunAmmo;
	[Tooltip("ハンドガンのリロードのオン・オフ")]
	bool isHandGunReloadTimeActive = false;
	public bool IsHandGunReloadTimeActive => isHandGunReloadTimeActive;
	float handGunReloadTime = 0.0f;
	[Tooltip("ハンドガンのリロード時間")]
	readonly float handGunReloadTimeDefine = 1.5f;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>ハンドガンセーブ</color>");
		ES3.Save<int>("HandGunCurrentMagazine", handGunCurrentMagazine);
		ES3.Save<int>("CurrentHandGunAmmo", currentHandGunAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=purple>プレイヤーカメラロード</color>");
		handGunCurrentMagazine = ES3.Load<int>("HandGunCurrentMagazine", handGunMagazineCapacity);
		Debug.Log("<color=purple>ハンドガンマガジン : " + handGunCurrentMagazine + "</color>");
		currentHandGunAmmo = ES3.Load<int>("CurrentHandGunAmmo", maxHandGunAmmo);
		Debug.Log("<color=purple>ハンドガン残弾数 : " + currentHandGunAmmo + "</color>");
	}

	/// <summary>
	/// ハンドガンで射撃
	/// </summary> 
	public void HandGunShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.Down)//左クリックまたはEnterを押している場合に中身を実行する
			{
				if (handGunCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
				{
					if (handGunCurrentMagazine != 0)
					{
						if (isHandGunReloadTimeActive == false)
						{
							handGunCurrentMagazine = handGunCurrentMagazine - 1;//現在のマガジンの弾数を-1する
							HandGunFire();
							handGunCountTimer = handGunFireRate;//カウントタイマーに射撃を待つ時間を入れる
						}
					}
				}
			}
		}

		//カウントタイマーが0以上なら中身を実行する
		if (0.0f < handGunCountTimer)
		{
			//カウントタイマーを減らす
			handGunCountTimer = handGunCountTimer - Time.deltaTime;
		}
	}

	/// <summary>
	/// ハンドガンのオートリロード
	/// </summary> 
	public void HandGunAutoReload()
	{
		//残弾数が0かつ弾薬が1発以上あるとき
		if (handGunCurrentMagazine == 0 && 0 < currentHandGunAmmo)
		{
			isHandGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ハンドガンの手動リロード
	/// </summary> 
	public void HandGunManualReload()
	{
		//残弾数が満タンなら切り上げ
		if (handGunCurrentMagazine == handGunMagazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentHandGunAmmo <= 0)
		{
			return;
		}

		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			isHandGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ハンドガンのリロード
	/// </summary> 
	public void HandGunReload()
	{
		if (isHandGunReloadTimeActive == true)//リロードがオンになったら
		{
			if (handGunReloadTime == 0)
			{
				//ハンドガンのリロードアニメーションをオン
				Player.SingletonInstance.Animator.SetBool("b_isHandGunReload", true);

				HandGunReloadSE();
			}

			//リロード中画像
			handGunReloadTime += Time.deltaTime;//リロードタイムをプラス

			if (handGunReloadTimeDefine <= handGunReloadTime)//リロードタイムが10以上になったら
			{
				//弾リセット
				int oldMagazine = handGunCurrentMagazine;
				int localMagazine = handGunMagazineCapacity - handGunCurrentMagazine;
				int localAmmo = currentHandGunAmmo - localMagazine;
				if (localAmmo < 0)
				{
					if (currentHandGunAmmo + oldMagazine < handGunMagazineCapacity)
					{
						handGunCurrentMagazine = currentHandGunAmmo + oldMagazine;
						currentHandGunAmmo = 0;
					}
					else
					{
						handGunCurrentMagazine = handGunMagazineCapacity;
						int totalAmmo = currentHandGunAmmo + oldMagazine;
						int resultAmmo = totalAmmo - handGunMagazineCapacity;
						currentHandGunAmmo = resultAmmo;
					}
				}
				else
				{
					handGunCurrentMagazine = handGunMagazineCapacity;
					currentHandGunAmmo = localAmmo;
				}

				handGunReloadTime = 0.0f;//リロードタイムをリセット
				isHandGunReloadTimeActive = false;//リロードのオフ
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

			PlayerCamera.SingletonInstance.ImpactEffect(hit);
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
	public void AcquireHandGunAmmo()
	{
		if (maxHandGunAmmo <= currentHandGunAmmo)
		{
			return;
		}

		currentHandGunAmmo = currentHandGunAmmo + 10;
		if (maxHandGunAmmo <= currentHandGunAmmo)
		{
			currentHandGunAmmo = maxHandGunAmmo;
		}
	}
}

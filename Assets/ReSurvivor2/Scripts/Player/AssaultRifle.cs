using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフル
/// </summary>
public class AssaultRifle
{
	[Header("ベース")]
	[Tooltip("銃のダメージ")]
	[SerializeField] float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す")]
	[SerializeField] float impactForce = 30.0f;

	[Header("アサルトライフル")]
	[Tooltip("アサルトライフルを何秒間隔で撃つか")]
	[SerializeField] float assaultRifleFireRate = 0.1f;
	[Tooltip("アサルトライフルの射撃間隔の時間カウント用のタイマー")]
	float assaultRifleCountTimer = 0.0f;
	[Tooltip("アサルトライフルの散乱角度")]
	[SerializeField] float assaultRifleRandomAngle = 1.0f;
	[Tooltip("アサルトライフルの現在のマガジンの弾数")]
	int assaultRifleCurrentMagazine;
	public int AssaultRifleCurrentMagazine => assaultRifleCurrentMagazine;
	[Tooltip("アサルトライフルの最大マガジン数")]
	readonly int assaultRifleMagazineCapacity = 30;
	public int AssaultRifleMagazineCapacity => assaultRifleMagazineCapacity;
	[Tooltip("アサルトライフルの現在の残弾数")]
	int currentAssaultRifleAmmo = 150;
	public int CurrentAssaultRifleAmmo => currentAssaultRifleAmmo;
	[Tooltip("アサルトライフルの最大残弾数")]
	readonly int maxAssaultRifleAmmo = 300;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも
	public int MaxAssaultRifleAmmo => maxAssaultRifleAmmo;
	[Tooltip("アサルトライフルのリロードのオン・オフ")]
	bool isAssaultRifleReloadTimeActive = false;
	public bool IsAssaultRifleReloadTimeActive => isAssaultRifleReloadTimeActive;
	float assaultRifleReloadTime = 0.0f;
	[Tooltip("アサルトライフルのリロード時間")]
	readonly float assaultRifleReloadTimeDefine = 1.5f;

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=blue>アサルトライフルセーブ</color>");
		ES3.Save<int>("AssaultRifleCurrentMagazine", assaultRifleCurrentMagazine);
		ES3.Save<int>("CurrentAssaultRifleAmmo", currentAssaultRifleAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public void Load()
	{
		Debug.Log("<color=blue>アサルトライフルロード</color>");
		assaultRifleCurrentMagazine = ES3.Load<int>("AssaultRifleCurrentMagazine", assaultRifleMagazineCapacity);
		Debug.Log("<color=purple>アサルトライフルマガジン : " + assaultRifleCurrentMagazine + "</color>");
		currentAssaultRifleAmmo = ES3.Load<int>("CurrentAssaultRifleAmmo", maxAssaultRifleAmmo);
		Debug.Log("<color=purple>アサルトライフル残弾数 : " + currentAssaultRifleAmmo + "</color>");
	}

	/// <summary>
	/// アサルトライフルで射撃
	/// </summary> 
	public void AssaultRifleShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return) || 0.5f < Input.GetAxisRaw("XInput RT"))//左クリックまたはEnterを押している場合に中身を実行する
			{
				if (assaultRifleCountTimer <= 0.0f)//カウントタイマーが0以下の場合は中身を実行する
				{
					if (assaultRifleCurrentMagazine != 0)
					{
						if (isAssaultRifleReloadTimeActive == false)
						{
							assaultRifleCurrentMagazine = assaultRifleCurrentMagazine - 1;//現在のマガジンの弾数を-1する
							AssaultRifleFire();
							assaultRifleCountTimer = assaultRifleFireRate;//カウントタイマーに射撃を待つ時間を入れる
						}
					}
				}
			}
		}

		//カウントタイマーが0以上なら中身を実行する
		if (0.0f < assaultRifleCountTimer)
		{
			//カウントタイマーを減らす
			assaultRifleCountTimer = assaultRifleCountTimer - Time.deltaTime;
		}
	}

	/// <summary>
	/// アサルトライフルのオートリロード
	/// </summary> 
	public void AssaultRifleAutoReload()
	{
		//残弾数が0かつの弾薬が1発以上あるとき
		if (assaultRifleCurrentMagazine == 0 && 0 < currentAssaultRifleAmmo)
		{
			isAssaultRifleReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// アサルトライフルの手動リロード
	/// </summary> 
	public void AssaultRifleManualReload()
	{
		//残弾数が満タンなら切り上げ
		if (assaultRifleCurrentMagazine == assaultRifleMagazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentAssaultRifleAmmo <= 0)
		{
			return;
		}

		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			isAssaultRifleReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// アサルトライフルのリロード
	/// </summary> 
	public void AssaultRifleReload()
	{
		if (isAssaultRifleReloadTimeActive == true)//リロードがオンになったら
		{
			if (assaultRifleReloadTime == 0)
			{
				//アサルトライフルのリロードアニメーションをオン
				Player.SingletonInstance.Animator.SetBool("b_isAssaultRifleReload", true);

				AssaultRifleReloadSE();
			}

			//リロード中画像
			assaultRifleReloadTime += Time.deltaTime;//リロードタイムをプラス

			if (assaultRifleReloadTimeDefine <= assaultRifleReloadTime)//リロードタイムが10以上になったら
			{
				//弾リセット
				int oldMagazine = assaultRifleCurrentMagazine;
				int localMagazine = assaultRifleMagazineCapacity - assaultRifleCurrentMagazine;
				int localAmmo = currentAssaultRifleAmmo - localMagazine;
				if (localAmmo < 0)
				{
					if (currentAssaultRifleAmmo + oldMagazine < assaultRifleMagazineCapacity)
					{
						assaultRifleCurrentMagazine = currentAssaultRifleAmmo + oldMagazine;
						currentAssaultRifleAmmo = 0;
					}
					else
					{
						assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
						int totalAmmo = currentAssaultRifleAmmo + oldMagazine;
						int resultAmmo = totalAmmo - assaultRifleMagazineCapacity;
						currentAssaultRifleAmmo = resultAmmo;
					}
				}
				else
				{
					assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
					currentAssaultRifleAmmo = localAmmo;
				}

				assaultRifleReloadTime = 0.0f;//リロードタイムをリセット
				isAssaultRifleReloadTimeActive = false;//リロードのオフ
													   //アサルトライフルのリロードアニメーションをオフ
				Player.SingletonInstance.Animator.SetBool("b_isAssaultRifleReload", false);
			}
		}
	}

	/// <summary>
	/// アサルトライフルの弾を発射
	/// </summary> 
	void AssaultRifleFire()
	{
		AssaultRifleBulletCasingSE();
		Player.SingletonInstance.AssaultRifleMuzzleFlashAndShell();
		Player.SingletonInstance.AssaultRifleSmoke();

		AssaultRifleFireSE();

		Vector3 direction = PlayerCamera.SingletonInstance.transform.forward;
		direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), PlayerCamera.SingletonInstance.transform.up) * direction;
		direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), PlayerCamera.SingletonInstance.transform.right) * direction;

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
				PlayerCamera.SingletonInstance.IsHitReticule = true;

				//ヒット音を再生
				SoundManager.SingletonInstance.HitSEPool.GetGameObject(PlayerCamera.SingletonInstance.transform);

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

		//追跡開始
		EnemyManager.SingletonInstance.AllChaseOn();
	}

	/// <summary>
	/// アサルトライフルの射撃SE
	/// </summary> 
	void AssaultRifleFireSE()
	{
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.GetGameObject(Player.SingletonInstance.AssaultRifleMuzzleTransform);
	}

	/// <summary>
	/// アサルトライフルのリロードSE
	/// </summary> 
	void AssaultRifleReloadSE()
	{
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.GetGameObject(Player.SingletonInstance.AssaultRifleBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	void AssaultRifleBulletCasingSE()
	{
		SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.GetGameObject(Player.SingletonInstance.AssaultRifleBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの弾を取得
	/// </summary> 
	public void AcquireAssaultRifleAmmo()
	{
		if (maxAssaultRifleAmmo <= currentAssaultRifleAmmo)
		{
			return;
		}

		currentAssaultRifleAmmo = currentAssaultRifleAmmo + 10;
		if (maxAssaultRifleAmmo <= currentAssaultRifleAmmo)
		{
			currentAssaultRifleAmmo = maxAssaultRifleAmmo;
		}
	}
}

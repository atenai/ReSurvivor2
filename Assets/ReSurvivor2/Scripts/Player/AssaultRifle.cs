using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// アサルトライフル
/// </summary>
[Serializable]
public class AssaultRifle : GunBase
{
	[Header("アサルトライフル")]

	[Tooltip("アサルトライフルの散乱角度")]
	[SerializeField] float assaultRifleRandomAngle = 1.0f;

	[Tooltip("アサルトライフルの最大マガジン数")]
	readonly int assaultRifleMagazineCapacity = 30;

	[Tooltip("アサルトライフルの初期残弾数")]
	readonly int initCurrentAssaultRifleAmmoDefine = 150;

	[Tooltip("アサルトライフルの最大残弾数")]
	readonly int maxAssaultRifleAmmo = 300;//将来的には拡張マガジンポーチを取得すると増える的なものを入れるかも

	[Tooltip("アサルトライフルのリロード時間")]
	readonly float assaultRifleReloadTimeDefine = 1.5f;

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public AssaultRifle()
	{
		this.gunType = EnumManager.GunTYPE.AssaultRifle;
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public override void Save()
	{
		Debug.Log("<color=blue>アサルトライフルセーブ</color>");
		ES3.Save<int>("AssaultRifleCurrentMagazine", currentMagazine);
		ES3.Save<int>("CurrentAssaultRifleAmmo", currentAmmo);
	}

	/// <summary>
	/// ロード
	/// </summary>
	public override void Load()
	{
		Debug.Log("<color=blue>アサルトライフルロード</color>");

		currentMagazine = ES3.Load<int>("AssaultRifleCurrentMagazine", assaultRifleMagazineCapacity);
		Debug.Log("<color=purple>アサルトライフルマガジン : " + currentMagazine + "</color>");

		currentAmmo = ES3.Load<int>("CurrentAssaultRifleAmmo", initCurrentAssaultRifleAmmoDefine);
		Debug.Log("<color=purple>アサルトライフル残弾数 : " + currentAmmo + "</color>");
	}

	/// <summary>
	/// 一連の全ての処理
	/// </summary>
	public override void AllSystem()
	{
		if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return) || XInputManager.SingletonInstance.XInputTriggerHandler.IsPressedRT)//左クリックまたはEnterを押している場合に中身を実行する
		{
			Shoot();
		}
		ResetFireCountTimer();
		AutoReloadTrigger();
		if (Input.GetKey(KeyCode.R) || Input.GetButtonDown("XInput X"))
		{
			ManualReloadTrigger(assaultRifleMagazineCapacity);
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
			//アサルトライフルのリロードアニメーションをオン
			Player.SingletonInstance.Animator.SetBool("b_isAssaultRifleReload", true);

			AssaultRifleReloadSE();
		}

		//リロード中画像
		reloadCountTimer += Time.deltaTime;//リロードタイムをプラス

		if (assaultRifleReloadTimeDefine <= reloadCountTimer)//リロードタイムが10以上になったら
		{
			//弾リセット
			int oldMagazine = currentMagazine;
			int localMagazine = assaultRifleMagazineCapacity - currentMagazine;
			int localAmmo = currentAmmo - localMagazine;
			if (localAmmo < 0)
			{
				if (currentAmmo + oldMagazine < assaultRifleMagazineCapacity)
				{
					currentMagazine = currentAmmo + oldMagazine;
					currentAmmo = 0;
				}
				else
				{
					currentMagazine = assaultRifleMagazineCapacity;
					int totalAmmo = currentAmmo + oldMagazine;
					int resultAmmo = totalAmmo - assaultRifleMagazineCapacity;
					currentAmmo = resultAmmo;
				}
			}
			else
			{
				currentMagazine = assaultRifleMagazineCapacity;
				currentAmmo = localAmmo;
			}

			reloadCountTimer = 0.0f;//リロードタイムをリセット
			isReloadTimeActive = false;//リロードのオフ

			//アサルトライフルのリロードアニメーションをオフ
			Player.SingletonInstance.Animator.SetBool("b_isAssaultRifleReload", false);
		}
	}

	/// <summary>
	/// 弾を発射
	/// </summary> 
	protected override void Fire()
	{
		AssaultRifleBulletCasingSE();
		Player.SingletonInstance.GunModelFacade.AssaultRifleModel.AssaultRifleMuzzleFlashAndShell();
		Player.SingletonInstance.GunModelFacade.AssaultRifleModel.AssaultRifleSmoke();

		AssaultRifleFireSE();

		Vector3 direction = PlayerCamera.SingletonInstance.transform.forward;
		direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), PlayerCamera.SingletonInstance.transform.up) * direction;
		direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), PlayerCamera.SingletonInstance.transform.right) * direction;

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
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.AssaultRifleModel.AssaultRifleMuzzleTransform);
	}

	/// <summary>
	/// アサルトライフルのリロードSE
	/// </summary> 
	void AssaultRifleReloadSE()
	{
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.AssaultRifleModel.AssaultRifleBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	void AssaultRifleBulletCasingSE()
	{
		SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.AssaultRifleModel.AssaultRifleBulletCasingTransform);
	}

	/// <summary>
	/// 弾を取得
	/// </summary> 
	/// <param name="amount">追加する弾数</param>
	/// <param name="unityAction">イベント</param>
	public override void AcquireAmmo(int amount = 10, UnityAction unityAction = null)
	{
		if (maxAssaultRifleAmmo <= currentAmmo)
		{
			return;
		}

		currentAmmo = currentAmmo + amount;
		if (maxAssaultRifleAmmo <= currentAmmo)
		{
			currentAmmo = maxAssaultRifleAmmo;
		}

		unityAction?.Invoke();
	}
}

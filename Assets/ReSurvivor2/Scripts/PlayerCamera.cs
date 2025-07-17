using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;
using Cinemachine;
using System.Data;

/// <summary>
/// プレイヤーカメラ
/// </summary> 
public class PlayerCamera : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static PlayerCamera singletonInstance = null;
	public static PlayerCamera SingletonInstance => singletonInstance;

	[Header("カメラ")]

	[Tooltip("X軸のカメラの回転スピード")]
	[Range(50, 150)][SerializeField] float normalCameraSpeedX = 100;
	[Tooltip("Y軸のカメラの回転スピード")]
	[Range(25, 125)][SerializeField] float normalCameraSpeedY = 50;

	[Tooltip("X軸のカメラの回転スピード")]
	[Range(50, 150)][SerializeField] float aimCameraSpeedX = 50;
	[Tooltip("Y軸のカメラの回転スピード")]
	[Range(25, 125)][SerializeField] float aimCameraSpeedY = 25;

	[Tooltip("X軸のカメラの入力デッドゾーン")]
	[Range(0.001f, 0.1f)][SerializeField] float deadZoneX = 0.1f;
	[Tooltip("Y軸のカメラの入力デッドゾーン")]
	[Range(0.001f, 0.1f)][SerializeField] float deadZoneY = 0.1f;

	[Tooltip("レティクルの中心点（レイキャスト）にターゲットがヒットしてるか？")]
	bool isTargethit = false;
	public bool IsTargethit => isTargethit;
	[Tooltip("ローカルで計算する為のX軸のカメラの回転スピード")]
	float localCameraSpeedX;
	[Tooltip("ローカルで計算する為のY軸のカメラの回転スピード")]
	float localCameraSpeedY;
	[Tooltip("カメラのスピードを遅くする")]
	[Range(1.0f, 4.0f)] float slowDownCameraSpeed = 2.0f;

	[Tooltip("通常カメラのy位置")]
	const float normalUpPos = 1.6f;
	public float NormalUpPos => normalUpPos;
	[Tooltip("通常カメラのz位置")]
	const float normalForwardPos = -4.0f;

	[Tooltip("肩越しカメラのx位置")]
	const float aimRightPos = 0.5f;
	[Tooltip("肩越しカメラのy位置")]
	const float aimUpPos = 1.6f;
	public float AimUpPos => aimUpPos;
	[Tooltip("肩越しカメラのz位置")]
	const float aimForwardPos = -0.6f;

	[Tooltip("シネマシーンカメラをアクティブにするか？")]
	bool isCinemachineActive = false;
	public bool IsCinemachineActive
	{
		get { return isCinemachineActive; }
		set { isCinemachineActive = value; }
	}

	[Header("レイキャスト")]
	[Tooltip("ヒットしたオブジェクトの名前")]
	string hitName = "";
	[Tooltip("レイの長さ")]
	[SerializeField] float range = 100.0f;
	[Tooltip("銃のダメージ")]
	[SerializeField] float damage = 10.0f;
	[Tooltip("着弾した物体を後ろに押す")]
	[SerializeField] float impactForce = 30.0f;
	[Tooltip("ヒットレティクル")]
	bool isHitReticule = false;
	[Tooltip("ヒットレティクルが消失するスピード")]
	float hitReticuleSpeed = 10.0f;

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

	[Tooltip("ハンドガンのリロードのSE")]
	[SerializeField] GameObject handGunReloadSe;

	[Tooltip("ハンドガンの薬莢のSE")]
	[SerializeField] GameObject handGunBulletCasingSe;

	//↓アセットストアのプログラム↓//
	[Tooltip("ハンドガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] handGunShotEmitters;
	[Tooltip("ハンドガンの硝煙")]
	[SerializeField] ParticleGroupPlayer handGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

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

	[Tooltip("アサルトライフルの射撃のSE")]
	[SerializeField] GameObject assaultRifleShootSe;
	[Tooltip("アサルトライフルのリロードのSE")]
	[SerializeField] GameObject assaultRifleReloadSe;
	[Tooltip("アサルトライフルの薬莢のSE")]
	[SerializeField] GameObject assaultRifleBulletCasingSe;

	//↓アセットストアのプログラム↓//
	[Tooltip("アサルトライフルのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] assaultRifleShotEmitters;
	[Tooltip("アサルトライフルの硝煙")]
	[SerializeField] ParticleGroupPlayer assaultRifleAfterFireSmoke;
	//↑アセットストアのプログラム↑//

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

	[Tooltip("ショットガンの射撃のSE")]
	[SerializeField] GameObject shotGunShootSe;
	[Tooltip("ショットガンのリロードのSE")]
	[SerializeField] GameObject shotGunReloadSe;
	[Tooltip("ショットガンの薬莢のSE")]
	[SerializeField] GameObject shotGunBulletCasingSe;

	//↓アセットストアのプログラム↓//
	[Tooltip("ショットガンのマズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotGunShotEmitters;
	[Tooltip("ショットガンの硝煙")]
	[SerializeField] ParticleGroupPlayer shotGunAfterFireSmoke;
	//↑アセットストアのプログラム↑//

	[Header("着弾エフェクト")]
	//パス(Assets/Knife/PRO Effects FPS Muzzle flashes & Impacts/Particles/Prefabs/Impacts)
	[Tooltip("血の着弾エフェクト")]
	[SerializeField] GameObject bloodImpactEffect;
	[Tooltip("煙の着弾エフェクト")]
	[SerializeField] GameObject rockImpactEffect;

	public enum GunTYPE
	{
		HandGun = 1,
		AssaultRifle = 2,
		ShotGun = 3,
	}
	public GunTYPE gunTYPE = GunTYPE.AssaultRifle;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、PlayerCameraのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		//各種初期化処理
		InitHandGunMagazine();
		InitAssaultRifleMagazine();
		InitShotGunMagazine();
	}

	/// <summary>
	/// ハンドガンの弾数の初期化処理
	/// </summary>
	void InitHandGunMagazine()
	{
		//ハンドガンの残弾数が満タンなら切り上げ
		if (handGunCurrentMagazine == handGunMagazineCapacity)
		{
			return;
		}

		//弾が0以下なら切り上げ
		if (currentHandGunAmmo <= 0)
		{
			return;
		}

		int localMagazine = handGunMagazineCapacity - handGunCurrentMagazine;
		int localAmmo = currentHandGunAmmo - localMagazine;
		if (localAmmo < 0)
		{
			handGunCurrentMagazine = currentHandGunAmmo;
			currentHandGunAmmo = 0;
		}
		else
		{
			handGunCurrentMagazine = handGunMagazineCapacity;
			currentHandGunAmmo = localAmmo;
		}
	}

	/// <summary>
	/// アサルトライフルの弾数の初期化処理
	/// </summary>
	void InitAssaultRifleMagazine()
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

		int localMagazine = assaultRifleMagazineCapacity - assaultRifleCurrentMagazine;
		int localAmmo = currentAssaultRifleAmmo - localMagazine;
		if (localAmmo < 0)
		{
			assaultRifleCurrentMagazine = currentAssaultRifleAmmo;
			currentAssaultRifleAmmo = 0;
		}
		else
		{
			assaultRifleCurrentMagazine = assaultRifleMagazineCapacity;
			currentAssaultRifleAmmo = localAmmo;
		}
	}

	/// <summary>
	/// ショットガンの弾数の初期化処理
	/// </summary>
	void InitShotGunMagazine()
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

		int localMagazine = shotGunMagazineCapacity - shotGunCurrentMagazine;
		int localAmmo = currentShotGunAmmo - localMagazine;
		if (localAmmo < 0)
		{
			shotGunCurrentMagazine = currentShotGunAmmo;
			currentShotGunAmmo = 0;
		}
		else
		{
			shotGunCurrentMagazine = shotGunMagazineCapacity;
			currentShotGunAmmo = localAmmo;
		}
	}

	void Update()
	{
		if (UI.SingletonInstance.IsPause == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		if (isCinemachineActive == true)
		{
			return;
		}

		SwitchWeapon();
		UpdateHitReticule();
	}

	/// <summary>
	/// 武器の切り替え
	/// </summary> 
	void SwitchWeapon()
	{
		if (isHandGunReloadTimeActive == false && isAssaultRifleReloadTimeActive == false && isShotGunReloadTimeActive == false)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				//Debug.Log("ハンドガン");
				gunTYPE = GunTYPE.HandGun;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				//Debug.Log("アサルトライフル");
				gunTYPE = GunTYPE.AssaultRifle;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				//Debug.Log("ショットガン");
				gunTYPE = GunTYPE.ShotGun;
			}
		}

		switch (gunTYPE)
		{
			case GunTYPE.HandGun:
				HandGunShoot();
				HandGunAutoReload();
				HandGunManualReload();
				HandGunReload();
				break;
			case GunTYPE.AssaultRifle:
				AssaultRifleShoot();
				AssaultRifleAutoReload();
				AssaultRifleManualReload();
				AssaultRifleReload();
				break;
			case GunTYPE.ShotGun:
				ShotGunShoot();
				ShotGunAutoReload();
				ShotGunManualReload();
				ShotGunReload();
				break;
		}
	}

	/// <summary>
	/// ハンドガンで射撃
	/// </summary> 
	void HandGunShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
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
	void HandGunAutoReload()
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
	void HandGunManualReload()
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

		if (Input.GetKey(KeyCode.R))
		{
			isHandGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ハンドガンのリロード
	/// </summary> 
	void HandGunReload()
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
		HandGunMuzzleFlashAndShell();
		HandGunSmoke();

		HandGunFireSE();

		Ray ray = new Ray(this.transform.position, this.transform.forward);
		Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
		{
			hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

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
				isHitReticule = true;

				//ヒット音を再生
				SoundManager.SingletonInstance.HitSEPool.GetGameObject(this.transform);

				//追跡開始
				GroundEnemy groundEnemy = hit.transform.GetComponent<GroundEnemy>();
				if (groundEnemy != null)
				{
					groundEnemy.ChaseOn();
				}

				//地雷を爆破
				Mine mine = hit.transform.GetComponent<Mine>();
				if (mine != null)
				{
					mine.Explosion();
				}
			}

			ImpactEffect(hit);
		}
	}

	/// <summary>
	/// ハンドガンの射撃SE
	/// </summary> 
	void HandGunFireSE()
	{
		SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(this.transform);
	}


	/// <summary>
	/// ハンドガンのリロードSE
	/// </summary> 
	void HandGunReloadSE()
	{
		SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	void HandGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void HandGunMuzzleFlashAndShell()
	{
		if (handGunShotEmitters != null)
		{
			foreach (var effect in handGunShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void HandGunSmoke()
	{
		if (handGunAfterFireSmoke != null)
		{
			handGunAfterFireSmoke.Play();
		}
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

	/// <summary>
	/// アサルトライフルで射撃
	/// </summary> 
	void AssaultRifleShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
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
	void AssaultRifleAutoReload()
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
	void AssaultRifleManualReload()
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

		if (Input.GetKey(KeyCode.R))
		{
			isAssaultRifleReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// アサルトライフルのリロード
	/// </summary> 
	void AssaultRifleReload()
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
		AssaultRifleMuzzleFlashAndShell();
		AssaultRifleSmoke();

		AssaultRifleFireSE();

		Vector3 direction = this.transform.forward;
		direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.up) * direction;
		direction = Quaternion.AngleAxis(Random.Range(-assaultRifleRandomAngle, assaultRifleRandomAngle), this.transform.right) * direction;

		Ray ray = new Ray(this.transform.position, direction);
		Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
		{
			hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

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
				isHitReticule = true;

				//ヒット音を再生
				SoundManager.SingletonInstance.HitSEPool.GetGameObject(this.transform);

				//追跡開始
				GroundEnemy groundEnemy = hit.transform.GetComponent<GroundEnemy>();
				if (groundEnemy != null)
				{
					groundEnemy.ChaseOn();
				}

				//地雷を爆破
				Mine mine = hit.transform.GetComponent<Mine>();
				if (mine != null)
				{
					mine.Explosion();
				}
			}

			ImpactEffect(hit);
		}
	}

	/// <summary>
	/// アサルトライフルの射撃SE
	/// </summary> 
	void AssaultRifleFireSE()
	{
		UnityEngine.GameObject se = Instantiate(assaultRifleShootSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// アサルトライフルのリロードSE
	/// </summary> 
	void AssaultRifleReloadSE()
	{
		UnityEngine.GameObject se = Instantiate(assaultRifleReloadSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	void AssaultRifleBulletCasingSE()
	{
		UnityEngine.GameObject se = Instantiate(assaultRifleBulletCasingSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void AssaultRifleMuzzleFlashAndShell()
	{
		if (assaultRifleShotEmitters != null)
		{
			foreach (var effect in assaultRifleShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void AssaultRifleSmoke()
	{
		if (assaultRifleAfterFireSmoke != null)
		{
			assaultRifleAfterFireSmoke.Play();
		}
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

	/// <summary>
	/// ショットガンで射撃
	/// </summary> 
	void ShotGunShoot()
	{
		if (Player.SingletonInstance.IsAim == true)
		{
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))//左クリックまたはEnterを押している場合に中身を実行する
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
	void ShotGunAutoReload()
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
	void ShotGunManualReload()
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

		if (Input.GetKey(KeyCode.R))
		{
			isShotGunReloadTimeActive = true;//リロードのオン
		}
	}

	/// <summary>
	/// ショットガンのリロード
	/// </summary> 
	void ShotGunReload()
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
		ShotGunMuzzleFlashAndShell();
		ShotGunSmoke();

		ShotGunFireSE();

		for (int i = 0; i < shotGunBullet; i++)
		{
			Vector3 direction = this.transform.forward;
			direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), this.transform.up) * direction;
			direction = Quaternion.AngleAxis(Random.Range(-shotGunRandomAngle, shotGunRandomAngle), this.transform.right) * direction;

			Ray ray = new Ray(this.transform.position, direction);
			Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 10.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
			{
				hitName = hit.collider.gameObject.name; // 衝突した相手オブジェクトの名前を取得

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
					isHitReticule = true;

					//ヒット音を再生
					SoundManager.SingletonInstance.HitSEPool.GetGameObject(this.transform);

					//追跡開始
					GroundEnemy groundEnemy = hit.transform.GetComponent<GroundEnemy>();
					if (groundEnemy != null)
					{
						groundEnemy.ChaseOn();
					}

					//地雷を爆破
					Mine mine = hit.transform.GetComponent<Mine>();
					if (mine != null)
					{
						mine.Explosion();
					}
				}

				ImpactEffect(hit);
			}
		}
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary> 
	void ShotGunFireSE()
	{
		UnityEngine.GameObject se = Instantiate(shotGunShootSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// ショットガンのリロードSE
	/// </summary> 
	void ShotGunReloadSE()
	{
		UnityEngine.GameObject se = Instantiate(shotGunReloadSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	void ShotGunBulletCasingSE()
	{
		UnityEngine.GameObject se = Instantiate(shotGunBulletCasingSe, this.transform.position, Quaternion.identity);
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void ShotGunMuzzleFlashAndShell()
	{
		if (shotGunShotEmitters != null)
		{
			foreach (var effect in shotGunShotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	void ShotGunSmoke()
	{
		if (shotGunAfterFireSmoke != null)
		{
			shotGunAfterFireSmoke.Play();
		}
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

	/// <summary>
	/// 着弾エフェクト
	/// </summary> 
	void ImpactEffect(RaycastHit hit)
	{
		if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
		{
			GameObject impactGameObject = Instantiate(bloodImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			Destroy(impactGameObject, 2.0f);
		}
		else
		{
			GameObject impactGameObject = Instantiate(rockImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
			Destroy(impactGameObject, 2.0f);
		}
	}

	/// <summary>
	/// ヒットレティクル
	/// </summary> 
	void UpdateHitReticule()
	{
		if (isHitReticule == true)
		{
			UI.SingletonInstance.ImageHitReticule.color = new Color32(255, 0, 0, 150);
		}

		if (isHitReticule == false)
		{
			UI.SingletonInstance.ImageHitReticule.color = Color.Lerp(UI.SingletonInstance.ImageHitReticule.color, Color.clear, Time.deltaTime * hitReticuleSpeed);
		}

		isHitReticule = false;
	}

	void FixedUpdate()
	{
		if (isCinemachineActive == true)
		{
			return;
		}

		//SRT
		if (Player.SingletonInstance.IsAim == false)
		{
			CameraNormalMove();
		}
		else if (Player.SingletonInstance.IsAim == true)
		{
			CameraAimMove();
		}


		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		CameraRot();
	}

	/// <summary>
	/// 通常カメラ
	/// </summary>
	void CameraNormalMove()
	{
		//通常カメラ位置をプレイヤーの座標位置から計算
		Vector3 cameraPos = Player.SingletonInstance.transform.position + (Vector3.up * normalUpPos) + (this.transform.forward * normalForwardPos);
		//カメラの位置を移動させる
		this.transform.position = Vector3.Lerp(transform.position, cameraPos, Player.SingletonInstance.NormalMoveSpeed * 10 * Time.deltaTime);
	}

	/// <summary>
	/// 肩越しカメラ
	/// </summary>
	void CameraAimMove()
	{
		//肩越しカメラ位置をプレイヤーの座標位置から計算
		Vector3 cameraPos = Player.SingletonInstance.transform.position + (Player.SingletonInstance.transform.right * aimRightPos) + (Vector3.up * aimUpPos) + (this.transform.forward * aimForwardPos);
		//カメラの位置を移動させる
		this.transform.position = Vector3.Lerp(transform.localPosition, cameraPos, Player.SingletonInstance.WeaponMoveSpeed * 10 * Time.deltaTime);
	}

	/// <summary>
	/// カメラの回転
	/// </summary> 
	void CameraRot()
	{
		// マウスの移動量を取得
		float x_Rotation = Input.GetAxis("Mouse X");
		float y_Rotation = Input.GetAxis("Mouse Y");

		if (Player.SingletonInstance.IsAim == true)
		{
			localCameraSpeedX = aimCameraSpeedX;
			localCameraSpeedY = aimCameraSpeedY;
			isTargethit = false;

			//ターゲットにあたった際にカメラを遅くする処理
			Ray ray = new Ray(this.transform.position, this.transform.forward);
			Debug.DrawRay(ray.origin, ray.direction * range, Color.gray, 1.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, range) == true) // もしRayを投射して何らかのコライダーに衝突したら
			{
				if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
				{
					//カメラの速さを遅くする
					localCameraSpeedX = aimCameraSpeedX / slowDownCameraSpeed;
					localCameraSpeedY = aimCameraSpeedY / slowDownCameraSpeed;
					isTargethit = true;
				}
			}
		}
		else if (Player.SingletonInstance.IsAim == false)
		{
			localCameraSpeedX = normalCameraSpeedX;
			localCameraSpeedY = normalCameraSpeedY;
			isTargethit = false;
		}

		//Mathf.Absは絶対値を返す(例)Mathf.Abs(10)なら10、Mathf.Abs(-10)なら10と+だろうが-だろうがプラスの値を返す
		//RotateAround(中心となるワールド座標, 回転軸, 回転角度(度数))関数は、指定の座標を中心にオブジェクトを回転させる関数

		// X方向に一定量移動していれば横回転
		if (deadZoneX < Mathf.Abs(x_Rotation))
		{
			// 回転軸はワールド座標のY軸
			this.transform.RotateAround(Player.SingletonInstance.transform.position, Vector3.up, x_Rotation * Time.deltaTime * localCameraSpeedX);
		}

		// Y方向に一定量移動していれば縦回転
		if (deadZoneY < Mathf.Abs(y_Rotation))
		{
			float cameraAngles = this.transform.localEulerAngles.x;
			const float lookingUpLimit = 360.0f;//変えてはいけない数値
			float lookingUp = 345.0f;//減らしていくほど上を見れる範囲が広がる
			const float lookingDownLimit = -10.0f;//変えてはいけない数値
			float lookingDown = 40;//増やしていくほど下を見れる範囲が広がる
			if (lookingUp < cameraAngles && cameraAngles < lookingUpLimit || lookingDownLimit < cameraAngles && cameraAngles < lookingDown)//ここの数値を変えればカメラの上下の止まる限界値が変わる
			{
				// 回転軸はカメラ自身のX軸
				this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
			}
			else
			{
				if (300 < cameraAngles)
				{
					if (Input.GetAxis("Mouse Y") < 0)
					{
						//マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
						this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
					}
				}
				else
				{
					if (0 < Input.GetAxis("Mouse Y"))
					{
						//マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
						this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
					}

				}
			}
		}
	}

	void OnGUI()
	{
#if UNITY_EDITOR//Unityエディター上での処理

		GUIStyle styleGreen = new GUIStyle();
		styleGreen.fontSize = 30;
		GUIStyleState styleStateGreen = new GUIStyleState();
		styleStateGreen.textColor = Color.green;
		styleGreen.normal = styleStateGreen;

		GUIStyle styleRed = new GUIStyle();
		styleRed.fontSize = 30;
		GUIStyleState styleStateRed = new GUIStyleState();
		styleStateRed.textColor = Color.red;
		styleRed.normal = styleStateRed;

		GUIStyle styleBlack = new GUIStyle();
		styleBlack.fontSize = 30;
		GUIStyleState styleStateBlack = new GUIStyleState();
		styleStateBlack.textColor = Color.black;
		styleBlack.normal = styleStateBlack;

		GUIStyle styleYellow = new GUIStyle();
		styleYellow.fontSize = 30;
		GUIStyleState styleStateYellow = new GUIStyleState();
		styleStateYellow.textColor = Color.yellow;
		styleYellow.normal = styleStateYellow;

		GUI.Box(new Rect(10, 650, 100, 50), "cameraPos", styleRed);
		GUI.Box(new Rect(350, 650, 100, 50), this.transform.position.ToString(), styleRed);
		GUI.Box(new Rect(10, 700, 100, 50), "hitName", styleRed);
		GUI.Box(new Rect(350, 700, 100, 50), hitName, styleRed);
		GUI.Box(new Rect(10, 750, 100, 50), "localCameraSpeedX", styleRed);
		GUI.Box(new Rect(350, 750, 100, 50), localCameraSpeedX.ToString(), styleRed);
		GUI.Box(new Rect(10, 800, 100, 50), "localCameraSpeedY", styleRed);
		GUI.Box(new Rect(350, 800, 100, 50), localCameraSpeedY.ToString(), styleRed);

#endif //終了  
	}
}

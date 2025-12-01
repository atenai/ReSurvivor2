using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Cinemachine;

/// <summary>
/// プレイヤー
/// </summary>
public class Player : MonoBehaviour
{
	//変数名は「小文字」から始める（test）
	//関数名は「大文字」から始める（Test）
	//引数名は「小文字」から始める（test）
	//アニメーション変数名は「型の最初の文字の_小文字から始める」（f_test）

	//シングルトンで作成（ゲーム中に１つのみにする）
	static Player singletonInstance = null;
	public static Player SingletonInstance => singletonInstance;

	[SerializeField] Animator animator;
	public Animator Animator => animator;
	[SerializeField] Rigidbody rb;

	[Header("プレイヤーキャラクターの移動関連")]
	[Tooltip("プレイヤーキャラクターの元気な時の通常移動速度")]
	[SerializeField] float energeticNormalMoveSpeed = 5.0f;
	[Tooltip("プレイヤーキャラクターの元気な時のエイム中移動速度")]
	[SerializeField] float energeticWeaponMoveSpeed = 3.0f;
	[Tooltip("プレイヤーキャラクターの疲れた時の通常移動速度")]
	float tiredNormalMoveSpeed = 4.0f;
	[Tooltip("プレイヤーキャラクターの疲れた時のエイム中移動速度")]
	float tiredWeaponMoveSpeed = 2.0f;

	[Tooltip("プレイヤーキャラクターの通常移動速度")]
	float normalMoveSpeed = 5.0f;
	public float NormalMoveSpeed => normalMoveSpeed;
	[Tooltip("プレイヤーキャラクターのエイム中移動速度")]
	float weaponMoveSpeed = 3.0f;
	public float WeaponMoveSpeed => weaponMoveSpeed;
	[Tooltip("ダッシュするか？")]
	bool isDash = false;
	float inputHorizontal;
	float inputVertical;
	Vector3 moveForward;
	Vector3 cameraForward;
	[Tooltip("エイムしているか？")]
	bool isAim = false;
	public bool IsAim => isAim;

	[Header("キャラクターモデル")]
	[Tooltip("キャラクターの首ボーン")]
	[SerializeField] Transform neck_01;
	[Tooltip("キャラクターの首ボーンの初期値")]
	float neck_01_InitEulerAnglesY;
	bool isNeck01AnimationRotInit = false;
	[Tooltip("キャラクターの脊椎ボーン")]
	[SerializeField] Transform spine_03;
	[Tooltip("キャラクターの脊椎ボーンの初期値")]
	float spine_03_InitEulerAnglesX;
	bool isSpine03AnimationRotInit = false;
	//※型のボーンを曲げると銃の持つ位置がずれておかしくなる為、首と背骨のボーンを曲げる事によって型のボーンを曲げずに済むようにする必要がある
	[Tooltip("キャラクターの右肩ボーン")]
	[SerializeField] Transform upperarm_r;
	[Tooltip("キャラクターの左肩ボーン")]
	[SerializeField] Transform upperarm_l;
	[Tooltip("肩のXボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	//const float armAimAnimationRotX = 5.0f;//←型のボーンを曲げると銃の持つ位置がずれてしまう
	const float armAimAnimationRotX = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる
	[Tooltip("肩のYボーンを曲げる数値(エイムアニメーションの銃の位置をカメラの中心に合わせる為の数値)")]
	//const float armAimAnimationRotY = 12.5f;//←型のボーンを曲げると銃の持つ位置がずれてしまう
	const float armAimAnimationRotY = 0.0f;//0にすれば武器を構えた際の腕のずれがなくなる
	[Tooltip("キャラクターの手に持っているハンドガンのモデル")]
	[SerializeField] GameObject handGunModel;
	[Tooltip("キャラクターの体についているハンドガンのモデル")]
	[SerializeField] GameObject handGunModelBodyDecoration;
	[Tooltip("ハンドガンのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform handGunMuzzleTransform;
	public Transform HandGunMuzzleTransform => handGunMuzzleTransform;
	[Tooltip("ハンドガンの薬莢の生成座標位置")]
	[SerializeField] Transform handGunBulletCasingTransform;
	public Transform HandGunBulletCasingTransform => handGunBulletCasingTransform;
	[Tooltip("ハンドガンの硝煙の生成座標位置")]
	[SerializeField] Transform handGunAfterFireSmokeTransform;
	public Transform HandGunAfterFireSmokeTransform => handGunAfterFireSmokeTransform;
	[Tooltip("キャラクターの手に持っているアサルトライフルのモデル")]
	[SerializeField] GameObject assaultRifleModel;
	[Tooltip("キャラクターの体についているアサルトライフルのモデル")]
	[SerializeField] GameObject assaultRifleModelBodyDecoration;
	[Tooltip("アサルトライフルのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform assaultRifleMuzzleTransform;
	public Transform AssaultRifleMuzzleTransform => assaultRifleMuzzleTransform;
	[Tooltip("アサルトライフルの薬莢の生成座標位置")]
	[SerializeField] Transform assaultRifleBulletCasingTransform;
	public Transform AssaultRifleBulletCasingTransform => assaultRifleBulletCasingTransform;
	[Tooltip("アサルトライフルの硝煙の生成座標位置")]
	[SerializeField] Transform assaultRifleAfterFireSmokeTransform;
	public Transform AssaultRifleAfterFireSmokeTransform => assaultRifleAfterFireSmokeTransform;
	[Tooltip("キャラクターの手に持っているショットガンのモデル")]
	[SerializeField] GameObject shotGunModel;
	[Tooltip("キャラクターの体についているショットガンのモデル")]
	[SerializeField] GameObject shotGunModelBodyDecoration;
	[Tooltip("ショットガンのマズルフラッシュの生成座標位置")]
	[SerializeField] Transform shotGunMuzzleTransform;
	public Transform ShotGunMuzzleTransform => shotGunMuzzleTransform;
	[Tooltip("ショットガンの薬莢の生成座標位置")]
	[SerializeField] Transform shotGunBulletCasingTransform;
	public Transform ShotGunBulletCasingTransform => shotGunBulletCasingTransform;
	[Tooltip("ショットガンの硝煙の生成座標位置")]
	[SerializeField] Transform shotGunAfterFireSmokeTransform;
	public Transform ShotGunAfterFireSmokeTransform => shotGunAfterFireSmokeTransform;

	[Header("UI")]
	[Tooltip("プレイヤーのキャンバス")]
	[SerializeField] Canvas canvasPlayer;
	[Tooltip("バックグラウンドイメージ")]
	[SerializeField] Image imageBG;
	[Tooltip("プレイヤーUIのY軸の傾き")]
	float playerUIRotY = 0.2f;
	[Tooltip("現在のHP")]
	float currentHp = 100.0f;
	[Tooltip("HPの最大値")]
	[SerializeField] float maxHp = 100.0f;
	[Tooltip("HPバー")]
	[SerializeField] Slider sliderHp;
	[Tooltip("現在のスタミナ")]
	float currentStamina = 1000.0f;
	[Tooltip("スタミナの最大値")]
	float maxStamina = 1000.0f;
	[Tooltip("スタミナバー")]
	[SerializeField] Slider sliderStamina;
	[Tooltip("疲れた時のスタミナ値")]
	[SerializeField] float tiredStamina = 100.0f;
	[Tooltip("現在のアーマープレート数")]
	int currentArmorPlate = 2;
	public int CurrentArmorPlate => currentArmorPlate;
	[Tooltip("アーマープレートの所持できる最大数")]
	int maxArmorPlate = 3;
	public int MaxArmorPlate => maxArmorPlate;
	[Tooltip("アーマープレートの所持できる限界最大数")]
	int limitMaximumArmorPlate = 10;
	[Tooltip("アーマープレートテキスト")]
	[SerializeField] TextMeshProUGUI textArmorPlate;
	[Tooltip("現在の食料数")]
	int currentFood = 2;
	public int CurrentFood => currentFood;
	[Tooltip("食料の所持できる最大数")]
	int maxFood = 3;
	public int MaxFood => maxFood;
	[Tooltip("食料の所持できる限界最大数")]
	int limitMaximumFood = 10;
	[Tooltip("食料テキスト")]
	[SerializeField] TextMeshProUGUI textFood;
	[Tooltip("リロード画像")]
	[SerializeField] GameObject imageReload;
	Color reloadColor = new Color(255.0f, 255.0f, 255.0f, 0.0f);
	float RotateSpeed = -500.0f;
	[Tooltip("マガジン弾数テキスト")]
	[SerializeField] TextMeshProUGUI textMagazine;
	[Tooltip("弾薬数テキスト")]
	[SerializeField] TextMeshProUGUI textAmmo;
	[Tooltip("タイマーテキスト")]
	[SerializeField] TextMeshProUGUI timerTMP;
	[Tooltip("分")]
	[SerializeField] int minute = 10;
	public int Minute
	{
		get { return minute; }
		set { minute = value; }
	}
	[Tooltip("秒")]
	[SerializeField] float seconds = 0.0f;
	public float Seconds
	{
		get { return seconds; }
		set { seconds = value; }
	}
	[Tooltip("totalTImeは秒で集計されている")]
	float totalTime = 0.0f;
	[Tooltip("ダメージ画像エフェクトトリガー")]
	bool isDamage = false;
	[Tooltip("HP回復画像エフェクトトリガー")]
	bool isHpHeal = false;
	[Tooltip("スタミナ回復画像エフェクトトリガー")]
	bool isStaminaHeal = false;
	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;
	[Tooltip("キャラクターからの地雷の生成距離")]
	float mineSpawnDistance = 1.0f;
	[Tooltip("地雷が再生成できるまでの間隔")]
	[SerializeField] float mineSpawnTimer = 2.0f;
	[Tooltip("地雷が再生成できるまでのカウント")]
	float mineSpawnCount;
	[Tooltip("現在の地雷数")]
	int currentMine = 3;
	[Tooltip("地雷テキスト")]
	[SerializeField] TextMeshProUGUI textMine;
	[Tooltip("地雷の所持できる最大数")]
	int maxMine = 3;
	[Tooltip("マップイメージ")]
	[SerializeField] Image imageMap;
	[SerializeField] MapUI mapUI;
	public MapUI MapUI
	{
		get { return mapUI; }
		set { mapUI = value; }
	}

	[SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
	public CinemachineImpulseSource CinemachineImpulseSource => cinemachineImpulseSource;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Start()
	{
		InitBoneNeck01();
		InitBoneSpine03();
		InitHP();
		InitStamina();
		StartTextArmorPlate();
		StartTextFood();
		StartImageReload();
		StartTextMagazine();
		StartDamageEffect();
		StartHpHealEffect();
		StartStaminaHealEffect();
		InitMine();
	}

	/// <summary>
	/// キャラクターの首ボーンの初期化処理
	/// </summary> 
	void InitBoneNeck01()
	{
		//キャラクターの脊椎ボーンの初期値を取得する（真正面に戻す際に必要なため）
		neck_01_InitEulerAnglesY = neck_01.eulerAngles.y;
	}

	/// <summary>
	/// キャラクターの脊椎ボーンの初期化処理
	/// </summary> 
	void InitBoneSpine03()
	{
		//キャラクターの脊椎ボーンの初期値を取得する（真正面に戻す際に必要なため）
		spine_03_InitEulerAnglesX = spine_03.eulerAngles.x;
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	void InitHP()
	{
		sliderHp.value = 1;
		currentHp = maxHp;
	}

	/// <summary>
	/// スタミナの初期化処理
	/// </summary> 
	void InitStamina()
	{
		sliderStamina.value = 1;
		currentStamina = maxStamina;
	}

	/// <summary>
	/// アーマープレートテキストの初期化処理
	/// </summary> 
	void StartTextArmorPlate()
	{
		textArmorPlate.text = currentArmorPlate.ToString();
	}

	/// <summary>
	/// 食料テキストの初期化処理
	/// </summary> 
	void StartTextFood()
	{
		textFood.text = currentFood.ToString();
	}

	/// <summary>
	/// リロードイメージの初期化処理
	/// </summary>
	void StartImageReload()
	{
		imageReload.GetComponent<Image>().color = reloadColor;
	}

	/// <summary>
	/// 残弾テキストの初期化処理
	/// </summary>
	void StartTextMagazine()
	{
		switch (PlayerCamera.SingletonInstance.gunTYPE)
		{
			case PlayerCamera.GunTYPE.HandGun:
				textMagazine.text = PlayerCamera.SingletonInstance.HandGunCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentHandGunAmmo.ToString();
				break;
			case PlayerCamera.GunTYPE.AssaultRifle:
				textMagazine.text = PlayerCamera.SingletonInstance.AssaultRifleCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentAssaultRifleAmmo.ToString();
				break;
			case PlayerCamera.GunTYPE.ShotGun:
				textMagazine.text = PlayerCamera.SingletonInstance.ShotGunCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentShotGunAmmo.ToString();
				break;
		}
	}

	/// <summary>
	/// ダメージ画像エフェクト
	/// </summary> 
	void StartDamageEffect()
	{
		isDamage = false;
	}

	/// <summary>
	/// HP回復画像エフェクト
	/// </summary> 
	void StartHpHealEffect()
	{
		isHpHeal = false;
	}

	/// <summary>
	/// スタミナ回復画像エフェクト
	/// </summary> 
	void StartStaminaHealEffect()
	{
		isStaminaHeal = false;
	}

	/// <summary>
	/// 地雷の初期化
	/// </summary> 
	void InitMine()
	{
		mineSpawnCount = mineSpawnTimer;
		textMine.text = currentMine.ToString();
	}

	void Update()
	{
		if (UI.SingletonInstance.IsPause == true)
		{
			return;
		}

		if (UI.SingletonInstance.IsComputerMenuActive == true)
		{
			return;
		}

		inputHorizontal = Input.GetAxisRaw("Horizontal");
		inputVertical = Input.GetAxisRaw("Vertical");

		//右ボタンまたはレフトシフトが押されていたら中身を実行
		if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftControl) || 0.5f < Input.GetAxisRaw("XInput LT"))
		{
			isAim = true;
		}
		else
		{
			isAim = false;
		}

		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("XInput L_Stick Click"))
		{
			isDash = true;
		}

		NormalMoveAnimation();

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		SwitchWeaponModel();

		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("XInput RB"))
		{
			Heal();
		}

		if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("XInput LB"))
		{
			RestoresStamina();
		}

		PlaceMine();
		PlayerUI();
		UpdateImageReload();
		UpdateTextMagazine();
		UpdateTimerSystem();
		UpdateDamageEffect();
		UpdateHpHealEffect();
		UpdateStaminaHealEffect();

		if (Input.GetKeyDown(KeyCode.M) || Input.GetButtonDown("XInput Pause"))
		{
			mapUI.EnableMap();
		}
	}

	/// <summary>
	/// 移動アニメーション
	/// </summary>
	void NormalMoveAnimation()
	{
		animator.SetFloat("f_moveSpeedX", inputHorizontal);
		animator.SetFloat("f_moveSpeedY", inputVertical);
		animator.SetBool("b_isAim", isAim);
	}

	/// <summary>
	/// 銃のモデルを切り替え
	/// </summary>
	void SwitchWeaponModel()
	{
		switch (PlayerCamera.SingletonInstance.gunTYPE)
		{
			case PlayerCamera.GunTYPE.HandGun:
				handGunModel.SetActive(true);
				handGunModelBodyDecoration.SetActive(false);
				assaultRifleModel.SetActive(false);
				assaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.SetActive(false);
				shotGunModelBodyDecoration.SetActive(true);
				break;
			case PlayerCamera.GunTYPE.AssaultRifle:
				handGunModel.SetActive(false);
				handGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.SetActive(true);
				assaultRifleModelBodyDecoration.SetActive(false);
				shotGunModel.SetActive(false);
				shotGunModelBodyDecoration.SetActive(true);
				break;
			case PlayerCamera.GunTYPE.ShotGun:
				handGunModel.SetActive(false);
				handGunModelBodyDecoration.SetActive(true);
				assaultRifleModel.SetActive(false);
				assaultRifleModelBodyDecoration.SetActive(true);
				shotGunModel.SetActive(true);
				shotGunModelBodyDecoration.SetActive(false);
				break;
		}
	}

	/// <summary>
	/// プレイヤーキャラクターの右横にある3DのUI
	/// </summary> 
	void PlayerUI()
	{
		if (isAim == false)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(this.transform.position.x, this.transform.position.y + PlayerCamera.SingletonInstance.NormalUpPos, this.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			//HP、スタミナ、弾薬、タイマーUI
			imageBG.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(150.0f, -100.0f, 0.0f);
			//マップUI
			imageMap.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
			imageMap.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageMap.transform.localPosition = new Vector3(150.0f, 70.0f, 0.0f);
		}
		else if (isAim == true)
		{
			//常にキャンバスをメインカメラの方を向かせる
			canvasPlayer.transform.rotation = Camera.main.transform.rotation;
			//キャンバスの高さとカメラの高さを合わせる（これをしないとプレイヤーUIの奥行がおかしくなる）
			canvasPlayer.gameObject.GetComponent<RectTransform>().position = new Vector3(this.transform.position.x, this.transform.position.y + PlayerCamera.SingletonInstance.AimUpPos, this.transform.position.z);
			//SRT(スケール→トランスフォーム→ローテーション)
			//HP、スタミナ、弾薬、タイマーUI
			imageBG.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
			imageBG.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageBG.transform.localPosition = new Vector3(85.0f, -20.0f, 0.0f);
			//マップUI
			imageMap.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
			imageMap.transform.localRotation = Quaternion.Euler(0.0f, playerUIRotY, 0.0f);
			imageMap.transform.localPosition = new Vector3(85.0f, 20.0f, 0.0f);
		}
	}

	/// <summary>
	/// リロードイメージの処理
	/// </summary>
	void UpdateImageReload()
	{
		imageReload.GetComponent<RectTransform>().transform.Rotate(0.0f, 0.0f, RotateSpeed * Time.deltaTime);

		switch (PlayerCamera.SingletonInstance.gunTYPE)
		{
			case PlayerCamera.GunTYPE.HandGun:
				if (PlayerCamera.SingletonInstance.IsHandGunReloadTimeActive == true)
				{
					if (reloadColor.a <= 1)
					{
						reloadColor.a += Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}

				if (PlayerCamera.SingletonInstance.IsHandGunReloadTimeActive == false)
				{
					if (reloadColor.a >= 0)
					{
						reloadColor.a -= Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}
				break;
			case PlayerCamera.GunTYPE.AssaultRifle:
				if (PlayerCamera.SingletonInstance.IsAssaultRifleReloadTimeActive == true)
				{
					if (reloadColor.a <= 1)
					{
						reloadColor.a += Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}

				if (PlayerCamera.SingletonInstance.IsAssaultRifleReloadTimeActive == false)
				{
					if (reloadColor.a >= 0)
					{
						reloadColor.a -= Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}
				break;
			case PlayerCamera.GunTYPE.ShotGun:
				if (PlayerCamera.SingletonInstance.IsShotGunReloadTimeActive == true)
				{
					if (reloadColor.a <= 1)
					{
						reloadColor.a += Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}

				if (PlayerCamera.SingletonInstance.IsShotGunReloadTimeActive == false)
				{
					if (reloadColor.a >= 0)
					{
						reloadColor.a -= Time.deltaTime * 2.0f;
						imageReload.GetComponent<Image>().color = reloadColor;
					}
				}
				break;
		}
	}

	/// <summary>
	/// 残弾テキスト
	/// </summary>
	void UpdateTextMagazine()
	{
		switch (PlayerCamera.SingletonInstance.gunTYPE)
		{
			case PlayerCamera.GunTYPE.HandGun:
				textMagazine.text = PlayerCamera.SingletonInstance.HandGunCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentHandGunAmmo.ToString();
				break;
			case PlayerCamera.GunTYPE.AssaultRifle:
				textMagazine.text = PlayerCamera.SingletonInstance.AssaultRifleCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentAssaultRifleAmmo.ToString();
				break;
			case PlayerCamera.GunTYPE.ShotGun:
				textMagazine.text = PlayerCamera.SingletonInstance.ShotGunCurrentMagazine.ToString();
				textAmmo.text = PlayerCamera.SingletonInstance.CurrentShotGunAmmo.ToString();
				break;
		}
	}

	/// <summary>
	/// 時間経過の処理
	/// </summary>
	void UpdateTimerSystem()
	{
		if (InGameManager.SingletonInstance.IsMissionActive == false)
		{
			timerTMP.text = "--" + ":" + "--";
			return;
		}

		totalTime = (minute * 60) + seconds;
		totalTime = totalTime - Time.deltaTime;

		minute = (int)totalTime / 60;
		seconds = totalTime - (minute * 60);

		if (minute <= 0 && seconds <= 0.0f)
		{
			timerTMP.text = "00" + ":" + "00";
			//ゲームオーバー処理
			SceneManager.LoadScene("GameOver");
		}
		else
		{
			timerTMP.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
		}
	}

	/// <summary>
	/// 地雷を設置する
	/// </summary>
	void PlaceMine()
	{
		if (minePrefab == null)
		{
			return;
		}

		mineSpawnCount = mineSpawnCount + Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.G) || 0.5 < Input.GetAxisRaw("XInput DPad Up&Down"))
		{
			if (currentMine <= 0)
			{
				return;
			}

			if (mineSpawnTimer < mineSpawnCount)
			{
				mineSpawnCount = 0.0f;
				currentMine = currentMine - 1;
				textMine.text = currentMine.ToString();

				Vector3 localPosition = new Vector3(this.transform.position.x, 0.0f, this.transform.position.z);
				// キャラクターの前方にオブジェクトを生成
				Vector3 spawnPosition = localPosition + (this.transform.forward * mineSpawnDistance);
				GameObject localGameObject = UnityEngine.Object.Instantiate(minePrefab, spawnPosition, this.transform.rotation);
			}
		}
	}

	void LateUpdate()
	{
		//ボーンを曲げる際は必ずLateUpdateに書く必要がある！（これいつかメモする！）
		RotateBoneNeck01();
		RotateBoneSpine03();
		RotateBoneUpperArmR();
		RotateBoneUpperArmL();
	}

	/// <summary>
	/// キャラクターの首ボーンを曲げる
	/// </summary> 
	void RotateBoneNeck01()
	{
		if (isAim == true)
		{
			const float aimAnimationRotY = -20.0f;
			//腰のボーンの角度をカメラの向きにする
			neck_01.rotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01.eulerAngles.y + aimAnimationRotY, neck_01.eulerAngles.z);
			isNeck01AnimationRotInit = true;
		}
		else if (isAim == false)
		{
			if (isNeck01AnimationRotInit == true)
			{
				//腰のボーンの角度を真正面（初期値）にする
				neck_01.rotation = Quaternion.Euler(neck_01.eulerAngles.x, neck_01_InitEulerAnglesY, neck_01.eulerAngles.z);
				isNeck01AnimationRotInit = false;
			}
		}
	}

	/// <summary>
	/// キャラクターの脊椎ボーンを曲げる
	/// </summary> 
	void RotateBoneSpine03()
	{
		if (isAim == true)
		{
			const float aimAnimationRotX = 12.5f;
			const float aimAnimationRotY = 12.5f;
			//腰のボーンの角度をカメラの向きにする
			spine_03.rotation = Quaternion.Euler(PlayerCamera.SingletonInstance.transform.localEulerAngles.x + aimAnimationRotX, spine_03.eulerAngles.y + aimAnimationRotY, spine_03.eulerAngles.z);
			isSpine03AnimationRotInit = true;
		}
		else if (isAim == false)
		{
			if (isSpine03AnimationRotInit == true)
			{
				//腰のボーンの角度を真正面（初期値）にする
				spine_03.rotation = Quaternion.Euler(spine_03_InitEulerAnglesX, spine_03.eulerAngles.y, spine_03.eulerAngles.z);
				isSpine03AnimationRotInit = false;
			}
		}
	}

	/// <summary>
	/// キャラクターの右肩ボーンを曲げる
	/// </summary> 
	void RotateBoneUpperArmR()
	{
		if (isAim == true)
		{
			//右肩のボーンの角度をカメラの向きにする
			//upperarm_r.rotation = Quaternion.Euler(PlayerCamera.singletonInstance.transform.localEulerAngles.x + aimAnimationRotX, upperarm_r.eulerAngles.y + aimAnimationRotY, upperarm_r.eulerAngles.z);
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x + armAimAnimationRotX, upperarm_r.eulerAngles.y + armAimAnimationRotY, upperarm_r.eulerAngles.z);
		}
		else if (isAim == false)
		{
			//右肩のボーンの角度を真正面（初期値）にする
			upperarm_r.rotation = Quaternion.Euler(upperarm_r.eulerAngles.x, upperarm_r.eulerAngles.y, upperarm_r.eulerAngles.z);
		}
	}

	/// <summary>
	/// キャラクターの左肩ボーンを曲げる
	/// </summary> 
	void RotateBoneUpperArmL()
	{
		if (isAim == true)
		{
			//左肩のボーンの角度をカメラの向きにする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x + armAimAnimationRotX, upperarm_l.eulerAngles.y + armAimAnimationRotY, upperarm_l.eulerAngles.z);
		}
		else if (isAim == false)
		{
			//左肩のボーンの角度を真正面（初期値）にする
			upperarm_l.rotation = Quaternion.Euler(upperarm_l.eulerAngles.x, upperarm_l.eulerAngles.y, upperarm_l.eulerAngles.z);
		}
	}

	void FixedUpdate()
	{
		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		ChangeMoveSpeed();
		NormalMove();
		AimMove();
	}

	/// プレイヤーの移動はフルスクラッチする、何故ならTPSで通常カメラと肩越しカメラとで移動やアニメーションを切り替える必要がある為
	/// キャラクターの移動はRigidbody.velocityで行う、参考作品としてランアウェイシュートを参考にする

	/// <summary>
	/// 通常状態移動
	/// </summary>
	void NormalMove()
	{
		if (isAim == true)
		{
			return;
		}

		//カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
		cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

		//方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
		moveForward = cameraForward * inputVertical + Camera.main.transform.right * inputHorizontal;
		//移動方向のベクトルを正規化
		moveForward.Normalize();

		//移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
		rb.velocity = moveForward * normalMoveSpeed + new Vector3(0, rb.velocity.y, 0);

		//キャラクターの向きをキャラクターの進行方向にする
		if (moveForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
		{
			this.transform.rotation = Quaternion.LookRotation(moveForward);
		}

		if (isDash == true)
		{
			if (inputVertical <= -1 || 1 <= inputVertical)//前後移動入力
			{
				//スタミナ消費
				ConsumeStamina(2.0f);
			}
			else if (inputHorizontal <= -1 || 1 <= inputHorizontal)//左右移動入力
			{
				//スタミナ消費
				ConsumeStamina(2.0f);
			}
			else
			{
				isDash = false;
			}
		}


#if UNITY_EDITOR
		VectorVisualizer();
#endif
	}

	/// <summary>
	/// 構えた状態移動
	/// </summary>
	void AimMove()
	{
		if (isAim == false)
		{
			return;
		}

		//カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
		cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

		//方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
		moveForward = cameraForward * inputVertical + Camera.main.transform.right * inputHorizontal;
		//移動方向のベクトルを正規化
		moveForward.Normalize();

		//移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
		rb.velocity = moveForward * weaponMoveSpeed + new Vector3(0, rb.velocity.y, 0);

		//キャラクターの向きをカメラの前方にする
		if (cameraForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
		{
			this.transform.rotation = Quaternion.LookRotation(cameraForward);
		}
	}

	/// <summary>
	/// ベクトルの可視化
	/// </summary>
	void VectorVisualizer()
	{
		Ray debugRayVelocity = new Ray(this.transform.position, rb.velocity);
		Debug.DrawRay(debugRayVelocity.origin, debugRayVelocity.direction, Color.magenta);
	}

	/// <summary>
	/// 移動スピードを変える
	/// </summary>
	void ChangeMoveSpeed()
	{
		if (isAim == true)
		{
			isDash = false;
		}

		if (currentStamina <= tiredStamina)
		{
			isDash = false;
		}

		if (isDash == true)
		{
			//元気な際の移動速度
			normalMoveSpeed = energeticNormalMoveSpeed;
			weaponMoveSpeed = energeticWeaponMoveSpeed;
		}
		else
		{
			//疲れた際の移動速度
			normalMoveSpeed = tiredNormalMoveSpeed;
			weaponMoveSpeed = tiredWeaponMoveSpeed;
		}

	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.tag == "Enemy" || collision.collider.tag == "FlyingEnemy" || collision.collider.tag == "GroundEnemy")
		{
			TakeDamage(10.0f);
			Shaker();
		}
	}

	void Shaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	public void TakeDamage(float amount)
	{
		currentHp = currentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		sliderHp.value = (float)currentHp / (float)maxHp;

		isDamage = true;

		//PlayerCamera.SingletonInstance.Shake(0.25f, 0.1f);

		if (currentHp <= 0.0f)
		{
			SceneManager.LoadScene("GameOver");
		}
	}

	/// <summary>
	/// ダメージ画像エフェクト
	/// </summary> 
	void UpdateDamageEffect()
	{
		if (isDamage == true)
		{
			UI.SingletonInstance.ImageDamage.color = new Color(0.5f, 0f, 0f, 0.5f);
		}

		if (isDamage == false)
		{
			UI.SingletonInstance.ImageDamage.color = Color.Lerp(UI.SingletonInstance.ImageDamage.color, Color.clear, Time.deltaTime);
		}

		isDamage = false;
	}

	/// <summary>
	/// HPを回復
	/// </summary>
	void Heal()
	{
		if (currentArmorPlate <= 0)
		{
			return;
		}

		if (maxHp <= currentHp)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate - 1;
		textArmorPlate.text = currentArmorPlate.ToString();

		currentHp = maxHp;
		sliderHp.value = (float)currentHp / (float)maxHp;

		isHpHeal = true;
	}

	/// <summary>
	/// HP回復画像エフェクト
	/// </summary> 
	void UpdateHpHealEffect()
	{
		if (isHpHeal == true)
		{
			UI.SingletonInstance.ImageHpHeal.color = new Color(0f, 0.5f, 0f, 0.5f);
		}

		if (isHpHeal == false)
		{
			UI.SingletonInstance.ImageHpHeal.color = Color.Lerp(UI.SingletonInstance.ImageHpHeal.color, Color.clear, Time.deltaTime);
		}

		isHpHeal = false;
	}

	/// <summary>
	/// アーマープレートを取得
	/// </summary> 
	public void AcquireArmorPlate()
	{
		if (maxArmorPlate <= currentArmorPlate)
		{
			return;
		}

		currentArmorPlate = currentArmorPlate + 1;
		textArmorPlate.text = currentArmorPlate.ToString();
	}

	/// <summary>
	/// アーマープレートの所持できる最大数を増加
	/// </summary>
	public void IncreaseMaxArmorPlate()
	{
		if (limitMaximumArmorPlate <= maxArmorPlate)
		{
			return;
		}

		maxArmorPlate = maxArmorPlate + 1;
	}

	/// <summary>
	/// スタミナを消費
	/// </summary> 
	void ConsumeStamina(float amount)
	{
		if (currentStamina <= 0.0f)
		{
			currentStamina = 0.0f;
			return;
		}

		currentStamina = currentStamina - amount;
		//Debug.Log("<color=orange>currentStamina : " + currentStamina + "</color>");
		sliderStamina.value = (float)currentStamina / (float)maxStamina;
	}

	/// <summary>
	/// スタミナを回復
	/// </summary>
	void RestoresStamina()
	{
		if (currentFood <= 0)
		{
			return;
		}

		if (maxStamina <= currentStamina)
		{
			return;
		}

		currentFood = currentFood - 1;
		textFood.text = currentFood.ToString();

		currentStamina = maxStamina;
		sliderStamina.value = (float)currentStamina / (float)maxStamina;

		isStaminaHeal = true;
	}

	/// <summary>
	/// スタミナ回復画像エフェクト
	/// </summary> 
	void UpdateStaminaHealEffect()
	{
		if (isStaminaHeal == true)
		{
			UI.SingletonInstance.ImageStaminaHeal.color = new Color(0.5f, 0.5f, 0f, 0.5f);
		}

		if (isStaminaHeal == false)
		{
			UI.SingletonInstance.ImageStaminaHeal.color = Color.Lerp(UI.SingletonInstance.ImageStaminaHeal.color, Color.clear, Time.deltaTime);
		}

		isStaminaHeal = false;
	}

	/// <summary>
	/// 食料を取得
	/// </summary> 
	public void AcquireFood()
	{
		if (maxFood <= currentFood)
		{
			return;
		}

		currentFood = currentFood + 1;
		textFood.text = currentFood.ToString();
	}

	/// <summary>
	/// 食料の所持できる最大数を増加
	/// </summary>
	public void IncreaseMaxFood()
	{
		if (limitMaximumFood <= maxFood)
		{
			return;
		}

		maxFood = maxFood + 1;
	}

	/// <summary>
	/// 地雷を取得
	/// </summary> 
	public void AcquireMine()
	{
		if (maxMine <= currentMine)
		{
			return;
		}

		currentMine = currentMine + 1;
		textMine.text = currentMine.ToString();
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

		int lineHeight = 50;

		GUI.Box(new Rect(10, 0 * lineHeight, 100, 50), "inputHorizontal", styleGreen);
		GUI.Box(new Rect(350, 0 * lineHeight, 100, 50), inputHorizontal.ToString(), styleGreen);
		GUI.Box(new Rect(10, 1 * lineHeight, 100, 50), "inputVertical", styleGreen);
		GUI.Box(new Rect(350, 1 * lineHeight, 100, 50), inputVertical.ToString(), styleGreen);
		GUI.Box(new Rect(10, 2 * lineHeight, 100, 50), "normalMoveSpeed", styleGreen);
		GUI.Box(new Rect(350, 2 * lineHeight, 100, 50), normalMoveSpeed.ToString(), styleGreen);
		GUI.Box(new Rect(10, 3 * lineHeight, 100, 50), "weaponMoveSpeed", styleGreen);
		GUI.Box(new Rect(350, 3 * lineHeight, 100, 50), weaponMoveSpeed.ToString(), styleGreen);
		GUI.Box(new Rect(10, 4 * lineHeight, 100, 50), "isDash", styleGreen);
		GUI.Box(new Rect(350, 4 * lineHeight, 100, 50), isDash.ToString(), styleGreen);
		GUI.Box(new Rect(10, 5 * lineHeight, 100, 50), "rb.velocity", styleGreen);
		GUI.Box(new Rect(350, 5 * lineHeight, 100, 50), rb.velocity.ToString(), styleGreen);
		GUI.Box(new Rect(10, 6 * lineHeight, 100, 50), "moveForward", styleGreen);
		GUI.Box(new Rect(350, 6 * lineHeight, 100, 50), moveForward.ToString(), styleGreen);
		GUI.Box(new Rect(10, 7 * lineHeight, 100, 50), "cameraForward", styleGreen);
		GUI.Box(new Rect(350, 7 * lineHeight, 100, 50), cameraForward.ToString(), styleGreen);
		GUI.Box(new Rect(10, 8 * lineHeight, 100, 50), "isAim", styleGreen);
		GUI.Box(new Rect(350, 8 * lineHeight, 100, 50), isAim.ToString(), styleGreen);
		GUI.Box(new Rect(10, 9 * lineHeight, 100, 50), "spine_03.eulerAngles", styleGreen);
		GUI.Box(new Rect(350, 9 * lineHeight, 100, 50), spine_03.eulerAngles.ToString(), styleGreen);
		GUI.Box(new Rect(10, 10 * lineHeight, 100, 50), "upperarm_r.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 10 * lineHeight, 100, 50), upperarm_r.eulerAngles.x + armAimAnimationRotX.ToString(), styleGreen);
		GUI.Box(new Rect(10, 11 * lineHeight, 100, 50), "upperarm_r.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 11 * lineHeight, 100, 50), upperarm_r.eulerAngles.y + armAimAnimationRotY.ToString(), styleGreen);
		GUI.Box(new Rect(10, 12 * lineHeight, 100, 50), "upperarm_l.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 12 * lineHeight, 100, 50), upperarm_l.eulerAngles.x + armAimAnimationRotX.ToString(), styleGreen);
		GUI.Box(new Rect(10, 13 * lineHeight, 100, 50), "upperarm_l.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 13 * lineHeight, 100, 50), upperarm_l.eulerAngles.y + armAimAnimationRotY.ToString(), styleGreen);

#endif //終了  
	}
}

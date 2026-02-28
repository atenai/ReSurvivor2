using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// プレイヤー
/// </summary>
public class Player : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static Player singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static Player SingletonInstance => singletonInstance;

	[Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
	static bool isFirstLoad = true;
	public static bool IsFirstLoad
	{
		get { return isFirstLoad; }
		set { isFirstLoad = value; }
	}

	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	public Animator Animator => animator;
	[Tooltip("リジッドボディ")]
	[SerializeField] Rigidbody rb;
	[Tooltip("プレイヤーUI")]
	[SerializeField] PlayerUI playerUI;
	public PlayerUI PlayerUI => playerUI;

	[Header("プレイヤーキャラクターの移動関連")]
	/// <summary>縦入力</summary>
	float inputVertical;
	public float InputVertical => inputVertical;
	/// <summary>横入力</summary>
	float inputHorizontal;
	public float InputHorizontal => inputHorizontal;
	/// <summary>プレイヤーの前方向</summary>
	Vector3 moveForward;
	/// <summary>カメラの前方向</summary>
	Vector3 cameraForward;
	/// <summary>ダッシュするか？</summary>
	bool isDash = false;
	public bool IsDash => isDash;
	/// <summary>エイムしているか？</summary>
	bool isAim = false;
	public bool IsAim => isAim;
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

	[Header("リスポーンポイント")]
	[Tooltip("プレイヤーのリスポーンポイントの位置")]
	Vector3 respawnPosition = new Vector3(0.0f, 1.0f, 0.0f);
	[Tooltip("プレイヤーのリスポーンポイントの回転")]
	Quaternion respawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

	[Tooltip("現在のHP")]
	float currentHp = 100.0f;
	public float CurrentHp => currentHp;
	[Tooltip("HPの最大値")]
	float maxHp = 100.0f;
	public float MaxHp => maxHp;
	[Tooltip("現在のスタミナ")]
	float currentStamina = 1000.0f;
	public float CurrentStamina => currentStamina;
	[Tooltip("スタミナの最大値")]
	float maxStamina = 1000.0f;
	public float MaxStamina => maxStamina;
	[Tooltip("疲れてダッシュできなくなる時のスタミナ値")]
	float tiredStamina = 100.0f;
	[Tooltip("現在のアーマープレート数")]
	int currentArmorPlate = 2;
	public int CurrentArmorPlate => currentArmorPlate;
	[Tooltip("アーマープレートの所持できる最大数")]
	int maxArmorPlate = 3;
	public int MaxArmorPlate => maxArmorPlate;
	[Tooltip("アーマープレートの所持できる限界最大数")]
	int limitMaximumArmorPlate = 10;
	[Tooltip("現在の食料数")]
	int currentFood = 2;
	public int CurrentFood => currentFood;
	[Tooltip("食料の所持できる最大数")]
	int maxFood = 3;
	public int MaxFood => maxFood;
	[Tooltip("食料の所持できる限界最大数")]
	int limitMaximumFood = 10;

	[Tooltip("ダメージ画像エフェクトトリガー")]
	bool isDamage = false;
	public bool IsDamage
	{
		get { return isDamage; }
		set { isDamage = value; }
	}
	[Tooltip("HP回復画像エフェクトトリガー")]
	bool isHpHeal = false;
	public bool IsHpHeal
	{
		get { return isHpHeal; }
		set { isHpHeal = value; }
	}
	[Tooltip("スタミナ回復画像エフェクトトリガー")]
	bool isStaminaHeal = false;
	public bool IsStaminaHeal
	{
		get { return isStaminaHeal; }
		set { isStaminaHeal = value; }
	}
	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;
	[Tooltip("キャラクターからの地雷の生成距離")]
	float mineSpawnDistance = 1.0f;
	/// <summary>地雷を生成する際に長押しと判定する間隔、例：0.5秒なら（30フレーム ÷ 60fps = 0.5秒）に設定（60fpsの場合）</summary>
	const float Mine_Hold_Time = 30.0f / 60.0f;

	[Tooltip("地雷が再生成できるまでのカウント")]
	float mineSpawnCount = 0.0f;
	[Tooltip("現在の地雷数")]
	int currentMine = 3;
	[Tooltip("地雷の所持できる最大数")]
	int maxMine = 3;

	[SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
	public CinemachineImpulseSource CinemachineImpulseSource => cinemachineImpulseSource;

	[Header("キャラクターモデル")]
	[Tooltip("キャラクターモデル")]
	[SerializeField] PlayerModel playerModel;
	public PlayerModel PlayerModel => playerModel;

	[Header("ガンモデル")]
	[Tooltip("ガンモデルファサード")]
	[SerializeField] GunModelFacade gunModelFacade = new GunModelFacade();
	public GunModelFacade GunModelFacade => gunModelFacade;

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

		Load();
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>プレイヤーセーブ</color>");
		ES3.Save<float>("Hp", currentHp);
		ES3.Save<float>("Stamina", currentStamina);
		ES3.Save<int>("ArmorPlate", currentArmorPlate);
		ES3.Save<int>("Food", currentFood);
		ES3.Save<int>("Mine", currentMine);
		ES3.Save("PlayerPos", respawnPosition);
		ES3.Save("PlayerRot", respawnRotation);
	}

	/// <summary>
	/// ロード
	/// </summary>
	void Load()
	{
		if (isFirstLoad == false)
		{
			return;
		}
		isFirstLoad = false;

		Debug.Log("<color=purple>プレイヤーロード</color>");
		currentHp = ES3.Load<float>("Hp", maxHp);
		Debug.Log("<color=purple>HP : " + currentHp + "</color>");
		currentStamina = ES3.Load<float>("Stamina", maxStamina);
		Debug.Log("<color=purple>スタミナ : " + currentStamina + "</color>");
		currentArmorPlate = ES3.Load<int>("ArmorPlate", 2);
		Debug.Log("<color=purple>アーマープレート : " + currentArmorPlate + "</color>");
		currentFood = ES3.Load<int>("Food", 2);
		Debug.Log("<color=purple>食料 : " + currentFood + "</color>");
		currentMine = ES3.Load<int>("Mine", 3);
		Debug.Log("<color=purple>地雷 : " + currentMine + "</color>");
		//ステージが切り替わる度にリスポーン位置が呼ばれるため、リスポーンのオブジェクトは遷移先のステージで敵やオブジェクトにかぶらないように注意すること！！
		if (ES3.KeyExists("PlayerPos") == true)
		{
			respawnPosition = ES3.Load<Vector3>("PlayerPos");
			Debug.Log("<color=purple>プレイヤー位置 : " + this.transform.position + "</color>");
		}
		if (ES3.KeyExists("PlayerRot") == true)
		{
			respawnRotation = ES3.Load<Quaternion>("PlayerRot");
			Debug.Log("<color=purple>プレイヤー回転 : " + this.transform.rotation + "</color>");
		}
	}

	/// <summary>
	/// プレイヤーのリスポーンポイントを設定する
	/// </summary>
	/// <param name="pos">プレイヤー位置</param>
	/// <param name="rot">プレイヤー回転</param>
	public void SetPlayerRespawnPoint(Vector3 pos, Quaternion rot)
	{
		respawnPosition = pos;
		respawnRotation = rot;
	}

	void Start()
	{
		InitHP();
		StartDamageEffect();
		StartHpHealEffect();
		StartStaminaHealEffect();
		InitMine();
		InitRespawnPos();
		ResetMove();
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	void InitHP()
	{
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", currentHp / maxHp);
	}

	/// <summary>
	/// ダメージ画像エフェクトの初期化処理
	/// </summary> 
	void StartDamageEffect()
	{
		isDamage = false;
	}

	/// <summary>
	/// HP回復画像エフェクトの初期化処理
	/// </summary> 
	void StartHpHealEffect()
	{
		isHpHeal = false;
	}

	/// <summary>
	/// スタミナ回復画像エフェクトの初期化処理
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
		mineSpawnCount = 0.0f;
		playerUI.TextMine.text = currentMine.ToString();
	}

	/// <summary>
	/// リスポーン座標
	/// </summary>
	void InitRespawnPos()
	{
		this.transform.position = respawnPosition;
		this.transform.rotation = respawnRotation;
	}

	void ResetMove()
	{
		inputHorizontal = 0.0f;
		inputVertical = 0.0f;
		moveForward = Vector3.zero;
		rb.velocity = Vector3.zero;
		isAim = false;
		isDash = false;
	}

	void Update()
	{
		//ポーズ中は切り上げる
		if (ScreenUI.SingletonInstance.IsPause == true)
		{
			return;
		}

		//コンピュータを使用中は切り上げる
		if (ScreenUI.SingletonInstance.IsComputerMenuActive == true)
		{
			ResetMove();
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			ResetMove();
			return;
		}
		//↓ロード中に動かせない処理

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

		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("XInput RB"))
		{
			Heal();
		}

		if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("XInput LB"))
		{
			RestoresStamina();
		}

		//MineHoldKey();
		MineHoldXInput();
	}

	void MineHoldKey()
	{
		//キーを押しているかを判定
		if (Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.G))
		{
			mineSpawnCount = mineSpawnCount + Time.deltaTime;
			Debug.Log("押しているフレーム数 : " + mineSpawnCount);
		}

		//キーを離した瞬間を判定
		if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.G))
		{
			//長押しを判定
			if (Mine_Hold_Time <= mineSpawnCount)
			{
				Debug.Log("長押し");
				PlaceMine();
			}

			mineSpawnCount = 0.0f;
		}
	}

	void MineHoldXInput()
	{
		if (XInputManager.SingletonInstance.XInputDPadHandler.UpDown == true)
		{
			PlaceMine();
		}
	}

	/// <summary>
	/// 地雷を設置する
	/// </summary>
	void PlaceMine()
	{
		if (currentMine <= 0)
		{
			return;
		}

		currentMine = currentMine - 1;
		playerUI.TextMine.text = currentMine.ToString();

		Vector3 localPosition = new Vector3(this.transform.position.x, 0.0f, this.transform.position.z);
		// キャラクターの前方にオブジェクトを生成
		Vector3 spawnPosition = localPosition + (this.transform.forward * mineSpawnDistance);
		GameObject localGameObject = UnityEngine.Object.Instantiate(minePrefab, spawnPosition, this.transform.rotation);
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
			if (inputVertical <= -0.1f || 0.1f <= inputVertical)//前後移動入力
			{
				//スタミナ消費
				ConsumeStamina(2.0f);
			}
			else if (inputHorizontal <= -0.1f || 0.1f <= inputHorizontal)//左右移動入力
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
			CameraShaker();
		}
	}

	/// <summary>
	/// カメラを揺さぶる
	/// </summary>
	void CameraShaker()
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
		playerUI.SliderHp.value = (float)currentHp / (float)maxHp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", currentHp / maxHp);
		isDamage = true;

		if (currentHp <= 0.0f)
		{
			//ゲームオーバー処理
			InGameManager.SingletonInstance.GameOver();
		}
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
		playerUI.TextArmorPlate.text = currentArmorPlate.ToString();

		currentHp = maxHp;
		playerUI.SliderHp.value = (float)currentHp / (float)maxHp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", currentHp / maxHp);
		isHpHeal = true;
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
		playerUI.TextArmorPlate.text = currentArmorPlate.ToString();
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
		playerUI.SliderStamina.value = (float)currentStamina / (float)maxStamina;
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
		playerUI.TextFood.text = currentFood.ToString();

		currentStamina = maxStamina;
		playerUI.SliderStamina.value = (float)currentStamina / (float)maxStamina;

		isStaminaHeal = true;
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
		playerUI.TextFood.text = currentFood.ToString();
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
		playerUI.TextMine.text = currentMine.ToString();
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
		GUI.Box(new Rect(350, 9 * lineHeight, 100, 50), playerModel.Spine_03.eulerAngles.ToString(), styleGreen);
		GUI.Box(new Rect(10, 10 * lineHeight, 100, 50), "upperarm_r.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 10 * lineHeight, 100, 50), playerModel.Upperarm_r.eulerAngles.x + playerModel.Arm_Aim_Animation_Rot_X.ToString(), styleGreen);
		GUI.Box(new Rect(10, 11 * lineHeight, 100, 50), "upperarm_r.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 11 * lineHeight, 100, 50), playerModel.Upperarm_r.eulerAngles.y + playerModel.Arm_Aim_Animation_Rot_Y.ToString(), styleGreen);
		GUI.Box(new Rect(10, 12 * lineHeight, 100, 50), "upperarm_l.eulerAngles.x + armAimAnimationRotX", styleGreen);
		GUI.Box(new Rect(750, 12 * lineHeight, 100, 50), playerModel.Upperarm_l.eulerAngles.x + playerModel.Arm_Aim_Animation_Rot_X.ToString(), styleGreen);
		GUI.Box(new Rect(10, 13 * lineHeight, 100, 50), "upperarm_l.eulerAngles.y + armAimAnimationRotY", styleGreen);
		GUI.Box(new Rect(750, 13 * lineHeight, 100, 50), playerModel.Upperarm_l.eulerAngles.y + playerModel.Arm_Aim_Animation_Rot_Y.ToString(), styleGreen);

#endif //終了  
	}
}

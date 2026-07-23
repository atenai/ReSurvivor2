using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// プレイヤーを管理するマネージャークラス
/// </summary>
public class PlayerManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static PlayerManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static PlayerManager SingletonInstance => singletonInstance;

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

	[Header("プレイヤーMVPパターン")]
	/// <summary>プレイヤーモデル</summary>
	[SerializeField]
	PlayerModel playerModel = new PlayerModel();
	public PlayerModel PlayerModel => playerModel;

	[Tooltip("プレイヤービュー")]
	[SerializeField] PlayerView playerView;
	public PlayerView PlayerView => playerView;

	[Tooltip("プレイヤーUI")]
	[SerializeField] PlayerUI playerUI;
	public PlayerUI PlayerUI => playerUI;

	[Header("ガンモデル")]
	[Tooltip("ガンモデルファサード")]
	[SerializeField] GunModelFacade gunModelFacade = new GunModelFacade();
	public GunModelFacade GunModelFacade => gunModelFacade;

	[Header("リスポーンポイント")]
	[Tooltip("プレイヤーのリスポーンポイントの位置")]
	Vector3 respawnPosition = new Vector3(0.0f, 1.0f, 0.0f);
	[Tooltip("プレイヤーのリスポーンポイントの回転")]
	Quaternion respawnRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

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
		playerModel.Save();
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

		//Debug.Log("<color=purple>プレイヤーロード</color>");
		playerModel.Load();

		currentMine = ES3.Load<int>("Mine", 3);
		//Debug.Log("<color=purple>地雷 : " + currentMine + "</color>");

		//ステージが切り替わる度にリスポーン位置が呼ばれるため、リスポーンのオブジェクトは遷移先のステージで敵やオブジェクトにかぶらないように注意すること！！
		if (ES3.KeyExists("PlayerPos") == true)
		{
			respawnPosition = ES3.Load<Vector3>("PlayerPos");
			//Debug.Log("<color=purple>プレイヤー位置 : " + this.transform.position + "</color>");
		}
		if (ES3.KeyExists("PlayerRot") == true)
		{
			respawnRotation = ES3.Load<Quaternion>("PlayerRot");
			//Debug.Log("<color=purple>プレイヤー回転 : " + this.transform.rotation + "</color>");
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
		InitStamina();
		StartDamageEffect();
		StartHpHealEffect();
		StartStaminaHealEffect();
		InitMine();
		InitRespawnPos();

		playerUI.InitHP(playerModel.HP.CurrentHp);
		playerUI.InitStamina(playerModel.Stamina.CurrentStamina);
		playerUI.StartTextArmorPlate(playerModel.CurrentArmorPlate);
		playerUI.StartTextFood(playerModel.CurrentFood);
		playerUI.SetTextMagazine(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine);
		playerUI.SetTextAmmo(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo);
	}

	public void ResetMove()
	{
		rb.velocity = Vector3.zero;
		playerModel.ResetMove();
		playerView.ResetMoveAnimation();
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	void InitHP()
	{
		playerModel.HP.Initialize(DamageEffect, ChangeSceneManager.SingletonInstance.GameOver, () => playerModel.UseArmorPlate((armorPlate) => playerUI.StartTextArmorPlate(armorPlate)), HealEffect);
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", PlayerModel.HP.CurrentHp / HitPoint.Max_Hp);
	}

	/// <summary>
	/// スタミナの初期化処理
	/// </summary>
	void InitStamina()
	{
		playerModel.Stamina.Initialize(ConsumeStaminaEffect, () => playerModel.UseFood((food) => playerUI.SetTextFood(food)), RestoresStaminaEffect);
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

	public void AfterUpdate()
	{
		playerModel.AfterUpdate();

		if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("XInput RB"))
		{
			playerModel.Heal();
		}

		if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Q) || Input.GetButtonDown("XInput LB"))
		{
			playerModel.RestoresStamina();
		}

		Mine();

		playerUI.SetTextMagazine(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine);
		playerUI.SetTextAmmo(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo);
	}

	/// <summary>
	/// 地雷の設置
	/// </summary>
	void Mine()
	{
		//押しているかを判定
		if (Input.GetKey(KeyCode.Alpha4) || Input.GetKey(KeyCode.G) || XInputManager.SingletonInstance.XInputDPadHandler.UpHold)
		{
			PlacingMine();
		}

		//離した瞬間を判定
		if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.G) || XInputManager.SingletonInstance.XInputDPadHandler.UpUp)
		{
			PlacedMine();
		}
	}

	/// <summary>
	/// 地雷を設置中
	/// </summary>
	void PlacingMine()
	{
		mineSpawnCount = mineSpawnCount + Time.deltaTime;
		playerUI.SetMinePlacingFillAmount(mineSpawnCount / Mine_Hold_Time);
		Debug.Log("押しているフレーム数 : " + mineSpawnCount);
	}

	/// <summary>
	/// 地雷を設置した
	/// </summary>
	void PlacedMine()
	{
		//長押しを判定
		if (Mine_Hold_Time <= mineSpawnCount)
		{
			Debug.Log("長押し");
			CreateMine();
		}

		mineSpawnCount = 0.0f;
		playerUI.SetMinePlacingFillAmount(0.0f);
	}

	/// <summary>
	/// 地雷を生成
	/// </summary>
	void CreateMine()
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

	public void AfterFixedUpdate()
	{
		playerModel.ChangeMoveSpeed();
		NormalMove();
		AimMove();
	}

	/// <summary>
	/// 通常状態移動
	/// </summary>
	void NormalMove()
	{
		if (playerModel.IsAim == true)
		{
			return;
		}

		//カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
		playerModel.CameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

		//方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
		playerModel.MoveForward = playerModel.CameraForward * playerModel.InputVertical + Camera.main.transform.right * playerModel.InputHorizontal;
		//移動方向のベクトルを正規化
		playerModel.MoveForward.Normalize();

		//移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
		rb.velocity = playerModel.MoveForward * playerModel.NormalMoveSpeed + new Vector3(0, rb.velocity.y, 0);

		//キャラクターの向きをキャラクターの進行方向にする
		if (playerModel.MoveForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
		{
			this.transform.rotation = Quaternion.LookRotation(playerModel.MoveForward);
		}

		if (playerModel.IsDash == true)
		{
			if (playerModel.InputVertical <= -0.1f || 0.1f <= playerModel.InputVertical)//前後移動入力
			{
				//スタミナ消費
				playerModel.Stamina.ConsumeStamina(2.0f);
			}
			else if (playerModel.InputHorizontal <= -0.1f || 0.1f <= playerModel.InputHorizontal)//左右移動入力
			{
				//スタミナ消費
				playerModel.Stamina.ConsumeStamina(2.0f);
			}
			else
			{
				playerModel.IsDash = false;
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
		if (playerModel.IsAim == false)
		{
			return;
		}

		//カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
		playerModel.CameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

		//方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
		playerModel.MoveForward = playerModel.CameraForward * playerModel.InputVertical + Camera.main.transform.right * playerModel.InputHorizontal;
		//移動方向のベクトルを正規化
		playerModel.MoveForward.Normalize();

		//移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
		rb.velocity = playerModel.MoveForward * playerModel.WeaponMoveSpeed + new Vector3(0, rb.velocity.y, 0);

		//キャラクターの向きをカメラの前方にする
		if (playerModel.CameraForward != Vector3.zero)//向きベクトルがある場合は中身を実行する
		{
			this.transform.rotation = Quaternion.LookRotation(playerModel.CameraForward);
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
	void OnCollisionEnter(Collision collision)
	{
		// 	Debug.Log("<color=red>当たった！ : " + collision.gameObject.name + "</color>");

		// if (collision.collider.tag == "Enemy" || collision.collider.tag == "FlyingEnemy" || collision.collider.tag == "GroundEnemy")
		// {
		// 	hp.Damage(10.0f);
		// 	CameraShaker();
		// }
	}

	/// <summary>
	/// カメラを揺さぶる
	/// </summary>
	void CameraShaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}

	/// <summary>
	/// ダメージエフェクト
	/// </summary>
	public void DamageEffect()
	{
		playerUI.SliderHp.value = (float)playerModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
		isDamage = true;
	}

	/// <summary>
	/// 回復エフェクト
	/// </summary>
	public void HealEffect()
	{
		playerUI.SliderHp.value = (float)playerModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
		isHpHeal = true;
	}

	/// <summary>
	/// スタミナを消費エフェクト
	/// </summary> 
	void ConsumeStaminaEffect()
	{
		playerUI.SliderStamina.value = (float)playerModel.Stamina.CurrentStamina / (float)Stamina.Max_Stamina;
	}

	/// <summary>
	/// スタミナを回復エフェクト
	/// </summary>
	void RestoresStaminaEffect()
	{
		playerUI.SliderStamina.value = (float)playerModel.Stamina.CurrentStamina / (float)Stamina.Max_Stamina;
		isStaminaHeal = true;
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
		GUI.Box(new Rect(350, 0 * lineHeight, 100, 50), playerModel.InputHorizontal.ToString(), styleGreen);
		GUI.Box(new Rect(10, 1 * lineHeight, 100, 50), "inputVertical", styleGreen);
		GUI.Box(new Rect(350, 1 * lineHeight, 100, 50), playerModel.InputVertical.ToString(), styleGreen);
		GUI.Box(new Rect(10, 2 * lineHeight, 100, 50), "normalMoveSpeed", styleGreen);
		GUI.Box(new Rect(350, 2 * lineHeight, 100, 50), playerModel.NormalMoveSpeed.ToString(), styleGreen);
		GUI.Box(new Rect(10, 3 * lineHeight, 100, 50), "weaponMoveSpeed", styleGreen);
		GUI.Box(new Rect(350, 3 * lineHeight, 100, 50), playerModel.WeaponMoveSpeed.ToString(), styleGreen);
		GUI.Box(new Rect(10, 4 * lineHeight, 100, 50), "isDash", styleGreen);
		GUI.Box(new Rect(350, 4 * lineHeight, 100, 50), playerModel.IsDash.ToString(), styleGreen);
		GUI.Box(new Rect(10, 5 * lineHeight, 100, 50), "rb.velocity", styleGreen);
		GUI.Box(new Rect(350, 5 * lineHeight, 100, 50), rb.velocity.ToString(), styleGreen);
		GUI.Box(new Rect(10, 6 * lineHeight, 100, 50), "moveForward", styleGreen);
		GUI.Box(new Rect(350, 6 * lineHeight, 100, 50), playerModel.MoveForward.ToString(), styleGreen);
		GUI.Box(new Rect(10, 7 * lineHeight, 100, 50), "cameraForward", styleGreen);
		GUI.Box(new Rect(350, 7 * lineHeight, 100, 50), playerModel.CameraForward.ToString(), styleGreen);
		GUI.Box(new Rect(10, 8 * lineHeight, 100, 50), "isAim", styleGreen);
		GUI.Box(new Rect(350, 8 * lineHeight, 100, 50), playerModel.IsAim.ToString(), styleGreen);
		GUI.Box(new Rect(10, 9 * lineHeight, 100, 50), "spine_03.eulerAngles", styleGreen);
#endif //終了  
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// プレイヤーを管理するマネージャークラス
/// MVPパターンのPresenter担当
/// </summary>
public class PlayerManagerPresenter : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static PlayerManagerPresenter singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static PlayerManagerPresenter SingletonInstance => singletonInstance;

	[Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
	static bool isFirstLoad = true;
	public static bool IsFirstLoad
	{
		get { return isFirstLoad; }
		set { isFirstLoad = value; }
	}

	[Header("プレイヤーMVPパターン")]
	/// <summary>プレイヤーモデル</summary>
	PlayerModel playerModel = new PlayerModel();
	public PlayerModel PlayerModel => playerModel;

	[Tooltip("プレイヤービュー")]
	[SerializeField] PlayerCharacterView playerCharacterView;
	public PlayerCharacterView PlayerCharacterView => playerCharacterView;

	[Tooltip("プレイヤーUI")]
	[SerializeField] PlayerUIView playerUIView;
	public PlayerUIView PlayerUIView => playerUIView;

	[Tooltip("リジッドボディ")]
	[SerializeField] Rigidbody rb;

	[Tooltip("カメラの揺れ")]
	[SerializeField] CinemachineImpulseSource cinemachineImpulseSource;
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
	}

	void Start()
	{
		InitHP();
		InitStamina();
		InitMine();
		InitRespawnPos();

		playerUIView.InitHP(playerModel.HP.CurrentHp);
		playerUIView.InitStamina(playerModel.Stamina.CurrentStamina);
		playerUIView.StartTextArmorPlate(playerModel.CurrentArmorPlate);
		playerUIView.StartTextFood(playerModel.CurrentFood);
		playerUIView.SetTextMagazine(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine);
		playerUIView.SetTextAmmo(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo);
		playerUIView.UpdateUITransform(playerModel.IsAim);
	}

	public void ResetMove()
	{
		rb.velocity = Vector3.zero;
		playerModel.ResetMove();
		playerCharacterView.ResetMoveAnimation();
	}

	/// <summary>
	/// HPの初期化処理
	/// </summary>
	void InitHP()
	{
		playerModel.HP.Initialize(DamageEffect, ChangeSceneManager.SingletonInstance.GameOver, () => playerModel.UseArmorPlate((armorPlate) => playerUIView.StartTextArmorPlate(armorPlate)), HealEffect);
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", PlayerModel.HP.CurrentHp / HitPoint.Max_Hp);
	}

	/// <summary>
	/// スタミナの初期化処理
	/// </summary>
	void InitStamina()
	{
		playerModel.Stamina.Initialize(ConsumeStaminaEffect, () => playerModel.UseFood((food) => playerUIView.SetTextFood(food)), RestoresStaminaEffect);
	}

	/// <summary>
	/// 地雷の初期化
	/// </summary> 
	void InitMine()
	{
		playerModel.MineSpawnCount = 0.0f;
		playerUIView.TextMine.text = playerModel.CurrentMine.ToString();
	}

	/// <summary>
	/// リスポーン座標
	/// </summary>
	void InitRespawnPos()
	{
		this.transform.position = playerModel.RespawnPosition;
		this.transform.rotation = playerModel.RespawnRotation;
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

		NormalMoveAnimation();
		playerUIView.UpdateUITransform(playerModel.IsAim);
		playerUIView.UpdateImageReload(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.IsReloadTimeActive);
		playerUIView.SetTextMagazine(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentMagazine);
		playerUIView.SetTextAmmo(PlayerCameraManager.SingletonInstance.GetGunFacade.GetGunBase.CurrentAmmo);
	}


	/// <summary>
	/// 移動アニメーション
	/// </summary>
	void NormalMoveAnimation()
	{
		playerCharacterView.Animator.SetFloat("f_moveSpeedX", playerModel.InputHorizontal);
		playerCharacterView.Animator.SetFloat("f_moveSpeedY", playerModel.InputVertical);
		playerCharacterView.Animator.SetBool("b_isAim", playerModel.IsAim);
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
		playerModel.MineSpawnCount = playerModel.MineSpawnCount + Time.deltaTime;
		playerUIView.SetMinePlacingFillAmount(playerModel.MineSpawnCount / PlayerModel.Mine_Hold_Time);
		Debug.Log("押しているフレーム数 : " + playerModel.MineSpawnCount);
	}

	/// <summary>
	/// 地雷を設置した
	/// </summary>
	void PlacedMine()
	{
		//長押しを判定
		if (PlayerModel.Mine_Hold_Time <= playerModel.MineSpawnCount)
		{
			Debug.Log("長押し");
			CreateMine();
		}

		playerModel.MineSpawnCount = 0.0f;
		playerUIView.SetMinePlacingFillAmount(0.0f);
	}

	/// <summary>
	/// 地雷を生成
	/// </summary>
	void CreateMine()
	{
		if (playerModel.CurrentMine <= 0)
		{
			return;
		}

		playerModel.CurrentMine = playerModel.CurrentMine - 1;
		playerUIView.TextMine.text = playerModel.CurrentMine.ToString();

		Vector3 localPosition = new Vector3(this.transform.position.x, 0.0f, this.transform.position.z);
		// キャラクターの前方にオブジェクトを生成
		Vector3 spawnPosition = localPosition + (this.transform.forward * playerModel.MineSpawnDistance);
		GameObject localGameObject = UnityEngine.Object.Instantiate(playerCharacterView.MinePrefab, spawnPosition, this.transform.rotation);
	}

	/// <summary>
	/// 地雷を取得
	/// </summary> 
	public void AcquireMine()
	{
		if (playerModel.MaxMine <= playerModel.CurrentMine)
		{
			return;
		}

		playerModel.CurrentMine = playerModel.CurrentMine + 1;
		playerUIView.TextMine.text = playerModel.CurrentMine.ToString();
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

	void LateUpdate()
	{
		//ボーンを曲げる際は必ずLateUpdateに書く必要がある！（これいつかメモする！）
		playerCharacterView.RotateBoneNeck01(playerModel.IsAim);
		playerCharacterView.RotateBoneSpine03(playerModel.IsAim, PlayerCameraManager.SingletonInstance.transform);
		playerCharacterView.RotateBoneUpperArmR(playerModel.IsAim);
		playerCharacterView.RotateBoneUpperArmL(playerModel.IsAim);
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
		playerUIView.SliderHp.value = (float)playerModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
		playerModel.IsDamage = true;
	}

	/// <summary>
	/// 回復エフェクト
	/// </summary>
	public void HealEffect()
	{
		playerUIView.SliderHp.value = (float)playerModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
		//シェーダーへ値を渡す（これだけでOK）
		Shader.SetGlobalFloat("HP", playerModel.HP.CurrentHp / HitPoint.Max_Hp);
		playerModel.IsHpHeal = true;
	}

	/// <summary>
	/// スタミナを消費エフェクト
	/// </summary> 
	void ConsumeStaminaEffect()
	{
		playerUIView.SliderStamina.value = (float)playerModel.Stamina.CurrentStamina / (float)Stamina.Max_Stamina;
	}

	/// <summary>
	/// スタミナを回復エフェクト
	/// </summary>
	void RestoresStaminaEffect()
	{
		playerUIView.SliderStamina.value = (float)playerModel.Stamina.CurrentStamina / (float)Stamina.Max_Stamina;
		playerModel.IsStaminaHeal = true;
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

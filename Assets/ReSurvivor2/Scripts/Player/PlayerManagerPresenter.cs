using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

/// <summary>
/// プレイヤーを管理するマネージャープレゼンタークラス
/// MVPパターンのPresenter担当
/// ※プレイヤーのロジック（移動・地雷・HP・スタミナ）を動かす。プレイヤーの見た目（キャラクターのアニメーション・銃の見た目と音・プレイヤーUI）は知らない。
///   見た目の側（PlayerViewPresenter）は、ここの知らせと、プレイヤーの状態を見て動く（見た目の側を外しても、ゲームは動く）
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

	[Tooltip("地雷のプレファブ")]
	[SerializeField] GameObject minePrefab;

	[Tooltip("リジッドボディ")]
	[SerializeField] Rigidbody rb;

	[Tooltip("カメラの揺れ")]
	[SerializeField] CinemachineImpulseSource cinemachineImpulseSource;
	public CinemachineImpulseSource CinemachineImpulseSource => cinemachineImpulseSource;

	/// <summary>
	/// プレイヤーの更新処理が終わった時の知らせ（移動の入力・構え・地雷を置く動きの状態が決まった後）
	/// </summary>
	public event UnityAction OnAfterUpdate;

	/// <summary>
	/// 銃の更新処理が終わった時の知らせ
	/// </summary>
	public event UnityAction OnAfterGunUpdate;

	/// <summary>
	/// LateUpdateの知らせ（アニメーションで骨が動いた後。ボーンを曲げる時に使う）
	/// </summary>
	public event UnityAction OnLateUpdate;

	/// <summary>
	/// 移動の状態をリセットした時の知らせ（ロード中・コンピューターを使っている間・ステージを移る時）
	/// </summary>
	public event UnityAction OnMoveReset;

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
		InitHPAndStamina();
		InitMine();
		InitRespawnPos();
	}

	public void ResetMove()
	{
		rb.velocity = Vector3.zero;
		playerModel.ResetMove();
		//ハンドガンに持ち替えている時は、メインの銃を持っている状態に戻す（持ち替えたまま次のステージに移らないようにする）
		PlayerCameraManager.SingletonInstance.GunModelFacade.CancelSideArm();
		//武器を切り替えている時は、切り替える先の銃を持っている状態にする（銃をしまう途中のまま次のステージに移らないようにする）
		PlayerCameraManager.SingletonInstance.GunModelFacade.CancelWeaponSwitch();
		OnMoveReset?.Invoke();
	}

	/// <summary>
	/// HPとスタミナの初期化処理
	/// </summary>
	void InitHPAndStamina()
	{
		//シーン切り替え中は無敵にする（切り替え中に死亡してゲームオーバー画面が割り込まないようにする）
		playerModel.InitializeHPAndStamina(ChangeSceneManager.SingletonInstance.GameOver, () => SceneLoadManager.IsChangingScene);
	}

	/// <summary>
	/// 地雷の初期化
	/// </summary> 
	void InitMine()
	{
		playerModel.MineSpawnCount = 0.0f;
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

		OnAfterUpdate?.Invoke();
	}

	/// <summary>
	/// 銃を更新した後の処理（銃の更新が終わったことを知らせる。見た目の側は、銃の状態に合わせてリロードのアニメーションを動かす）
	/// ※InGameManagerが、銃の更新（PlayerCameraManager.AfterUpdate）の後に呼ぶ
	/// </summary>
	public void AfterGunUpdate()
	{
		OnAfterGunUpdate?.Invoke();
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

		StandUpFromMine();
	}

	/// <summary>
	/// 地雷を設置中（ボタンを押している間、しゃがんで左手を地面へ下ろしていく）
	/// </summary>
	void PlacingMine()
	{
		//地雷を置いた後（置くのをやめた後）に立ち上がっている間は、次の地雷を置き始めない
		if (playerModel.MinePlacePhase == EnumManager.MinePlacePhaseTYPE.StandUp || playerModel.MinePlacePhase == EnumManager.MinePlacePhaseTYPE.Cancel)
		{
			return;
		}

		//地雷を持っていない時と、リロード中と、ハンドガンに持ち替えている間と、武器を切り替えている間は、地雷を置き始めない
		//（リロードやハンドガンの持ち替えや武器の切り替えの上半身の動きと、しゃがむ動きが重ならないようにする）
		if (playerModel.MinePlacePhase == EnumManager.MinePlacePhaseTYPE.None)
		{
			GunModelFacade gunModelFacade = PlayerCameraManager.SingletonInstance.GunModelFacade;
			if (playerModel.CurrentMine <= 0 || gunModelFacade.GunModelBase.IsReloadTimeActive == true || gunModelFacade.IsSideArmActive == true || gunModelFacade.IsWeaponSwitching == true)
			{
				return;
			}
		}

		playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.Crouch;
		playerModel.MineSpawnCount = playerModel.MineSpawnCount + Time.deltaTime;
		Debug.Log("押しているフレーム数 : " + playerModel.MineSpawnCount);
	}

	/// <summary>
	/// 地雷を設置した（ボタンを離した）
	/// </summary>
	void PlacedMine()
	{
		if (playerModel.MinePlacePhase != EnumManager.MinePlacePhaseTYPE.Crouch)
		{
			return;
		}

		//長押しを判定
		if (PlayerModel.Mine_Hold_Time <= playerModel.MineSpawnCount)
		{
			Debug.Log("長押し");
			CreateMine();
			//地雷を置いたので、立ち上がる
			playerModel.MineSpawnCount = 0.0f;
			playerModel.MineStandUpCount = 0.0f;
			playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.StandUp;
		}
		else
		{
			//長押しの途中で離したので、地雷を置くのをやめて、しゃがんだ分だけ戻す（長押しした時間を、同じ速さで0まで戻す）
			playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.Cancel;
		}
	}

	/// <summary>
	/// 地雷を置いた後（置くのをやめた後）に立ち上がる
	/// </summary>
	void StandUpFromMine()
	{
		switch (playerModel.MinePlacePhase)
		{
			case EnumManager.MinePlacePhaseTYPE.Crouch:
				//しゃがんでいる途中でリロードが始まった時は、地雷を置くのをやめる
				//（長押しを続けて手が地面に着いた後なら、手が地面に着くまでの時間だけかけて戻す）
				if (PlayerCameraManager.SingletonInstance.GunModelFacade.GunModelBase.IsReloadTimeActive == true)
				{
					playerModel.MineSpawnCount = Mathf.Min(playerModel.MineSpawnCount, PlayerModel.Mine_Hold_Time);
					playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.Cancel;
				}
				break;
			case EnumManager.MinePlacePhaseTYPE.StandUp:
				playerModel.MineStandUpCount = playerModel.MineStandUpCount + Time.deltaTime;
				if (PlayerModel.Mine_Stand_Up_Time <= playerModel.MineStandUpCount)
				{
					playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.None;
				}
				break;
			case EnumManager.MinePlacePhaseTYPE.Cancel:
				playerModel.MineSpawnCount = playerModel.MineSpawnCount - Time.deltaTime;
				if (playerModel.MineSpawnCount <= 0.0f)
				{
					playerModel.MineSpawnCount = 0.0f;
					playerModel.MinePlacePhase = EnumManager.MinePlacePhaseTYPE.None;
				}
				break;
		}
	}

	/// <summary>
	/// 地雷を生成
	/// </summary>
	void CreateMine()
	{
		if (playerModel.UseMine() == false)
		{
			return;
		}

		Vector3 localPosition = new Vector3(this.transform.position.x, 0.0f, this.transform.position.z);
		// キャラクターの前方にオブジェクトを生成
		Vector3 spawnPosition = localPosition + (this.transform.forward * playerModel.MineSpawnDistance);
		GameObject localGameObject = UnityEngine.Object.Instantiate(minePrefab, spawnPosition, this.transform.rotation);
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
		//地雷を置いている間は、その場で止まる
		if (playerModel.IsPlacingMine == true)
		{
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
			return;
		}

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
		//（移動スティックを少しだけ倒した時は、歩く速さにする）
		rb.velocity = playerModel.MoveForward * playerModel.NormalMoveSpeed * playerModel.MoveRate + new Vector3(0, rb.velocity.y, 0);

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

		//※カメラの向きは、描画するカメラ（Camera.main）ではなく、カメラを回している土台（PlayerCameraManager）から取る
		//  （描画するカメラは、Cinemachineがフレームの最後に土台の向きを写すので、ここではまだ回す前の向きのまま。
		//    それに合わせると、カメラを回している間、プレイヤーが描画するカメラより遅れたり追いついたりして、ガクガクする）
		Transform cameraTransform = PlayerCameraManager.SingletonInstance.transform;

		//カメラの方向から、XとZのベクトルを取得 0,0,1 * 1,0,1 = 1,0,1;
		playerModel.CameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;

		//方向キーの入力値とカメラの向きから、移動方向のベクトルを算出する
		playerModel.MoveForward = playerModel.CameraForward * playerModel.InputVertical + cameraTransform.right * playerModel.InputHorizontal;
		//移動方向のベクトルを正規化
		playerModel.MoveForward.Normalize();

		//移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
		//（移動スティックを少しだけ倒した時は、歩く速さにする）
		rb.velocity = playerModel.MoveForward * playerModel.WeaponMoveSpeed * playerModel.MoveRate + new Vector3(0, rb.velocity.y, 0);

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
		//（見た目の側が、ここの知らせを受けて、構えに合わせて背骨を曲げる）
		OnLateUpdate?.Invoke();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// プレイヤーカメラ
/// </summary> 
public class PlayerCamera : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static PlayerCamera singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static PlayerCamera SingletonInstance => singletonInstance;

	[Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
	static bool isFirstLoad = true;
	public static bool IsFirstLoad
	{
		get { return isFirstLoad; }
		set { isFirstLoad = value; }
	}

	[Header("カメラ")]

	[Tooltip("子のメインカメラ")]
	[SerializeField] CinemachineVirtualCamera childMainDashMoveVirtualCamera;

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
	[Tooltip("カメラのスピードを少し遅くする")]
	[Range(1.0f, 4.0f)] float littleSlowDownCameraSpeed = 1.5f;

	[Tooltip("通常カメラのy位置")]
	const float normalUpPos = 1.6f;
	public float NormalUpPos => normalUpPos;
	[Tooltip("通常カメラのz位置")]
	const float normalForwardPos = -3.5f;

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
	[Tooltip("レイの長さ")]
	[SerializeField] float raycastRange = 100.0f;
	public float RaycastRange => raycastRange;
	[Tooltip("スフィアレイキャストの半径")]
	[Range(0.5f, 1.0f)] float sphereRayCastRadius = 1.0f;

#if UNITY_EDITOR//Unityエディター上での処理
	[Tooltip("ヒットしたオブジェクトの名前")]
	string hitName = "";
	public string HitName
	{
		get { return hitName; }
		set { hitName = value; }
	}
#endif //終了  

	[SerializeField] GunFacade gunFacade = new GunFacade();
	public GunFacade GetGunFacade => gunFacade;

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

		Load();
	}

	/// <summary>
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>プレイヤーカメラセーブ</color>");
		gunFacade.Save();
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

		Debug.Log("<color=purple>プレイヤーカメラロード</color>");
		gunFacade.Load();
	}

	void Start()
	{

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
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		//シネマシーンカメラがアクティブの場合は切り上げる
		if (isCinemachineActive == true)
		{
			return;
		}

		gunFacade.UpdateGun();
	}

	void FixedUpdate()
	{
		//シネマシーンカメラがアクティブの場合は切り上げる
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

		if (Player.SingletonInstance.IsDash == true)
		{
			//徐々に子カメラをダッシュ時の位置にする
			childMainDashMoveVirtualCamera.Priority = 200;
		}
		else
		{
			//徐々に子カメラを通常時の位置にする
			childMainDashMoveVirtualCamera.Priority = 10;
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
		float x_Rotation = Input.GetAxis("Mouse X") + Input.GetAxis("XInput R_Stick_Left&Right");
		float y_Rotation = Input.GetAxis("Mouse Y") + Input.GetAxis("XInput R_Stick_Up&Down");

		if (Player.SingletonInstance.IsAim == true)
		{
			localCameraSpeedX = aimCameraSpeedX;
			localCameraSpeedY = aimCameraSpeedY;
			isTargethit = false;

			//ターゲットにあたった際にカメラを少し遅くする処理(スフィアレイキャスト)
			Debug.DrawRay(this.transform.position, this.transform.forward * raycastRange, Color.yellow, 1.0f);
			RaycastHit sphereHit;
			if (Physics.SphereCast(this.transform.position, sphereRayCastRadius, this.transform.forward, out sphereHit, raycastRange) == true)
			{
				if (sphereHit.collider.gameObject.CompareTag("Enemy") || sphereHit.collider.gameObject.CompareTag("FlyingEnemy") || sphereHit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
				{
					//カメラのスピードを少し遅くする
					localCameraSpeedX = aimCameraSpeedX / littleSlowDownCameraSpeed;
					localCameraSpeedY = aimCameraSpeedY / littleSlowDownCameraSpeed;
				}
			}

			//ターゲットにあたった際にカメラを遅くする処理(通常のレイキャスト)
			Ray ray = new Ray(this.transform.position, this.transform.forward);
			Debug.DrawRay(ray.origin, ray.direction * raycastRange, Color.red, 1.0f);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, raycastRange) == true) // もしRayを投射して何らかのコライダーに衝突したら
			{
				if (hit.collider.gameObject.CompareTag("Enemy") || hit.collider.gameObject.CompareTag("FlyingEnemy") || hit.collider.gameObject.CompareTag("GroundEnemy"))//※間違ってオブジェクトの設定にレイヤーとタグを間違えるなよおれｗ
				{
					//カメラのスピードを遅くする
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
					if (y_Rotation < 0)
					{
						//マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
						this.transform.RotateAround(Player.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
					}
				}
				else
				{
					if (0 < y_Rotation)
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

		int lineHeight = 50;

		GUI.Box(new Rect(10, 14 * lineHeight, 100, 50), "cameraPos", styleRed);
		GUI.Box(new Rect(350, 14 * lineHeight, 100, 50), this.transform.position.ToString(), styleRed);
		GUI.Box(new Rect(10, 15 * lineHeight, 100, 50), "hitName", styleRed);
		GUI.Box(new Rect(350, 15 * lineHeight, 100, 50), hitName, styleRed);
		GUI.Box(new Rect(10, 16 * lineHeight, 100, 50), "localCameraSpeedX", styleRed);
		GUI.Box(new Rect(350, 16 * lineHeight, 100, 50), localCameraSpeedX.ToString(), styleRed);
		GUI.Box(new Rect(10, 17 * lineHeight, 100, 50), "localCameraSpeedY", styleRed);
		GUI.Box(new Rect(350, 17 * lineHeight, 100, 50), localCameraSpeedY.ToString(), styleRed);

#endif //終了  
	}
}

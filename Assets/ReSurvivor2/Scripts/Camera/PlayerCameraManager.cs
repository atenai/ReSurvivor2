using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/// <summary>
/// プレイヤーカメラを管理するマネージャークラス
/// </summary> 
public class PlayerCameraManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static PlayerCameraManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static PlayerCameraManager SingletonInstance => singletonInstance;

	[Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
	static bool isFirstLoad = true;
	public static bool IsFirstLoad
	{
		get { return isFirstLoad; }
		set { isFirstLoad = value; }
	}

	[Header("カメラ")]

	[Tooltip("子の近距離カメラ")]
	[SerializeField] CinemachineVirtualCamera childMainShortDistanceVirtualCamera;

	[Tooltip("子の中距離カメラ")]
	[SerializeField] CinemachineVirtualCamera childMainMidDistanceVirtualCamera;

	[Tooltip("子の遠距離カメラ")]
	[SerializeField] CinemachineVirtualCamera childMainLongDistanceVirtualCamera;

	[Tooltip("近距離カメラの高優先度")]
	int shortDistanceCameraHighPriority = 200;
	[Tooltip("近距離カメラの通常優先度")]
	int shortDistanceCameraNormalPriority = 50;
	[Tooltip("遠距離カメラの高優先度")]
	int longDistanceCameraHighPriority = 150;
	[Tooltip("遠距離カメラの通常優先度")]
	int longDistanceCameraNormalPriority = 10;

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
	bool isTargetHit = false;
	public bool IsTargetHit => isTargetHit;
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

	[Header("近距離の構え（C.A.R.システムのような構え）")]
	[Tooltip("狙っている敵がこの距離（m）以内に入ったら、近距離の構えにする（プレイヤーから、スフィアキャストが敵に当たった位置までの水平の距離）")]
	const float CloseQuarterEnterDistance = 3.0f;
	[Tooltip("近距離の構えをやめる距離（m）（入る距離より少し遠くして、境目で構えが行ったり来たりしないようにする）")]
	const float CloseQuarterExitDistance = 3.5f;
	[Tooltip("狙っている方向に近い敵がいるかを調べる、スフィアキャストの半径（m）（細いレイだと、敵の腕と体のすき間などを通り抜けて、構えが行ったり来たりするため）")]
	const float CloseQuarterCheckRadius = 0.3f;
	[Tooltip("近い敵を調べるスフィアキャストの当たりを入れておく配列の大きさ（足りない時は、全部の当たりを取り直す）")]
	const int CloseQuarterHitBufferSize = 16;
	[Tooltip("近い敵を調べるスフィアキャストの当たりを入れておく配列（毎回新しい配列を作らないように使い回す）")]
	readonly RaycastHit[] closeQuarterHitBuffer = new RaycastHit[CloseQuarterHitBufferSize];
	[Tooltip("スフィアキャストを始めた時から球に重なっていたコライダーの、当たった距離（当たった位置がわからないので数えない）")]
	const float OverlapAtStartDistance = 0.0f;
	[Tooltip("狙っている方向の一番手前に、近い敵がいるか？（近い時は、ハンドガンを構えると近距離の構えにする）")]
	bool isEnemyClose = false;
	public bool IsEnemyClose => isEnemyClose;

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

		//Debug.Log("<color=purple>プレイヤーカメラロード</color>");
		gunFacade.Load();
	}

	public void AfterUpdate()
	{
		//シネマシーンカメラがアクティブの場合は切り上げる
		if (isCinemachineActive == true)
		{
			return;
		}

		gunFacade.UpdateGun(PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim);
		//銃の状態に合わせて、プレイヤーのリロードのアニメーションを動かす（銃のクラスはプレイヤーのアニメーションを知らないので、銃を更新した後にここで呼ぶ）
		PlayerManagerPresenter.SingletonInstance.PlayerCharacterView.UpdateReloadAnimation(gunFacade);
	}

	public void AlwaysFixedUpdate()
	{
		//シネマシーンカメラがアクティブの場合は切り上げる
		if (isCinemachineActive == true)
		{
			return;
		}

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim == false)
		{
			CameraNormalMove();
		}
		else if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim == true)
		{
			CameraAimMove();
		}

		CameraDash();

		CameraOcclusion();
	}


	public void AfterFixedUpdate()
	{
		CameraRot();
		UpdateIsEnemyClose();
	}

	/// <summary>
	/// 通常カメラ
	/// </summary>
	void CameraNormalMove()
	{
		//通常カメラ位置をプレイヤーの座標位置から計算
		Vector3 cameraPos = PlayerManagerPresenter.SingletonInstance.transform.position + (Vector3.up * normalUpPos) + (this.transform.forward * normalForwardPos);
		//カメラの位置を移動させる
		this.transform.position = Vector3.Lerp(transform.position, cameraPos, PlayerManagerPresenter.SingletonInstance.PlayerModel.NormalMoveSpeed * 10 * Time.deltaTime);
	}

	/// <summary>
	/// 肩越しカメラ
	/// </summary>
	void CameraAimMove()
	{
		//肩越しカメラ位置をプレイヤーの座標位置から計算
		Vector3 cameraPos = PlayerManagerPresenter.SingletonInstance.transform.position + (PlayerManagerPresenter.SingletonInstance.transform.right * aimRightPos) + (Vector3.up * aimUpPos) + (this.transform.forward * aimForwardPos);
		//カメラの位置を移動させる
		this.transform.position = Vector3.Lerp(transform.localPosition, cameraPos, PlayerManagerPresenter.SingletonInstance.PlayerModel.WeaponMoveSpeed * 10 * Time.deltaTime);
	}

	/// <summary>
	/// カメラの回転
	/// </summary> 
	void CameraRot()
	{
		// マウスの移動量を取得
		float x_Rotation = Input.GetAxis("Mouse X") + Input.GetAxis("XInput R_Stick_Left&Right");
		float y_Rotation = Input.GetAxis("Mouse Y") + Input.GetAxis("XInput R_Stick_Up&Down");

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim == true)
		{
			localCameraSpeedX = aimCameraSpeedX;
			localCameraSpeedY = aimCameraSpeedY;
			isTargetHit = false;

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
					isTargetHit = true;
				}
			}
		}
		else if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim == false)
		{
			localCameraSpeedX = normalCameraSpeedX;
			localCameraSpeedY = normalCameraSpeedY;
			isTargetHit = false;
		}

		//Mathf.Absは絶対値を返す(例)Mathf.Abs(10)なら10、Mathf.Abs(-10)なら10と+だろうが-だろうがプラスの値を返す
		//RotateAround(中心となるワールド座標, 回転軸, 回転角度(度数))関数は、指定の座標を中心にオブジェクトを回転させる関数

		// X方向に一定量移動していれば横回転
		if (deadZoneX < Mathf.Abs(x_Rotation))
		{
			// 回転軸はワールド座標のY軸
			this.transform.RotateAround(PlayerManagerPresenter.SingletonInstance.transform.position, Vector3.up, x_Rotation * Time.deltaTime * localCameraSpeedX);
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
				this.transform.RotateAround(PlayerManagerPresenter.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
			}
			else
			{
				if (300 < cameraAngles)
				{
					if (y_Rotation < 0)
					{
						//マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
						this.transform.RotateAround(PlayerManagerPresenter.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
					}
				}
				else
				{
					if (0 < y_Rotation)
					{
						//マウスYの入力量 × カメラのスピード × 時間 = の値をY回転の量にする
						this.transform.RotateAround(PlayerManagerPresenter.SingletonInstance.transform.position, -transform.right, y_Rotation * Time.deltaTime * localCameraSpeedY);
					}

				}
			}
		}
	}

	/// <summary>
	/// ダッシュカメラ 
	/// </summary>
	void CameraDash()
	{
		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsDash == true)
		{
			//徐々に子カメラをダッシュ時の位置にする
			childMainLongDistanceVirtualCamera.Priority = longDistanceCameraHighPriority;
		}
		else
		{
			//徐々に子カメラを通常時の位置にする
			childMainLongDistanceVirtualCamera.Priority = longDistanceCameraNormalPriority;
		}
	}

	/// <summary>
	/// 閉塞カメラ
	/// </summary>
	void CameraOcclusion()
	{
		if (PlayerManagerPresenter.SingletonInstance == null)
		{
			return;
		}

		if (PlayerManagerPresenter.SingletonInstance.PlayerModel.IsAim == true)
		{
			childMainShortDistanceVirtualCamera.Priority = shortDistanceCameraNormalPriority;
			return;
		}

		Vector3 origin = childMainMidDistanceVirtualCamera.transform.position;
		Vector3 targetPosition = PlayerManagerPresenter.SingletonInstance.transform.position;
		Vector3 direction = targetPosition - origin;
		float distance = direction.magnitude;
		if (distance <= 0.001f)
		{
			return;
		}

		Ray ray = new Ray(origin, direction.normalized);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, distance) == true)
		{
			if (IsPlayerCollider(hit.collider) == false)
			{
				childMainShortDistanceVirtualCamera.Priority = shortDistanceCameraHighPriority;
				//Debug.Log("<color=cyan>カメラのオクルージョン発生</color>");
				return;
			}
		}

		childMainShortDistanceVirtualCamera.Priority = shortDistanceCameraNormalPriority;
	}

	/// <summary>
	/// 狙っている方向（カメラの正面）の一番手前にある物が近い敵かを調べて、ハンドガンを近距離の構え（C.A.R.システムのような構え）にするかを決める
	/// ※構えていない時も調べておく（構え始める前から近い敵に気づいておき、構えるのと一緒に近距離の構えにするため。
	///   構えてから気づくと、構え始めの片手で狙う構えが少し見えてから、近距離の構えに切り替わってしまう）
	/// ※構えていない時のカメラはプレイヤーの後ろにあるので、プレイヤー自身には当たらないことにする
	/// </summary>
	void UpdateIsEnemyClose()
	{
		int count = Physics.SphereCastNonAlloc(this.transform.position, CloseQuarterCheckRadius, this.transform.forward, closeQuarterHitBuffer, raycastRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
		RaycastHit[] hits = closeQuarterHitBuffer;
		//配列がいっぱいの時は一番手前の当たりが入っていない可能性があるので、全部取り直す
		if (CloseQuarterHitBufferSize <= count)
		{
			hits = Physics.SphereCastAll(this.transform.position, CloseQuarterCheckRadius, this.transform.forward, raycastRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
			count = hits.Length;
		}

		//一番手前の当たりを探す（プレイヤー自身と、弾道エフェクトと、始めから球に重なっていたコライダーは数えない）
		bool isHit = false;
		RaycastHit nearestHit = new RaycastHit();
		for (int i = 0; i < count; i++)
		{
			if (hits[i].distance <= OverlapAtStartDistance || IsPlayerCollider(hits[i].collider) == true || EnemyBullet.IsIgnoredCollider(hits[i].collider) == true)
			{
				continue;
			}

			if (isHit == false || hits[i].distance < nearestHit.distance)
			{
				nearestHit = hits[i];
				isHit = true;
			}
		}

		//一番手前が敵でない時（何にも当たらない時や、壁などにさえぎられている時）は、近くないことにする
		if (isHit == false || IsEnemyCollider(nearestHit.collider) == false)
		{
			isEnemyClose = false;
			return;
		}

		//今近い時は、やめる距離までは近いままにする（境目で構えが行ったり来たりしないようにする）
		isEnemyClose = GetHorizontalDistanceFromPlayer(nearestHit.point) <= (isEnemyClose == true ? CloseQuarterExitDistance : CloseQuarterEnterDistance);
	}

	/// <summary>
	/// 敵のコライダーか？
	/// </summary>
	/// <param name="collider">コライダー</param>
	/// <returns>敵のコライダーならtrue</returns>
	bool IsEnemyCollider(Collider collider)
	{
		return collider.CompareTag("Enemy") || collider.CompareTag("FlyingEnemy") || collider.CompareTag("GroundEnemy");
	}

	/// <summary>
	/// プレイヤーから、指定した位置までの水平の距離を求める（高さの違いは数えない）
	/// </summary>
	/// <param name="position">位置</param>
	/// <returns>水平の距離（m）</returns>
	float GetHorizontalDistanceFromPlayer(Vector3 position)
	{
		return Vector3.ProjectOnPlane(position - PlayerManagerPresenter.SingletonInstance.transform.position, Vector3.up).magnitude;
	}

	bool IsPlayerCollider(Collider collider)
	{
		if (collider == null || PlayerManagerPresenter.SingletonInstance == null)
		{
			return false;
		}

		if (collider.gameObject == PlayerManagerPresenter.SingletonInstance.gameObject)
		{
			return true;
		}

		return collider.transform.IsChildOf(PlayerManagerPresenter.SingletonInstance.transform);
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

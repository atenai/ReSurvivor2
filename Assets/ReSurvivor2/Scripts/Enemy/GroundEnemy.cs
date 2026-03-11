using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Knife.Effects;
using UnityEngine.AI;
using Cinemachine;

/// <summary>
/// 地上敵
/// </summary>
public class GroundEnemy : Target
{
	[Tooltip("プレイヤー")]
	GameObject targetPlayer;
	public GameObject TargetPlayer => targetPlayer;
	[Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	public Animator Animator => animator;
	[Tooltip("物理")]
	[SerializeField] Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;
	[Tooltip("ナビメッシュ")]
	[SerializeField] NavMeshAgent navMeshAgent;
	public NavMeshAgent NavMeshAgent => navMeshAgent;

	[Header("コライダー")]

	[Tooltip("エネミー本体のコライダー")]
	[SerializeField] Collider enemyCollider;
	public Collider EnemyCollider
	{
		get { return enemyCollider; }
		set { enemyCollider = value; }
	}

	[Tooltip("センサーコライダー用の変数")]
	[SerializeField] ColliderEventHandler[] colliders = default;
	private bool[] hits;
	public bool GetHit(int index) => hits[index];
	private Collider hitCollider = null;
	public Collider HitCollider => hitCollider;

	[Header("レイキャスト")]
	[Tooltip("視界用の頭ゲームオブジェクト")]
	[SerializeField] GameObject head;
	/// <summary>視界な長さ</summary>
	float rayDistance = 14.0f;
	/// <summary>左右の最大角</summary>
	float halfAngle = 45f;
	/// <summary>度/秒（例：90なら1秒で90度回る）</summary>
	float sweepSpeed = 180f;
	/// <summary>現在の角度</summary>
	float currentAngle = 0f;
	/// <summary>角度の回転方向（+1で右へ、-1で左へ）</summary>
	float angleDir = 1f;

	[Header("追跡")]
	/// <summary>追跡時間</summary>
	float chaseTime = 25.0f;
	public float ChaseTime => chaseTime;
	/// <summary>追跡カウントタイマー</summary>
	float chaseCountTime;
	public float ChaseCountTime
	{
		get { return chaseCountTime; }
		set { chaseCountTime = value; }
	}
	/// <summary>追跡中か？</summary>
	bool isChase = false;
	public bool IsChase
	{
		get { return isChase; }
		set { isChase = value; }
	}
	[Tooltip("エネミー追跡範囲の中心点")]
	[SerializeField] GameObject centerPos;
	public GameObject CenterPos => centerPos;
	[Tooltip("追跡中か？のアラートイメージ")]
	[SerializeField] GameObject alert;


	[Header("パトロール")]
	[Tooltip("パトロールポイントの位置")]
	[SerializeField] List<GameObject> patrolPoints = new List<GameObject>();
	public List<GameObject> PatrolPoints => patrolPoints;
	[Tooltip("現在のパトロールポイントのナンバー")]
	int patrolPointNumber = 0;
	public int PatrolPointNumber
	{
		get { return patrolPointNumber; }
		set { patrolPointNumber = value; }
	}

	[Header("地面")]
	[Tooltip("地面の中心点")]
	[SerializeField] Transform groundCheckCenter;
	[Tooltip("地面となるレイヤー")]
	[SerializeField] LayerMask groundLayers;
	[Tooltip("地面検知範囲")]
	[SerializeField] float groundedRadius = 0.2f;
	[Tooltip("地面と接地しているか？")]
	bool isGrounded = false;
	public bool IsGrounded => isGrounded;

	[Header("攻撃")]
	[Tooltip("射撃距離")]
	[SerializeField] float shootingDistance = 8.0f;
	public float ShootingDistance => shootingDistance;

	[Tooltip("銃の現在のマガジンの弾数")]
	int currentMagazine;
	public int CurrentMagazine
	{
		get { return currentMagazine; }
		set { currentMagazine = value; }
	}
	[Tooltip("銃の最大マガジン数")]
	[SerializeField] int magazineCapacity = 8;
	public int MagazineCapacity => magazineCapacity;
	[Tooltip("銃のリロードのオン・オフ")]
	bool isReloadTimeActive = false;
	public bool IsReloadTimeActive
	{
		get { return isReloadTimeActive; }
		set { isReloadTimeActive = value; }
	}
	float reloadTime = 0.0f;
	public float ReloadTime
	{
		get { return reloadTime; }
		set { reloadTime = value; }
	}
	[Tooltip("銃のリロード時間")]
	readonly float reloadTimeDefine = 1.5f;
	public float ReloadTimeDefine => reloadTimeDefine;

	//↓アセットストアのプログラム↓//
	[Tooltip("マズルフラッシュと薬莢")]
	[SerializeField] ParticleGroupEmitter[] shotEmitters;
	[Tooltip("硝煙")]
	[SerializeField] ParticleGroupPlayer afterFireSmoke;
	//↑アセットストアのプログラム↑//

	[Tooltip("現在のグレネード数")]
	int currentGrenade = 3;
	public int CurrentGrenade
	{
		get { return currentGrenade; }
		set { currentGrenade = value; }
	}

	[Tooltip("シネマシーンインパルス")]
	[SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
	public CinemachineImpulseSource CinemachineImpulseSource => cinemachineImpulseSource;

	[Tooltip("デバッグ")]
	[SerializeField] DebugEnemy debugEnemy;
	public DebugEnemy DebugEnemy => debugEnemy;

	void Awake()
	{
		InitNavMeshAgent();
	}

	/// <summary>
	/// Awake で NavMeshAgent の自動位置・回転更新をオフにする。
	/// これにより NavMeshAgent は経路計算（path, steeringTarget 等）専用になり、
	/// 実際の移動は Rigidbody.velocity によって行う。
	/// </summary>
	void InitNavMeshAgent()
	{
		// NavMeshAgent による Transform 更新を無効化
		navMeshAgent.updatePosition = false;
		navMeshAgent.updateRotation = false;
		// ★これが重要：Agent内部位置を物理位置に同期
		navMeshAgent.nextPosition = enemyRigidbody.position;
	}

	/// <summary>
	/// 初期化処理
	/// </summary>
	new void Start()
	{
		base.Start();
		//Debug.Log("<color=orange>GroundEnemyクラスを初期化</color>");
		InitSensorCollider();
		Initialize();
	}

	/// <summary>
	/// センサーコライダーの初期化処理
	/// </summary> 
	void InitSensorCollider()
	{
		hits = new bool[colliders.Length];

		foreach (var i in colliders)
		{
			i.OnTriggerEnterEvent.AddListener(OnTriggerEnterHit);
			i.OnTriggerExitEvent.AddListener(OnTriggerExitHit);
		}
	}

	/// <summary>
	/// リスポーンした際の初期化処理
	/// </summary>
	void Initialize()
	{
		if (targetPlayer == null)
		{
			targetPlayer = Player.SingletonInstance.gameObject;
		}

		enemyRigidbody.useGravity = true;
		enemyCollider.enabled = true;

		ResetSensorCollider();

		//各種パラメーターを初期化
		isChase = false;
		patrolPointNumber = 0;
		isGrounded = false;
		InitGunMagazine();
		GroundCheck();
	}

	/// <summary>
	/// センサーコライダーの状態をリセットする
	/// </summary>
	void ResetSensorCollider()
	{
		//コライダーが何も当たっていない状態にする
		if (hits != null)
		{
			for (int i = 0; i < colliders.Length; i++)
			{
				hits[i] = false;
			}
		}
		hitCollider = null;
	}

	/// <summary>
	/// 銃の弾数の初期化処理
	/// </summary>
	void InitGunMagazine()
	{
		CurrentMagazine = MagazineCapacity;
		isReloadTimeActive = false;
	}

	/// <summary>
	/// 地面に接触しているか？をチェックする関数
	/// </summary>
	void GroundCheck()
	{
		Vector3 spherePosition = groundCheckCenter.transform.position;
		bool centerChecker = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
		isGrounded = centerChecker;
	}

	/// <summary>
	/// 地面に接触しているか？を可視化する関数
	/// </summary>
	void OnDrawGizmosSelected()
	{
		if (isGrounded == true)
		{
			//地面についている
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Gizmos.color = transparentGreen;
		}
		else
		{
			//地面についていない
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
			Gizmos.color = transparentRed;
		}

		Gizmos.DrawSphere(new Vector3(groundCheckCenter.transform.position.x, groundCheckCenter.transform.position.y, groundCheckCenter.transform.position.z), groundedRadius);
	}

	/// <summary>
	/// 更新処理
	/// </summary>
	new void Update()
	{
		base.Update();
		GroundCheck();

		Eyesight();
		Alert();
		ChasePlayer();

#if UNITY_EDITOR
		//デバッグ関連の処理
		DebugText();
#endif
	}

	/// <summary>
	/// レイキャストによる視界
	/// </summary>
	void Eyesight()
	{
		if (IsDead == true)
		{
			return;
		}

		// 角度を進める
		currentAngle = currentAngle + angleDir * sweepSpeed * Time.deltaTime;

		// 端で反転（往復）
		if (halfAngle < currentAngle)
		{
			currentAngle = halfAngle;
			angleDir = -1f;
		}
		else if (currentAngle < -halfAngle)
		{
			currentAngle = -halfAngle;
			angleDir = 1f;
		}

		Vector3 pos = head.transform.position;
		Vector3 forward = this.transform.forward;
		Vector3 up = this.transform.up;

		if (forward.sqrMagnitude < 0.0001f)
		{
			forward = Vector3.forward;
		}
		forward.Normalize();

		// 今の角度だけ回した方向に1本Ray
		Vector3 dir = Quaternion.AngleAxis(currentAngle, up) * forward;

		Debug.DrawRay(pos, dir * rayDistance, Color.yellow);

		RaycastHit hit;
		if (Physics.Raycast(pos, dir, out hit, rayDistance))
		{
			// ヒット時の処理（例：プレイヤー検知など）
			if (hit.collider.CompareTag("Player"))
			{
				//Debug.Log("<color=red>プレイヤーを発見!</color>");
				//ChaseOn();
				EnemyManager.SingletonInstance.AllChaseOn();
			}
		}
	}

	/// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn()
	{
		//Debug.Log("<color=red>GroundEnemyのChaseOn()</color>");
		isChase = true;
		chaseCountTime = chaseTime;
	}

	/// <summary>
	/// アラートイメージの表示・非表示
	/// </summary>
	void Alert()
	{
		if (IsDead == false)
		{
			alert.gameObject.SetActive(isChase);
		}
		else
		{
			alert.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// プレイヤーを追跡中の処理
	/// </summary>
	void ChasePlayer()
	{
		if (isChase == true)
		{
			chaseCountTime -= Time.deltaTime;
			const float minTimer = 0.0f;
			if (chaseCountTime <= minTimer)
			{
				isChase = false;
			}
		}
	}

	/// <summary>
	/// デバッグテキスト
	/// </summary> 
	void DebugText()
	{
		string[] debugTexts = new string[11];

		debugTexts[10] = "IsDead : " + IsDead.ToString();

		debugTexts[9] = "currentGrenade : " + currentGrenade.ToString();

		debugTexts[8] = "reloadTime : " + reloadTime.ToString();
		debugTexts[7] = "isReloadTimeActive : " + isReloadTimeActive.ToString();
		debugTexts[6] = "currentMagazine : " + currentMagazine.ToString();

		debugTexts[5] = "chaseCountTime : " + chaseCountTime.ToString();
		debugTexts[4] = "isChase : " + isChase.ToString();

		debugTexts[3] = "patrolPointNumber : " + patrolPointNumber.ToString();
		debugTexts[2] = "isGrounded : " + isGrounded.ToString();


		if (hitCollider != null)
		{
			debugTexts[1] = "hitCollider : " + hitCollider.ToString();
		}
		else
		{
			debugTexts[1] = "hitCollider : " + "null";
		}
		debugTexts[0] = "hits[0] : " + hits[0].ToString();

		debugEnemy.DebugSystem(debugEnemy.IsDebugMode, Canvas, debugTexts.Length);
		debugEnemy.SetDebugText(debugTexts);
	}

	/// <summary>
	/// センサーコライダーの当たり判定(触れた時)
	/// </summary>
	/// <param name="self"></param>
	/// <param name="collider"></param>
	void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
	{
		if (collider.tag == "Player")
		{
			if (collider != null)
			{
				//Debug.Log("<color=blue>プレイヤーを発見!2</color>");
				hitCollider = collider;
			}

			int index = 0;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i] == self)
				{
					index = i;
				}
			}

			hits[index] = true;
		}
	}

	/// <summary>
	/// センサーコライダーの当たり判定(離れた時)
	/// </summary>
	/// <param name="self"></param>
	/// <param name="collider"></param>
	void OnTriggerExitHit(ColliderEventHandler self, Collider collider)
	{
		if (collider.tag == "Player")
		{
			if (collider != null)
			{
				hitCollider = collider;
			}

			int index = 0;
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i] == self)
				{
					index = i;
				}
			}

			hits[index] = false;
		}
	}

	/// <summary>
	/// 物理の当たり判定
	/// </summary>
	/// <param name="collision"></param>
	void OnCollisionEnter(Collision collision)
	{
		//Debug.Log("Hit");
	}

	/// <summary>
	/// 地面に接触しているか？を判別する当たり判定(触れた時)
	/// </summary>
	public void OnTriggerEnterHitCliff(Collider collider)
	{
		//Debug.Log("<color=red>OnTriggerEnterHitGround</color>");
	}

	/// <summary>
	/// 地面に接触しているか？を判別する当たり判定(離れた時)
	/// </summary>
	public void OnTriggerExitHitCliff(Collider collider)
	{
		//Debug.Log("<color=green>OnTriggerExitHitGround</color>");
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	/// <param name="amount">ダメージ量</param>
	public override void TakeDamage(float amount)
	{
		Debug.Log("<color=green>GroundEnemyのTakeDamage()</color>");
		CurrentHp = CurrentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		SliderHp.value = (float)CurrentHp / (float)MaxHp;
		if (CurrentHp <= 0.0f)
		{
			//敵マーカー削除
			EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
			IsDead = true;
		}

		//地雷の攻撃を受けた際にチェイス状態になってほしくは無い
		//ChaseOn();
	}

	/// <summary>
	/// ハンドガンの射撃SE
	/// </summary> 
	public void HandGunFireSE()
	{
		SoundManager.SingletonInstance.HandGunShootSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunMuzzleTransform);
	}

	/// <summary>
	/// ハンドガンのリロードSE
	/// </summary> 
	public void HandGunReloadSE()
	{
		SoundManager.SingletonInstance.HandGunReloadSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// ハンドガンの薬莢SE
	/// </summary>
	public void HandGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.HandGunBulletCasingSEPool.GetGameObject(Player.SingletonInstance.GunModelFacade.HandGunModel.HandGunBulletCasingTransform);
	}

	/// <summary>
	/// アサルトライフルの射撃SE
	/// </summary> 
	public void AssaultRifleFireSE()
	{
		SoundManager.SingletonInstance.AssaultRifleShootSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// アサルトライフルのリロードSE
	/// </summary> 
	public void AssaultRifleReloadSE()
	{
		SoundManager.SingletonInstance.AssaultRifleReloadSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// アサルトライフルの薬莢SE
	/// </summary>
	public void AssaultRifleBulletCasingSE()
	{
		SoundManager.SingletonInstance.AssaultRifleBulletCasingSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンの射撃SE
	/// </summary> 
	public void ShotGunFireSE()
	{
		SoundManager.SingletonInstance.ShotGunShootSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンのリロードSE
	/// </summary> 
	public void ShotGunReloadSE()
	{
		SoundManager.SingletonInstance.ShotGunReloadSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// ショットガンの薬莢SE
	/// </summary>
	public void ShotGunBulletCasingSE()
	{
		SoundManager.SingletonInstance.ShotGunBulletCasingSEPool.GetGameObject(this.transform);
	}

	/// <summary>
	/// マズルフラッシュのエフェクトと薬莢を出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void MuzzleFlashAndShell()
	{
		if (shotEmitters != null)
		{
			foreach (var effect in shotEmitters)
			{
				effect.Emit(1);
			}
		}
	}

	/// <summary>
	/// 硝煙のエフェクトを出す（アセットストアで買ったコードをそのままもってきている）
	/// </summary>
	public void AfterFireSmoke()
	{
		if (afterFireSmoke != null)
		{
			afterFireSmoke.Play();
		}
	}

	/// <summary>
	/// プレイヤーにダメージを与えた際にカメラを揺らす
	/// </summary>
	public void Shaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
	}

	void OnDisable()
	{
		UnInitRigidbody();
	}

	/// <summary>
	/// コンポーネントやオブジェクトが無効化されたら速度をリセットする。
	/// これにより停止状態で無効化され、再有効化時に不意な移動が発生しない。
	/// </summary>
	void UnInitRigidbody()
	{
		enemyRigidbody.velocity = Vector3.zero;
	}
}

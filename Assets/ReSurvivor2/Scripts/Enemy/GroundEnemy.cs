using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Knife.Effects;

/// <summary>
/// 地上敵
/// </summary>
public class GroundEnemy : Target
{
	[UnityEngine.Tooltip("プレイヤー")]
	GameObject targetPlayer;
	public GameObject TargetPlayer => targetPlayer;
	[UnityEngine.Tooltip("アニメーター")]
	[SerializeField] Animator animator;
	public Animator Animator => animator;
	[UnityEngine.Tooltip("物理")]
	[SerializeField] Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;
	[UnityEngine.Tooltip("エネミー本体のコライダー")]
	[SerializeField] Collider enemyCollider;
	public Collider EnemyCollider
	{
		get { return enemyCollider; }
		set { enemyCollider = value; }
	}

	[Header("センサーコライダー用の変数")]
	[SerializeField] ColliderEventHandler[] colliders = default;
	private bool[] hits;
	public bool GetHit(int index) => hits[index];
	private Collider hitCollider = null;
	public Collider HitCollider => hitCollider;

	[Header("追跡")]
	[UnityEngine.Tooltip("追跡時間の設定")]
	[SerializeField] float chaseTime = 10.0f;
	public float ChaseTime => chaseTime;
	[UnityEngine.Tooltip("追跡カウントタイマー")]
	float chaseCountTime;
	public float ChaseCountTime
	{
		get { return chaseCountTime; }
		set { chaseCountTime = value; }
	}
	[UnityEngine.Tooltip("追跡中か？")]
	bool isChase = false;
	public bool IsChase
	{
		get { return isChase; }
		set { isChase = value; }
	}
	[UnityEngine.Tooltip("エネミー追跡範囲の中心点")]
	[SerializeField] GameObject centerPos;
	public GameObject CenterPos => centerPos;
	[UnityEngine.Tooltip("追跡中か？のアラートイメージ")]
	[SerializeField] GameObject alert;
	[UnityEngine.Tooltip("視界用の頭ゲームオブジェクト")]
	[SerializeField] GameObject head;
	[UnityEngine.Tooltip("視界な長さ")]
	[SerializeField] float rayDistance = 14.0f;

	[Header("パトロール")]
	[UnityEngine.Tooltip("パトロールポイントの位置")]
	[SerializeField] List<GameObject> patrolPoints = new List<GameObject>();
	public List<GameObject> PatrolPoints => patrolPoints;
	[UnityEngine.Tooltip("現在のパトロールポイントのナンバー")]
	int patrolPointNumber = 0;
	public int PatrolPointNumber
	{
		get { return patrolPointNumber; }
		set { patrolPointNumber = value; }
	}

	[UnityEngine.Tooltip("地面の中心点")]
	[SerializeField] Transform groundCheckCenter;
	[UnityEngine.Tooltip("地面となるレイヤー")]
	[SerializeField] LayerMask groundLayers;
	[UnityEngine.Tooltip("地面検知範囲")]
	[SerializeField] float groundedRadius = 0.2f;
	[UnityEngine.Tooltip("地面と接地しているか？")]
	bool isGrounded = false;
	public bool IsGrounded => isGrounded;

	[Header("攻撃")]
	[UnityEngine.Tooltip("射撃距離")]
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

	[Tooltip("射撃のSE")]
	[SerializeField] GameObject shootSe;
	float shootSeDestroyTime = 1.0f;
	[Tooltip("リロードのSE")]
	[SerializeField] GameObject reloadSe;
	float reloadSeDestroyTime = 1.0f;
	[Tooltip("薬莢のSE")]
	[SerializeField] GameObject bulletCasingSe;
	float bulletCasingSeDestroyTime = 1.0f;

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

	[Tooltip("デバッグ")]
	[SerializeField] DebugEnemy debugEnemy;
	public DebugEnemy DebugEnemy => debugEnemy;

	new void Start()
	{
		base.Start();
		//Debug.Log("<color=orange>GroundEnemyクラスを初期化</color>");
		InitSensorCollider();
		Initialize();

		//エフェクトの初期化処理（正直気に入らない、いつかこの処理が必要ないように作る）
		MuzzleFlashAndShell();
		AfterFireSmoke();
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
	/// 地面に接触しているか？をチェックする関数
	/// </summary>
	void GroundCheck()
	{
		Vector3 spherePosition = groundCheckCenter.transform.position;
		bool centerChecker = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
		isGrounded = centerChecker;
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
		Ray ray = new Ray(head.transform.position, this.transform.forward);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, rayDistance))
		{
			if (hit.collider.tag == "Player")
			{
				//Debug.Log("<color=red>プレイヤーを発見!</color>");
				ChaseOn();
			}
		}
		Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow);
	}

	/// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn()
	{
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
	/// 射撃SE
	/// </summary> 
	public void FireSE()
	{
		UnityEngine.GameObject se = Instantiate(shootSe, this.transform.position, Quaternion.identity);
		Destroy(se, shootSeDestroyTime);
	}

	/// <summary>
	/// リロードSE
	/// </summary> 
	public void ReloadSE()
	{
		UnityEngine.GameObject se = Instantiate(reloadSe, this.transform.position, Quaternion.identity);
		Destroy(se, reloadSeDestroyTime);
	}

	/// <summary>
	/// 薬莢SE
	/// </summary>
	public void BulletCasingSE()
	{
		UnityEngine.GameObject se = Instantiate(bulletCasingSe, this.transform.position, Quaternion.identity);
		Destroy(se, bulletCasingSeDestroyTime);
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

	void FixedUpdate()
	{

	}

	/// <summary>
	/// センサーコライダーの当たり判定(触れた時)
	/// </summary>
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

	public override void TakeDamage(float amount)
	{
		Debug.Log("<color=green>GroundEnemyのTakeDamage()</color>");
		CurrentHp = CurrentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		SliderHp.value = (float)CurrentHp / (float)MaxHp;
		if (CurrentHp <= 0.0f)
		{
			IsDead = true;
		}
	}
}

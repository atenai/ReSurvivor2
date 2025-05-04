using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 空中敵
/// </summary>
public class FlyingEnemy : Target
{
	[UnityEngine.Tooltip("プレイヤー")]
	GameObject targetPlayer;
	public GameObject TargetPlayer
	{
		get { return targetPlayer; }
		private set { targetPlayer = value; }
	}
	[UnityEngine.Tooltip("物理")]
	[SerializeField] Rigidbody rb;
	public Rigidbody Rigidbody
	{
		get { return rb; }
		private set { rb = value; }
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
	float rayDistance = 8.0f;

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

	[Header("爆発")]
	[Tooltip("爆発エフェクトのプレファブ")]
	[SerializeField] GameObject mineExplosionEffectPrefab = null;
	[Tooltip("爆発エフェクトの終了時間")]
	[SerializeField] float mineExplosionEffectDestroyTime = 1.0f;

	[Tooltip("地雷のSEプレファブ")]
	[SerializeField] GameObject mineSePrefab = null;
	[Tooltip("地雷のSE終了時間")]
	[SerializeField] float mineSeEndtime = 1.0f;

	[Tooltip("爆発のあたり判定オブジェクトを生成")]
	[SerializeField] GameObject mineExplosionColliderPrefab = null;
	float mineExplosionColliderDestroyTime = 1.0f;

	[Tooltip("デバッグ")]
	[SerializeField] DebugEnemy debugEnemy;
	public DebugEnemy DebugEnemy => debugEnemy;

	[Header("ビヘイビアデザイナー用変数")]
	bool isRotateToDirectionPlayer = false;
	public bool IsRotateToDirectionPlayer
	{
		get { return isRotateToDirectionPlayer; }
		set { isRotateToDirectionPlayer = value; }
	}

	bool isMoveForward = false;
	public bool IsMoveForward
	{
		get { return isMoveForward; }
		set { isMoveForward = value; }
	}

	bool isMoveBack = false;
	public bool IsMoveBack
	{
		get { return isMoveBack; }
		set { isMoveBack = value; }
	}

	new void Start()
	{
		base.Start();
		//Debug.Log("<color=orange>FlyingEnemyクラスを初期化</color>");
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

		if (rb == null)
		{
			rb = this.GetComponent<Rigidbody>();
		}

		ResetSensorCollider();

		//各種パラメーターを初期化
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

	new void Update()
	{
		base.Update();
		Eyesight();
		alert.gameObject.SetActive(isChase);
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
		Ray ray = new Ray(head.transform.position, -head.transform.forward);//-head.transform.forwardをマイナスにしているのはなぜかドローンのモデル目の前方が反対になっていた為
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
		string[] debugTexts = new string[8];

		debugTexts[7] = "chaseCountTime : " + chaseCountTime.ToString();
		debugTexts[6] = "isChase : " + isChase.ToString();

		debugTexts[5] = "patrolPointNumber : " + patrolPointNumber.ToString();

		if (hitCollider != null)
		{
			debugTexts[4] = "hitCollider : " + hitCollider.ToString();
		}
		else
		{
			debugTexts[4] = "hitCollider : " + "null";
		}
		debugTexts[3] = "hits[3]Back : " + hits[3].ToString();
		debugTexts[2] = "hits[2]Up : " + hits[2].ToString();
		debugTexts[1] = "hits[1]Under : " + hits[1].ToString();
		debugTexts[0] = "hits[0]Front : " + hits[0].ToString();

		debugEnemy.DebugSystem(debugEnemy.IsDebugMode, Canvas, debugTexts.Length);
		debugEnemy.SetDebugText(debugTexts);
	}

	/// <summary>
	/// センサーコライダーの当たり判定(触れた時)
	/// </summary>
	void OnTriggerEnterHit(ColliderEventHandler self, Collider collider)
	{
		if (collider.tag == "Player" || collider.tag == "Object")
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
		if (collider.tag == "Player" || collider.tag == "Object")
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
		if (isChase == true)
		{
			if (collision.gameObject.CompareTag("Player") == true)
			{
				Debug.Log("<color=red>プレイヤー爆発！</color>");
				Explosion();
			}

			if (collision.gameObject.CompareTag("GroundEnemy") == true)
			{
				Debug.Log("<color=orange>グラウンドエネミー爆発！</color>");
				Explosion();
			}

			if (collision.gameObject.CompareTag("Ground") == true)
			{
				Debug.Log("<color=yellow>グラウンド爆発！</color>");
				Explosion();
			}
		}
	}

	/// <summary>
	/// 爆発
	/// </summary>
	void Explosion()
	{
		if (mineSePrefab != null)
		{
			//SEオブジェクトを生成する
			var se = Instantiate(mineSePrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(se, mineSeEndtime);
		}

		if (mineExplosionEffectPrefab != null)
		{
			//爆発エフェクトオブジェクトを生成する	
			var effect = Instantiate(mineExplosionEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(effect, mineExplosionEffectDestroyTime);
		}

		if (mineExplosionColliderPrefab != null)
		{
			//爆発のあたり判定オブジェクトを生成する	
			var mineExplosionCollider = Instantiate(mineExplosionColliderPrefab, this.gameObject.transform.position, Quaternion.identity);
			Destroy(mineExplosionCollider, mineExplosionColliderDestroyTime);
		}

		Destroy(this.gameObject);
	}
}

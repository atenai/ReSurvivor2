using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
/// 空中敵
/// </summary>
public class FlyingEnemy : MonoBehaviour, IEnemy
{
	[UnityEngine.Tooltip("HP")]
	HitPoint hp;
	public HitPoint GetHitPoint()
	{
		return hp;
	}

	[UnityEngine.Tooltip("HPバー")]
	[SerializeField] Slider sliderHp;
	public Slider SliderHp
	{
		get { return sliderHp; }
		set { sliderHp = value; }
	}

	[UnityEngine.Tooltip("キャンバス")]
	[SerializeField] Canvas canvas;
	public Canvas Canvas => canvas;

	[Tooltip("ナビメッシュ")]
	[SerializeField] NavMeshAgent navMeshAgent;
	public NavMeshAgent NavMeshAgent => navMeshAgent;

	[Tooltip("物理")]
	[SerializeField] Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;

	[Tooltip("カバーポイント")]
	CoverPoint[] coverPoints;
	public CoverPoint[] GetCoverPoints => coverPoints;
	public void SetCoverPoints(CoverPoint[] coverPoints)
	{
		this.coverPoints = coverPoints;
	}

	[Tooltip("プレイヤー")]
	GameObject targetPlayer;
	public GameObject TargetPlayer => targetPlayer;

	[Header("コライダー")]

	[Header("センサーコライダー用の変数")]
	[SerializeField] ColliderEventHandler[] colliders = default;
	bool[] hits;
	public bool GetHit(int index) => hits[index];
	Collider hitCollider = null;
	public Collider HitCollider => hitCollider;

	[Header("レイキャスト")]
	[Tooltip("視界用の頭ゲームオブジェクト")]
	[SerializeField] GameObject head;
	/// <summary>視界な長さ</summary>
	float rayDistance = 20.0f;
	/// <summary>スフィアレイキャストの半径</summary>
	float sphereRayCastRadius = 2.0f;
	/// <summary>左右の最大角</summary>
	float halfAngle = 60f;
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
	void Start()
	{
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
		hp = new HitPoint(HitPoint.MaxHp);
		hp.Initialize(DamageEffect, Dead);
		sliderHp.value = 1;
		//敵マーカー作成
		EnemyIndicatorManager.SingletonInstance.InstanceIndicator(this);

		if (targetPlayer == null)
		{
			targetPlayer = Player.SingletonInstance.gameObject;
		}

		if (enemyRigidbody == null)
		{
			enemyRigidbody = this.GetComponent<Rigidbody>();
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

	/// <summary>
	/// 更新処理
	/// </summary>
	void Update()
	{
		RotCanvas();
		Eyesight();
		Alert();
		ChasePlayer();

#if UNITY_EDITOR
		//デバッグ関連の処理
		DebugText();
#endif
	}

	/// <summary>
	/// 常にキャンバスをメインカメラの方を向かせる
	/// </summary>
	void RotCanvas()
	{
		canvas.transform.rotation = Camera.main.transform.rotation;
	}

	/// <summary>
	/// レイキャストによる視界
	/// </summary>
	void Eyesight()
	{
		if (hp.IsDead == true)
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

		// レイの原点と基準方向はheadの向きに合わせる
		Vector3 pos = head.transform.position;
		//-head.transform.forwardをマイナスにしているのはなぜかドローンのモデル目の前方が反対になっていた為
		Vector3 forward = -head.transform.forward;
		Vector3 right = head.transform.right;

		if (forward.sqrMagnitude < 0.0001f)
		{
			forward = Vector3.forward;
		}
		forward.Normalize();

		// ローカルの右方向（head.transform.right）を回転軸にして上下にスイープ
		Vector3 dir = Quaternion.AngleAxis(currentAngle, right) * forward;

		Debug.DrawRay(pos, dir * rayDistance, Color.yellow);

		RaycastHit hit;
		if (Physics.SphereCast(pos, sphereRayCastRadius, dir, out hit, rayDistance))
		{
			// ヒット時の処理（例：プレイヤー検知など）
			if (hit.collider.CompareTag("Player"))
			{
				EnemyManager.SingletonInstance.AllChaseOn();
			}
		}
	}

	/// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn()
	{
		//Debug.Log("<color=red>FlyingEnemyのChaseOn()</color>");
		isChase = true;
		chaseCountTime = chaseTime;
	}

	/// <summary>
	/// アラートイメージの表示・非表示
	/// </summary>
	void Alert()
	{
		if (hp.IsDead == false)
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
	/// <param name="self"></param>
	/// <param name="collider"></param>
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
	/// <param name="self"></param>
	/// <param name="collider"></param>
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

	/// <summary>
	/// 物理の当たり判定
	/// </summary>
	/// <param name="collision"></param>
	void OnCollisionEnter(Collision collision)
	{
		Debug.Log("<color=red>当たった！ : " + collision.gameObject.name + "</color>");
		if (isChase == true)
		{
			hp.Damage(HitPoint.MaxHp);
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
	}

	/// <summary>
	/// ダメージエフェクト
	/// </summary>
	public void DamageEffect()
	{
		SliderHp.value = (float)hp.CurrentHp / (float)HitPoint.MaxHp;
	}

	/// <summary>
	/// 死んだ際の処理
	/// </summary>
	public void Dead()
	{
		EnemyManager.SingletonInstance.RemoveEnemyList(this.transform.parent.gameObject);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
		Explosion();
		Destroy(this.transform.parent.gameObject);
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		DeleteIndicator();
	}

	/// <summary>
	/// 敵マーカー削除
	/// </summary>
	void DeleteIndicator()
	{
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
	}

	/// <summary>
	/// エネミーのゲームオブジェクトを取得する
	/// </summary>
	/// <returns></returns>
	public GameObject GetEnemyGameObject()
	{
		return this.gameObject;
	}
}

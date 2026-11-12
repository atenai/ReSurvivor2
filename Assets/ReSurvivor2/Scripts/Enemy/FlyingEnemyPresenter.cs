using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;

/// <summary>
/// 空中敵
/// MVPパターンのPresenter担当
/// </summary>
public class FlyingEnemyPresenter : MonoBehaviour, IEnemy
{
	[Header("空中敵MVPパターン")]
	/// <summary>空中敵モデル</summary>
	FlyingEnemyModel flyingEnemyModel = new FlyingEnemyModel();
	public FlyingEnemyModel FlyingEnemyModel => flyingEnemyModel;

	[Tooltip("空中敵キャラクタービュー")]
	[SerializeField] FlyingEnemyCharacterView flyingEnemyCharacterView;
	public FlyingEnemyCharacterView FlyingEnemyCharacterView => flyingEnemyCharacterView;

	[Tooltip("空中敵UIビュー")]
	[SerializeField] FlyingEnemyUIView flyingEnemyUIView;
	public FlyingEnemyUIView FlyingEnemyUIView => flyingEnemyUIView;

	[Tooltip("ナビメッシュ")]
	[SerializeField] NavMeshAgent navMeshAgent;
	public NavMeshAgent NavMeshAgent => navMeshAgent;

	[Tooltip("物理")]
	[SerializeField] Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;

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

	[Header("追跡")]
	[Tooltip("エネミー追跡範囲の中心点")]
	[SerializeField] GameObject centerPos;
	public GameObject CenterPos => centerPos;

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

	[Tooltip("シネマシーンインパルス")]
	[SerializeField] CinemachineImpulseSource cinemachineImpulseSource;

	[Tooltip("デバッグ")]
	[SerializeField] DebugEnemy debugEnemy;

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

	/// <summary>エネミーのゲームオブジェクトを取得する</summary>
	public GameObject GetEnemyGameObject()
	{
		return this.gameObject;
	}

	/// <summary>HPを取得する</summary>
	public HitPoint GetHitPoint()
	{
		return flyingEnemyModel.HP;
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
		flyingEnemyModel.Initialize();
		flyingEnemyModel.HP.Initialize(DamageEffect, Dead);
		flyingEnemyUIView.InitHP(flyingEnemyModel.HP.CurrentHp);
		//敵マーカー作成
		EnemyIndicatorManager.SingletonInstance.InstanceIndicator(this);
		EnemyManager.SingletonInstance.ChaseEvent.AddListener(flyingEnemyModel.ChaseOn);

		if (targetPlayer == null)
		{
			targetPlayer = PlayerManagerPresenter.SingletonInstance.gameObject;
		}

		ResetSensorCollider();

		//各種パラメーターを初期化
	}

	/// <summary>
	/// ダメージエフェクト
	/// </summary>
	public void DamageEffect()
	{
		flyingEnemyUIView.SliderHp.value = (float)flyingEnemyModel.HP.CurrentHp / (float)HitPoint.Max_Hp;
	}

	/// <summary>
	/// 死んだ際の処理
	/// </summary>
	public void Dead()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(flyingEnemyModel.ChaseOn);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
		Explosion();
		Destroy(this.transform.parent.gameObject);
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
		flyingEnemyUIView.RotCanvas();

		//-head.transform.forwardをマイナスにしているのはなぜかドローンのモデル目の前方が反対になっていた為
		if (flyingEnemyModel.Eyesight(flyingEnemyCharacterView.Head.transform.position, -flyingEnemyCharacterView.Head.transform.forward, flyingEnemyCharacterView.Head.transform.right) == true)
		{
			//Debug.Log("<color=red>プレイヤーを発見!</color>");
			//まだ警戒体制の敵が1体もいない時に見つけた ＝ この敵がプレイヤーを最初に発見したので、この敵の方向に発見マーカーを表示する
			//（ChaseEventで全員が警戒体制になるので、ChaseEventより先に調べる。自分が警戒体制の時は、他の敵を調べるまでもなく最初の発見ではない）
			if (flyingEnemyModel.IsAlert == false && EnemyManager.SingletonInstance.IsAnyEnemyAlert() == false)
			{
				EnemyIndicatorManager.SingletonInstance.ShowAlertIndicator(this);
			}
			EnemyManager.SingletonInstance.ChaseEvent.Invoke();
		}
		Alert();
		flyingEnemyModel.ChasePlayer();

#if UNITY_EDITOR
		//デバッグ関連の処理
		DebugText();
#endif
	}

	/// <summary>
	/// アラートイメージの表示・非表示
	/// </summary>
	void Alert()
	{
		if (flyingEnemyModel.HP.IsDead == false)
		{
			flyingEnemyUIView.Alert.SetActive(flyingEnemyModel.IsChase);
		}
		else
		{
			flyingEnemyUIView.Alert.SetActive(false);
		}
	}

	/// <summary>
	/// デバッグテキスト
	/// </summary> 
	void DebugText()
	{
		string[] debugTexts = new string[8];

		debugTexts[7] = "chaseCountTime : " + flyingEnemyModel.ChaseCountTime.ToString();
		debugTexts[6] = "isChase : " + flyingEnemyModel.IsChase.ToString();

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

		debugEnemy.DebugSystem(debugEnemy.IsDebugMode, flyingEnemyUIView.Canvas, debugTexts.Length);
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
		// 	Debug.Log("<color=red>当たった！ : " + collision.gameObject.name + "</color>");

		if (flyingEnemyModel.IsChase == true)
		{
			flyingEnemyModel.HP.Damage(HitPoint.Max_Hp);
		}

		// Note : 爆発コライダーの方で行っている
		// if (collision.collider.tag == "Player")
		// {
		// 	Player.SingletonInstance.HP.Damage(damage);
		// 	CameraShaker();
		// }
	}

	/// <summary>
	/// プレイヤーにダメージを与えた際にカメラを揺らす
	/// </summary>
	void CameraShaker()
	{
		cinemachineImpulseSource.GenerateImpulse();
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
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(flyingEnemyModel.ChaseOn);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
	}
}

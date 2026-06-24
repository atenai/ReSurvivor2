using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
/// 的
/// </summary>
public class Target : MonoBehaviour, IEnemy
{
	[UnityEngine.Tooltip("HP")]
	[SerializeField] HitPoint hp;
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

	void Start()
	{
		//Debug.Log("<color=orange>Targetクラスを初期化</color>");
		Initialize();
	}

	/// <summary>
	/// リスポーンした際の初期化処理
	/// </summary>
	void Initialize()
	{
		hp = new HitPoint(HitPoint.Max_Hp);
		hp.Initialize(DamageEffect, Dead);
		sliderHp.value = 1;
		//敵マーカー作成
		EnemyIndicatorManager.SingletonInstance.InstanceIndicator(this);
		EnemyManager.SingletonInstance.ChaseEvent.AddListener(ChaseOn);
	}

	void Update()
	{
		RotCanvas();
	}

	/// <summary>
	/// 常にキャンバスをメインカメラの方を向かせる
	/// </summary>
	void RotCanvas()
	{
		canvas.transform.rotation = Camera.main.transform.rotation;
	}

	/// <summary>
	/// 追跡開始
	/// </summary>
	public void ChaseOn()
	{
		Debug.Log("<color=red>GroundEnemyのChaseOn()</color>");
	}

	/// <summary>
	/// ダメージエフェクト
	/// </summary>
	public void DamageEffect()
	{
		sliderHp.value = (float)hp.CurrentHp / (float)HitPoint.Max_Hp;
	}

	/// <summary>
	/// 死んだ際の処理
	/// </summary>
	public void Dead()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(ChaseOn);
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
		Destroy(this.transform.parent.gameObject);
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		EnemyManager.SingletonInstance.ChaseEvent.RemoveListener(ChaseOn);
		//敵マーカー削除
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

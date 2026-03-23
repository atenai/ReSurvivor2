using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
	[UnityEngine.Tooltip("現在のHP")]
	float currentHp = 100.0f;
	public float CurrentHp
	{
		get { return currentHp; }
		set { currentHp = value; }
	}
	[UnityEngine.Tooltip("最大HP")]
	[SerializeField] float maxHp = 100.0f;
	public float MaxHp => maxHp;
	[UnityEngine.Tooltip("キャンバス")]
	[SerializeField] Canvas canvas;
	public Canvas Canvas => canvas;
	[UnityEngine.Tooltip("HPバー")]
	[SerializeField] Slider sliderHp;
	public Slider SliderHp
	{
		get { return sliderHp; }
		set { sliderHp = value; }
	}
	[UnityEngine.Tooltip("死んだか？")]
	bool isDead = false;
	public bool IsDead
	{
		get { return isDead; }
		set { isDead = value; }
	}

	[Tooltip("ナビメッシュ")]
	[SerializeField] protected NavMeshAgent navMeshAgent;
	public NavMeshAgent NavMeshAgent => navMeshAgent;
	[Tooltip("物理")]
	[SerializeField] protected Rigidbody enemyRigidbody;
	public Rigidbody Rigidbody => enemyRigidbody;

	[Tooltip("カバーポイント")]
	[SerializeField] CoverPoint[] coverPoints;
	public CoverPoint[] CoverPoints => coverPoints;

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

	public void Start()
	{
		//Debug.Log("<color=orange>Targetクラスを初期化</color>");
		sliderHp.value = 1;
		currentHp = maxHp;
		isDead = false;
		//敵マーカー作成
		EnemyIndicatorManager.SingletonInstance.InstanceIndicator(this);
	}

	public void Update()
	{
		//常にキャンバスをメインカメラの方を向かせる
		canvas.transform.rotation = Camera.main.transform.rotation;
	}

	/// <summary>
	/// ゲームオブジェクトが非表示またはデストロイされた際に呼ばれる
	/// </summary>
	void OnDisable()
	{
		//敵マーカー削除
		EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
	}

	/// <summary>
	/// ダメージ処理
	/// </summary>
	/// <param name="amount">ダメージ量</param>
	public virtual void TakeDamage(float amount)
	{
		Debug.Log("<color=orange>TargetのTakeDamage()</color>");
		currentHp = currentHp - amount;
		//Debug.Log("<color=orange>currentHp : " + currentHp + "</color>");
		sliderHp.value = (float)currentHp / (float)maxHp;
		if (currentHp <= 0.0f)
		{
			//敵マーカー削除
			EnemyIndicatorManager.SingletonInstance.DeleteIndicator(this);
			Destroy(this.gameObject);
		}
	}
}

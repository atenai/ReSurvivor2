using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーを攻撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyGrenadeAction : Action
{
	GroundEnemy groundEnemy;
	bool isEnd = false;

	[UnityEngine.Tooltip("グレネード")]
	[SerializeField] GameObject grenadeGameObjectPrefab;
	[UnityEngine.Tooltip("キャラクターからの攻撃物の生成距離")]
	[SerializeField] float spawnDistance = 2.0f;
	[UnityEngine.Tooltip("攻撃間隔")]
	float attackTime = 2.0f;
	[UnityEngine.Tooltip("攻撃カウント")]
	float count = 0.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		groundEnemy.InitAnimation();
		groundEnemy.Animator.SetBool("b_isGrenadeEquip", true);
		InitMove();
	}

	/// <summary>
	/// 移動の初期化
	/// </summary> 
	void InitMove()
	{
		groundEnemy.Rigidbody.velocity = Vector3.zero;
		isEnd = false;
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (isEnd == true)
		{
			isEnd = false;
			groundEnemy.InitAnimation();
			//射撃終了
			return TaskStatus.Success;
		}

		if (groundEnemy.IsChase == false)
		{
			groundEnemy.InitAnimation();
			//追跡終了
			return TaskStatus.Success;
		}

		//実行中
		return TaskStatus.Running;
	}

	public override void OnFixedUpdate()
	{
		if (groundEnemy == null)
		{
			return;
		}

		//ゲームクリアーおよびゲームオーバーシーンに遷移した際に必要となる処理
		if (groundEnemy.TargetPlayer == null)
		{
			Debug.Log("<color=red>プレイヤーが破棄されています</color>");
			groundEnemy.IsChase = false;
			return;
		}

		RotateToDirectionTarget();
		Throw();
	}

	/// <summary>
	/// ターゲットの方を向く
	/// </summary> 
	void RotateToDirectionTarget()
	{
		if (groundEnemy == null || groundEnemy.TargetPlayer == null)
		{
			return;
		}

		//対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
		Vector3 direction = groundEnemy.TargetPlayer.transform.position - groundEnemy.transform.position;
		//単純に左右だけを見るようにしたいので、y軸の数値を0にする
		direction.y = 0;
		//第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
		Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
		//↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
		groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10f);
	}

	/// <summary>
	/// 投げる
	/// </summary>
	void Throw()
	{
		count = count + Time.deltaTime;
		if (attackTime < count)
		{
			count = 0.0f;

			groundEnemy.Animator.SetTrigger("t_isGrenadeThrow");
			GrenadeSpawn();
			groundEnemy.CurrentGrenade = groundEnemy.CurrentGrenade - 1;//現在のマガジンの弾数を-1する

			isEnd = true;
			return;
		}
	}

	/// <summary>
	/// グレネードの生成
	/// </summary> 
	void GrenadeSpawn()
	{
		if (groundEnemy == null || groundEnemy.TargetPlayer == null)
		{
			return;
		}

		// キャラクターの前方にオブジェクトを生成
		Vector3 spawnPosition = this.transform.position + this.transform.forward * spawnDistance;
		spawnPosition.y = spawnPosition.y + 2.0f;
		GameObject localGameObject = UnityEngine.Object.Instantiate(grenadeGameObjectPrefab, spawnPosition, this.transform.rotation);
		Rigidbody rb = localGameObject.GetComponent<Rigidbody>();
		if (rb == null)
		{
			return;
		}

		// プレイヤー方向へ投げる
		Vector3 targetPosition = groundEnemy.TargetPlayer.transform.position;
		// 少し上を狙うことで落下を補正
		targetPosition.y += 1.0f;
		Vector3 direction = (targetPosition - spawnPosition).normalized;

		// 調整可能な投擲力（インパルス）
		float throwForce = 15.0f;
		rb.AddForce(direction * throwForce, ForceMode.Impulse);
	}
}
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using UnityEngine;

/// <summary>
/// エネミーが死んだ際のタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyDeadAction : Action
{
	GroundEnemyPresenter groundEnemy;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemyPresenter>();

		groundEnemy.Rigidbody.useGravity = false;
		//死亡アニメーションを選ぶ時に倒れる広さを調べるので、その前に自分のコライダーを無効にしておく
		groundEnemy.EnemyCollider.enabled = false;

		groundEnemy.GroundEnemyCharacterView.InitAnimation();
		groundEnemy.GroundEnemyCharacterView.PlayDeadAnimation();
		InitMove();
	}

	/// <summary>
	/// 移動の初期化
	/// </summary> 
	void InitMove()
	{
		groundEnemy.Rigidbody.velocity = Vector3.zero;
	}

	// Tick毎に呼ばれる
	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Running;
	}
}
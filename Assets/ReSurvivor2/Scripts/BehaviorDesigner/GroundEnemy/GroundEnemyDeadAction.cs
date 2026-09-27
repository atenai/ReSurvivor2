using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using UnityEngine;

/// <summary>
/// エネミーが死んだ際のタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyDeadAction : Action
{
	GroundEnemy groundEnemy;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();

		groundEnemy.InitAnimation();
		groundEnemy.Animator.applyRootMotion = true;
		groundEnemy.Animator.SetBool("b_isDead", true);
		InitMove();

		groundEnemy.Rigidbody.useGravity = false;
		groundEnemy.EnemyCollider.enabled = false;
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
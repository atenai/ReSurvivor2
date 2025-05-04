using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

		InitAnimation();
		InitMove();

		groundEnemy.Rigidbody.useGravity = false;
		groundEnemy.EnemyCollider.enabled = false;
	}

	/// <summary>
	/// アニメーションの初期化処理
	/// </summary>
	void InitAnimation()
	{
		groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
		groundEnemy.Animator.SetBool("b_isReload", false);
		groundEnemy.Animator.SetBool("b_isRifleAim", false);
		groundEnemy.Animator.SetBool("b_isRifleFire", false);
		groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
		groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
		groundEnemy.Animator.SetBool("b_isDead", true);
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
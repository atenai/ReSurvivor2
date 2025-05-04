using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーが死んだか？判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyDeadConditional : Conditional
{
	GroundEnemy groundEnemy;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();
	}

	// 更新時に呼ばれる
	public override TaskStatus OnUpdate()
	{
		if (groundEnemy.IsDead == true)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
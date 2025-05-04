using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがターゲットから特定の距離離れているか？を調べるクラス
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyWithinSpecificDistanceConditional : Conditional
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
		if (groundEnemy.CurrentMagazine <= 0)
		{
			return TaskStatus.Failure;
		}

		//距離を求める
		float sqrCurrentDistance = Vector3.SqrMagnitude(groundEnemy.TargetPlayer.transform.position - groundEnemy.transform.position);
		//Debug.Log("<color=red>sqrCurrentDistance : " + sqrCurrentDistance + "</color>");

		//↑の値が特定の範囲以上だとtrue、範囲以内だとfalse
		if (sqrCurrentDistance < groundEnemy.ShootingDistance * groundEnemy.ShootingDistance)
		{
			//範囲以内
			return TaskStatus.Success;
		}

		//範囲以上
		return TaskStatus.Failure;
	}
}
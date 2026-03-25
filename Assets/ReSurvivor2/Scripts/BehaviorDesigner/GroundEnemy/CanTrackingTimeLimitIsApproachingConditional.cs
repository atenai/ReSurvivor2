using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーの追跡制限時間が迫っている場合
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanTrackingTimeLimitIsApproachingConditional : Conditional
{
	GroundEnemy groundEnemy;
	float timeLimit = 10.0f;

	// Taskが処理される直前に呼ばれる
	public override void OnStart()
	{
		groundEnemy = this.GetComponent<GroundEnemy>();
	}

	// 更新時に呼ばれる
	public override TaskStatus OnUpdate()
	{
		// groundEnemy が破棄されている可能性をチェック
		if (groundEnemy == null)
		{
			return TaskStatus.Failure;
		}

		//ゲームクリアーおよびゲームオーバーシーンに遷移した際に必要となる処理
		if (groundEnemy.TargetPlayer == null)
		{
			Debug.Log("<color=red>プレイヤーが破棄されています</color>");
			return TaskStatus.Failure;
		}

		if (groundEnemy.CurrentMagazine <= 0)
		{
			return TaskStatus.Failure;
		}

		//追跡制限時間がせまっている場合は中身を実行する
		if (groundEnemy.ChaseCountTime <= timeLimit)
		{
			Debug.Log("<color=red>groundEnemy.ChaseCountTime : " + groundEnemy.ChaseCountTime + "</color>");
			return TaskStatus.Success;
		}

		return TaskStatus.Failure;
	}
}

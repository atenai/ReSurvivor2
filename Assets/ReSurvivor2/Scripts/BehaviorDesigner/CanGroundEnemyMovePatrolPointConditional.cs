using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがパトロールする為にプレイヤーを追跡中では無いか？を判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyMovePatrolPointConditional : Conditional
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
        if (groundEnemy.IsChase == false && groundEnemy.IsGrounded == true)
        {
            //プレイヤーを発見していない
            return TaskStatus.Success;
        }
        else
        {
            //プレイヤーを発見した
            return TaskStatus.Failure;
        }
    }
}
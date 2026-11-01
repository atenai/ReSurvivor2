using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがパトロールする為にプレイヤーを追跡中では無いか？を判別するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class CanFlyingEnemyMovePatrolPointConditional : Conditional
{
    FlyingEnemyPresenter flyingEnemy;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemy = this.GetComponent<FlyingEnemyPresenter>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //プレイヤーを発見していない
        return TaskStatus.Success;
    }
}
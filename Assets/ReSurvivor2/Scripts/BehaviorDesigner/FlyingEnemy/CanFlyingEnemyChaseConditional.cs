using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを追跡中か？を判別するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class CanFlyingEnemyChaseConditional : Conditional
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
        if (flyingEnemy.IsChase == true)
        {
            //プレイヤーを発見した
            return TaskStatus.Success;
        }
        else
        {
            //プレイヤーを発見していない
            return TaskStatus.Failure;
        }
    }
}
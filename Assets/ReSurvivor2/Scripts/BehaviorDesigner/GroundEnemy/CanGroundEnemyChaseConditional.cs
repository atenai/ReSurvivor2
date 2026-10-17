using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを追跡中か？を判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyChaseConditional : Conditional
{
    GroundEnemyPresenter groundEnemy;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemyPresenter>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.GroundEnemyModel.IsChase == true)
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
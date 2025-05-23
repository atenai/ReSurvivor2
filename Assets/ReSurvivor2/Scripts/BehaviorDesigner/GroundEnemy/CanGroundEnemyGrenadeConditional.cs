using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーの残弾数が残っていないか？を調べるクラス
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyGrenadeConditional : Conditional
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
        if (0 < groundEnemy.CurrentGrenade)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}
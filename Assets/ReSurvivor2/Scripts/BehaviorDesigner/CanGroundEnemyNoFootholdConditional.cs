using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーの移動先に足場がないか？を判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyNoFootholdConditional : Conditional
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
        if (groundEnemy.IsChase == true && groundEnemy.IsCliff == true)
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
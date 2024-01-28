using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーに当たっているか？を判別するタスク
/// </summary>
[TaskCategory("Kashiwabara")]
public class CanFlyingEnemyMoveForwardConditional : Conditional
{
    FlyingEnemyController flyingEnemyController;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (flyingEnemyController.IsMoveForward == true)
        {
            //成功
            return TaskStatus.Success;
        }
        else
        {
            //失敗
            return TaskStatus.Failure;
        }
    }
}

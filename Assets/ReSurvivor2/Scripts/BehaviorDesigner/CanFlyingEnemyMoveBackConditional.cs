using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーに当たっているか？を判別するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class CanFlyingEnemyMoveBackConditional : Conditional
{
    FlyingEnemy flyingEnemy;

    [UnityEngine.Tooltip("配列に設定したコライダーのどの番数目を当たり判定に使うか？を指定する数値")]
    [SerializeField] int checkCollisionIndex = default;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemy = this.GetComponent<FlyingEnemy>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (flyingEnemy.GetHit(checkCollisionIndex) == true)
        {
            // 成功
            Debug.Log("<color=orange>" + checkCollisionIndex + "</color>");

            flyingEnemy.IsMoveBack = true;
        }

        if (flyingEnemy.IsMoveBack == true)
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

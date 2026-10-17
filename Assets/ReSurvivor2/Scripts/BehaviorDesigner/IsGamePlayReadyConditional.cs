using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 次の処理に遷移させるか？を決めるタスク
/// </summary>
[TaskCategory("Kashiwabara")]

public class IsGamePlayReadyConditional : Conditional
{
    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (InGameManager.SingletonInstance.IsGamePlayReady == true)
        {
            //次の処理に移動して良い
            return TaskStatus.Success;
        }
        else
        {
            //次の処理に移動してはいけない
            return TaskStatus.Failure;
        }
    }
}

using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// デバッグ用に次の処理に遷移させるか？を決めるタスク
/// </summary>
[TaskCategory("TestKashiwabara")]

public class CanTestConditional : Conditional
{
    [SerializeField] bool isTest = true;

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isTest == true)
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

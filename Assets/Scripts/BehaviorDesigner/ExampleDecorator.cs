using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Decorator Task
/// 子の実行方法を制御するタスク
/// </summary>
[TaskCategory("Kashiwabara")]//カテゴリー分けする為の名前（別にこのアトリビュートは付けなくても良い）
public class ExampleDecorator : Decorator
{
    // 実行可能状態か
    public override bool CanExecute()
    {
        Debug.Log("<color=green>" + "ビヘイビアデザイナーのカスタムタスクのCanExecute" + "</color>");
        return true;
    }

    // 子Taskの最大数
    public override int MaxChildren()
    {
        Debug.Log("<color=green>" + "ビヘイビアデザイナーのカスタムタスクのMaxChildren" + "</color>");
        return 3;
    }

    // 子Taskを並列実行できるか
    public override bool CanRunParallelChildren()
    {
        Debug.Log("<color=green>" + "ビヘイビアデザイナーのカスタムタスクのCanRunParallelChildren" + "</color>");
        return false;
    }

    // 子Taskが実行されたときに呼ばれる
    public override void OnChildExecuted(int childIndex, TaskStatus childStatus)
    {
        Debug.Log("<color=green>" + "ビヘイビアデザイナーのカスタムタスクのOnChildExecuted" + "</color>");
    }

    // 子Taskが開始した時に呼ばれる
    public override void OnChildStarted()
    {
        Debug.Log("<color=green>" + "ビヘイビアデザイナーのカスタムタスクのOnChildStarted" + "</color>");
    }
}

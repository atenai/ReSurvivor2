using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// Composite Task
/// 子に複数のタスクを接続できるタスク
/// </summary>
[TaskCategory("Kashiwabara")]//カテゴリー分けする為の名前（別にこのアトリビュートは付けなくても良い）
public class ExampleComposite : Composite
{
    // 実行可能状態か
    public override bool CanExecute()
    {
        Debug.Log("<color=yellow>" + "ビヘイビアデザイナーのカスタムタスクのCanExecute" + "</color>");
        return true;
    }

    // 子Taskの最大数
    public override int MaxChildren()
    {
        Debug.Log("<color=yellow>" + "ビヘイビアデザイナーのカスタムタスクのMaxChildren" + "</color>");
        return 3;
    }

    // 子Taskを並列実行できるか
    public override bool CanRunParallelChildren()
    {
        Debug.Log("<color=yellow>" + "ビヘイビアデザイナーのカスタムタスクのCanRunParallelChildren" + "</color>");
        return false;
    }

    // 子Taskが実行されたときに呼ばれる
    public override void OnChildExecuted(int childIndex, TaskStatus childStatus)
    {
        Debug.Log("<color=yellow>" + "ビヘイビアデザイナーのカスタムタスクのOnChildExecuted" + "</color>");
    }

    // 子Taskが開始した時に呼ばれる
    public override void OnChildStarted()
    {
        Debug.Log("<color=yellow>" + "ビヘイビアデザイナーのカスタムタスクのOnChildStarted" + "</color>");
    }
}

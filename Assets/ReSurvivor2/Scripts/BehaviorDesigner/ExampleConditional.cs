using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// Conditional Task
/// 何かしらの条件判定を行いその結果をtrueかfakseで返すタスク
/// </summary>
[TaskCategory("Kashiwabara")]//カテゴリー分けする為の名前（別にこのアトリビュートは付けなくても良い）
public class ExampleConditional : Conditional
{
    // Behavior Treeが有効になった時に呼ばれる
    public override void OnAwake()
    {
        Debug.Log("<color=blue>" + "ビヘイビアデザイナーのカスタムタスクのAwake" + "</color>");
    }

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        Debug.Log("<color=blue>" + "ビヘイビアデザイナーのカスタムタスクのStart" + "</color>");
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        Debug.Log("<color=blue>" + "ビヘイビアデザイナーのカスタムタスクのUpdate" + "</color>");

        return TaskStatus.Success;
    }

    // Taskが失敗or成功した時に呼ばれる
    public override void OnEnd()
    {
        Debug.Log("<color=blue>" + "ビヘイビアデザイナーのカスタムタスクのEnd" + "</color>");
    }

    // Behavior Treeが終了した時に呼ばれる
    public override void OnBehaviorComplete()
    {
        Debug.Log("<color=blue>" + "ビヘイビアデザイナーのカスタムタスクのComplete" + "</color>");
    }
}
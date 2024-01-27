using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// Action Task
/// 単純に中身の処理だけを行うタスク
/// </summary>
[TaskCategory("Kashiwabara")]//カテゴリー分けする為の名前（別にこのアトリビュートは付けなくても良い）
public class ExampleAction : Action
{
    [SerializeField] int number;
    [SerializeField] GameObject target;


    // Behavior Treeが有効になった時に呼ばれる
    public override void OnAwake()
    {
        Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのAwake" + "</color>");
    }

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのStart" + "</color>");
        Debug.Log("<color=purple> number : " + number + "</color>");
        Debug.Log("<color=purple> target : " + target.gameObject.name + "</color>");
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのUpdate" + "</color>");

        // 成功
        return TaskStatus.Success;
        // 失敗
        //return TaskStatus.Failure;
        // 実行中
        //return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    // Taskが失敗or成功した時に呼ばれる
    public override void OnEnd()
    {
        Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのEnd" + "</color>");
    }

    // Behavior Treeが終了した時に呼ばれる
    public override void OnBehaviorComplete()
    {
        Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのComplete" + "</color>");
    }
}
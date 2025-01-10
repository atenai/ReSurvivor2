using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーが追跡範囲内か？を調べるタスク
/// 追跡範囲を指定(追跡範囲の中心にオブジェクトを置いて、そこからエネミーとの距離を測り、距離が一定以上になった場合は追跡を切り上げるという形にする)
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyTrackingRangeConditional : Conditional
{
    GroundEnemy groundEnemy;

    [UnityEngine.Tooltip("エネミーの追跡範囲")]
    [SerializeField] float range = 40.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //距離を求める
        float sqrCurrentDistance = Vector3.SqrMagnitude(groundEnemy.CenterPos.transform.position - groundEnemy.transform.position);
        //Debug.Log("<color=red>sqrCurrentDistance : " + sqrCurrentDistance + "</color>");

        //↑の値が特定の範囲以上だとfalse、範囲以内だとtrue
        if (range * range < sqrCurrentDistance)
        {
            groundEnemy.IsChase = false;
            //範囲以上
            return TaskStatus.Failure;
        }

        //範囲以内
        return TaskStatus.Success;

    }
}
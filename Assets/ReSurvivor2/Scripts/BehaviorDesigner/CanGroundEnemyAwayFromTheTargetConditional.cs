using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーとの距離からxメートル以上
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyAwayFromTheTargetConditional : Conditional
{
    GroundEnemy groundEnemy;
    float range = 4.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //距離を求める
        float sqrCurrentDistance = Vector3.SqrMagnitude(groundEnemy.Target.transform.position - groundEnemy.transform.position);
        //Debug.Log("<color=red>sqrCurrentDistance : " + sqrCurrentDistance + "</color>");

        //↑の値が特定の範囲以上だとtrue、範囲以内だとfalse
        if (sqrCurrentDistance < range * range)
        {
            //範囲以内
            return TaskStatus.Failure;
        }

        //範囲以上
        return TaskStatus.Success;
    }
}
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// オブジェクトを視認するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyJumpObjectAction1 : Action
{
    GroundEnemy groundEnemy;

    float forwardJumpForce = 25.0f;
    float upJumpForce = 200.0f;
    bool isJump = false;

    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();

        isJump = false;
    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isJump == true)
        {
            //Debug.Log("<color=red>ジャンプ成功！</color>");
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        if (isJump == false)
        {
            groundEnemy.Rigidbody.AddForce(this.transform.forward * forwardJumpForce, ForceMode.Force);
            groundEnemy.Rigidbody.AddForce(this.transform.up * upJumpForce, ForceMode.Force);
            isJump = true;
        }
    }
}
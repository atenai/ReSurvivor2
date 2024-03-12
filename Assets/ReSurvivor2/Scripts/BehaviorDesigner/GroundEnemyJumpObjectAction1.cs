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

    Vector3 jumpForce = new Vector3(10, 200, 0);
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
            groundEnemy.Rigidbody.AddForce(jumpForce * groundEnemy.Rigidbody.mass, ForceMode.Force);
            isJump = true;
        }
    }
}
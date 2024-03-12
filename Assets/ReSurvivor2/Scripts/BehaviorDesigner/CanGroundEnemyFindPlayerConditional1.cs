using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを視認しているか？を判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyFindPlayerConditional1 : Conditional
{
    GroundEnemy groundEnemy;

    [UnityEngine.Tooltip("配列に設定したコライダーのどの番数目を当たり判定に使うか？を指定する数値")]
    [SerializeField] int checkCollisionIndex = default;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.HitCollider == null)
        {
            return TaskStatus.Failure;
        }

        if (groundEnemy.GetHit(checkCollisionIndex) == true && groundEnemy.HitCollider.tag == "Player")
        {
            //Debug.Log("<color=green>プレイヤーを発見!3</color>");

            groundEnemy.IsChase = true;
            groundEnemy.ChaseCountTime = groundEnemy.ChaseTime;
            groundEnemy.Alert.gameObject.SetActive(true);
            //プレイヤーを発見した
            return TaskStatus.Success;
        }

        //プレイヤーを発見していない
        return TaskStatus.Failure;

    }
}
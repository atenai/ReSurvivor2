using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを視認しているか？を判別するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class CanGroundEnemyFindPlayerConditional : Conditional
{
    GroundEnemyPresenter groundEnemy;

    [UnityEngine.Tooltip("配列に設定したコライダーのどの番数目を当たり判定に使うか？を指定する数値")]
    [SerializeField] int checkCollisionIndex = default;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemyPresenter>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.GetHit(checkCollisionIndex) == true && groundEnemy.HitCollider.tag == "Player")
        {
            //Debug.Log("<color=green>プレイヤーを発見!3</color>");
            groundEnemy.GroundEnemyModel.IsChase = true;
            groundEnemy.GroundEnemyModel.ChaseCountTime = groundEnemy.GroundEnemyModel.ChaseTime;

        }

        //プレイヤーを発見した
        return TaskStatus.Success;

    }
}
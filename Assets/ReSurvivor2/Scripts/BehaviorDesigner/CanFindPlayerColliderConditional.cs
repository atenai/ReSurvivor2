using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// コリジョンを使いエネミーがプレイヤーを視認しているか？を判別するタスク
/// </summary> 
[TaskCategory("Kashiwabara")]
public class CanFindPlayerColliderConditional : Conditional
{
    EnemyController enemyController;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        enemyController = this.GetComponent<EnemyCollider>().EnemyController;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (this.GetComponent<EnemyCollider>().IsHit == true)
        {
            //Debug.Log("<color=orange>ビヘイビアデザイナーの当たり判定に当たった！</color>");
            enemyController.IsChase = true;
            enemyController.CountTime = enemyController.ChaseTime;
            //Debug.Log("<color=cyan>countTime : " + enemyController.CountTime + "</color>");
            enemyController.Alert.gameObject.SetActive(true);//アラートのイメージを表示
            //プレイヤーを発見した
            return TaskStatus.Success;
        }
        else
        {
            enemyController.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
            //プレイヤーを発見していない
            return TaskStatus.Failure;
        }
    }
}

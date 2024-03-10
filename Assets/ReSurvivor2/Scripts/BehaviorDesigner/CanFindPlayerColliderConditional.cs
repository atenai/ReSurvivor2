using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// コリジョンを使いエネミーがプレイヤーを視認しているか？を判別するタスク
/// </summary> 
[TaskCategory("EnemyCollider")]
public class CanFindPlayerColliderConditional : Conditional
{
    EnemyCollider enemyCollider;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        enemyCollider = this.GetComponent<EnemyCollider>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (this.GetComponent<EnemyCollider>().IsHit == true)
        {
            //Debug.Log("<color=orange>ビヘイビアデザイナーの当たり判定に当たった！</color>");
            enemyCollider.IsChase = true;
            enemyCollider.CountTime = enemyCollider.ChaseTime;
            //Debug.Log("<color=cyan>countTime : " + enemyController.CountTime + "</color>");
            enemyCollider.Alert.gameObject.SetActive(true);//アラートのイメージを表示
            //プレイヤーを発見した
            return TaskStatus.Success;
        }
        else
        {
            enemyCollider.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
            //プレイヤーを発見していない
            return TaskStatus.Failure;
        }
    }
}

using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを追跡中か？を判別するタスク
/// </summary>
[TaskCategory("EnemyCollider")]
public class CanChaseColliderConditional : Conditional
{
    EnemyCollider enemyCollider;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        if (this.GetComponent<EnemyCollider>() == true)
        {
            enemyCollider = this.GetComponent<EnemyCollider>();
        }
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (enemyCollider.IsChase == true)
        {
            if (this.GetComponent<EnemyCollider>().IsHit == true)
            {
                //Debug.Log("<color=orange>ビヘイビアデザイナーの当たり判定に当たった！</color>");
                enemyCollider.CountTime = enemyCollider.ChaseTime;
            }

            enemyCollider.CountTime = enemyCollider.CountTime - (10.0f * Time.deltaTime);
            if (enemyCollider.CountTime <= 0.0f)
            {
                enemyCollider.IsChase = false;
            }
            //Debug.Log("<color=blue>enemyController.IsChase : " + enemyController.IsChase + "</color>");
            //Debug.Log("<color=red>countTime : " + enemyController.CountTime + "</color>");

            enemyCollider.Alert.gameObject.SetActive(true);//アラートのイメージを表示
            return TaskStatus.Success;//プレイヤーを発見した
        }
        else
        {
            enemyCollider.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
            return TaskStatus.Failure;//プレイヤーを発見していない
        }
    }
}

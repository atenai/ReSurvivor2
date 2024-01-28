using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーが追跡範囲内か？を調べるタスク
/// </summary>
[TaskCategory("Kashiwabara")]
public class CanTrackingRangeConditional : Conditional
{
    [SerializeField] Transform centerPos;
    [SerializeField] float range;

    EnemyController enemyController;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        if (this.GetComponent<EnemyRayCast>() == true)
        {
            enemyController = this.GetComponent<EnemyRayCast>().EnemyController;
        }
        else if (this.GetComponent<EnemyCollider>() == true)
        {
            enemyController = this.GetComponent<EnemyCollider>().EnemyController;
        }
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //距離を求める
        float distance = Vector3.SqrMagnitude(this.transform.position - centerPos.position);
        //↑で求めた距離の絶対値を求める
        float absoluteValue = Mathf.Abs(distance);

        //Debug.Log("<color=orange>" + absoluteValue + "</color>");

        //↑の絶対値が特定の範囲以上だとfalse範囲以内だとtrue
        if (range < absoluteValue)
        {
            enemyController.IsChase = false;
            enemyController.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
            //false
            return TaskStatus.Failure;
        }
        else
        {
            //true
            return TaskStatus.Success;
        }
    }
}
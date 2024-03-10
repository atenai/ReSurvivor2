using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーが追跡範囲内か？を調べるタスク
/// </summary>
[TaskCategory("EnemyCollider")]
public class CanTrackingRangeColliderConditional : Conditional
{
    [SerializeField] float range = 40.0f;

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
        //距離を求める
        float distance = Vector3.SqrMagnitude(this.transform.position - enemyCollider.CenterPos.transform.position);
        //↑で求めた距離の絶対値を求める
        float absoluteValue = Mathf.Abs(distance);

        //Debug.Log("<color=orange>" + absoluteValue + "</color>");

        //↑の絶対値が特定の範囲以上だとfalse範囲以内だとtrue
        if (range < absoluteValue)
        {
            enemyCollider.IsChase = false;
            enemyCollider.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
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
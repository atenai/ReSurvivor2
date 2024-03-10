using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// エネミーがプレイヤーを追跡中か？を判別するタスク
/// </summary>
[TaskCategory("EnemyRayCast")]
public class CanChaseRayCastConditional : Conditional
{
    EnemyRayCast enemyRayCast;
    float rayDistance = 5.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        if (this.GetComponent<EnemyRayCast>() == true)
        {
            enemyRayCast = this.GetComponent<EnemyRayCast>();
        }
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (enemyRayCast.IsChase == true)
        {
            if (Eyesight() == true)
            {
                //Debug.Log("<color=orange>ビヘイビアデザイナーの当たり判定に当たった！</color>");
                enemyRayCast.CountTime = enemyRayCast.ChaseTime;
            }

            enemyRayCast.CountTime = enemyRayCast.CountTime - (10.0f * Time.deltaTime);
            if (enemyRayCast.CountTime <= 0.0f)
            {
                enemyRayCast.IsChase = false;
            }
            //Debug.Log("<color=blue>enemyController.IsChase : " + enemyController.IsChase + "</color>");
            //Debug.Log("<color=red>countTime : " + enemyController.CountTime + "</color>");

            enemyRayCast.Alert.gameObject.SetActive(true);//アラートのイメージを表示
            return TaskStatus.Success;//プレイヤーを発見した
        }
        else
        {
            enemyRayCast.Alert.gameObject.SetActive(false);//アラートのイメージを非表示
            return TaskStatus.Failure;//プレイヤーを発見していない
        }
    }

    bool Eyesight()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.magenta, 1);
            if (hit.collider.tag == "Player")
            {
                //Debug.Log("<color=yellow>プレイヤーを発見!</color>");
                return true;
            }
        }

        return false;
    }
}
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// レイキャストを使いエネミーがプレイヤーを視認しているか？を判別するタスク
/// </summary> 
[TaskCategory("Kashiwabara")]
public class CanFindPlayerRayCastConditional : Conditional
{
    EnemyController enemyController;
    float rayDistance = 5.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        enemyController = this.GetComponent<EnemyRayCast>().EnemyController;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (Eyesight() == true)
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

    bool Eyesight()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.yellow, 1);
            if (hit.collider.tag == "Player")
            {
                //Debug.Log("<color=yellow>プレイヤーを発見!</color>");
                return true;
            }
        }

        return false;
    }
}
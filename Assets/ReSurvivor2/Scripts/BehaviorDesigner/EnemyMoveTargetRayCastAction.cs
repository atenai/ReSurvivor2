using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// レイキャストを使いプレイヤーを追跡するタスク（レイキャストで判別して、前方に障害物がある場合はジャンプする）
/// </summary>
[TaskCategory("EnemyRayCast")]
public class EnemyMoveTargetRayCastAction : Action
{
    EnemyRayCast enemyRayCast;
    float rayDistance = 2.5f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        //Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのStart" + "</color>");
        enemyRayCast = this.GetComponent<EnemyRayCast>();
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        //Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのUpdate" + "</color>");

        if (Move() == true)
        {
            //成功
            return TaskStatus.Success;
        }
        else
        {
            //実行中
            return TaskStatus.Running;
        }
    }


    void Eyesight()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 1);
            if (hit.collider.tag == "Player")
            {
                //Debug.Log("<color=red>プレイヤーを発見!</color>");
            }
            else if (hit.collider.tag == "Object")
            {
                enemyRayCast.Rigidbody.AddForce(new Vector3(0, enemyRayCast.JumpForce, 0));
            }
        }
    }

    bool Move()
    {
        Eyesight();

        float distanceToTarget = Vector3.SqrMagnitude(this.transform.position - enemyRayCast.Target.transform.position);

        if (distanceToTarget < 2.5f)
        {
            return false;
        }

        //Debug.Log("<color=red>回転!</color>");
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        var direction = enemyRayCast.Target.transform.position - this.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;

        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, Time.deltaTime * enemyRayCast.RotationSpeed * 10f);

        this.transform.Translate(Vector3.forward * enemyRayCast.MoveSpeed * Time.deltaTime);

        return true;
    }
}

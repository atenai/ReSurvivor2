using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// コリジョンを使いプレイヤーを追跡するタスク
/// </summary>
[TaskCategory("EnemyCollider")]
public class EnemyMoveTargetColliderAction : Action
{
    EnemyCollider enemyCollider;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        //Debug.Log("<color=red>" + "ビヘイビアデザイナーのカスタムタスクのStart" + "</color>");
        enemyCollider = this.GetComponent<EnemyCollider>();
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

    bool Move()
    {
        float distanceToTarget = Vector3.SqrMagnitude(this.transform.position - enemyCollider.Target.transform.position);

        if (distanceToTarget < 2.5f)
        {
            return false;
        }

        //Debug.Log("<color=red>回転!</color>");
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        var direction = enemyCollider.Target.transform.position - this.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;

        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        var lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, Time.deltaTime * enemyCollider.RotationSpeed * 10f);

        this.transform.Translate(Vector3.forward * enemyCollider.MoveSpeed * Time.deltaTime);

        return true;
    }
}

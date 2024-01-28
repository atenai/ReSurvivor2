using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがターゲットから離れるタスク
/// </summary>
[TaskCategory("Kashiwabara")]
public class FlyingEnemyMoveBackAction : Action
{
    FlyingEnemyController flyingEnemyController;

    [SerializeField] float acceleration = 0.1f;//加速度
    [SerializeField] float stopPos = 0.5f;//エネミーが止まってほしい座標の位置

    Vector3 hitPos = new Vector3(0.0f, 0.0f, 0.0f);//ターゲット座標位置の変数系
    Vector3 velocity;//現在速度
    float hitPosDistance = 0.0f;//ターゲットの当たった座標位置とエネミーの座標位置の距離
    float spareGoalDistance = 0.0f;//ゴールの座標をすり抜けた際の移動を止める為の予備距離
    float halfDistance = 0.0f;//当たった距離の半分
    float currentDistance = 0.0f;//現在のエネミー座標位置と当たった座標位置の距離

    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）


    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;
        MoveBackStart();
    }

    public void MoveBackStart()
    {
        Vector3 heading = flyingEnemyController.Target.transform.position - this.transform.position;//ターゲットの向きベクトルと長さを出す
        heading.y = 0;//ベクトルのy軸の座標を0にする
        Vector3 oppositePosition = this.transform.position - heading;//現在にエネミーの座標位置からベクトルの方角の座標分を引く（つまり後ろに座標位置を出す）
        oppositePosition.y = flyingEnemyController.Target.transform.position.y;//ターゲットのy軸の座標を取得して↑のベクトルに入れる
        hitPos = oppositePosition;//↑で求めた座標位置がヒット座標位置になる
#if UNITY_EDITOR
        UnityEngine.Object.Instantiate(obj, hitPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
#endif
        hitPosDistance = Mathf.Abs(Vector3.Distance(this.transform.position, hitPos));
        spareGoalDistance = Mathf.Abs(hitPosDistance * 1.25f);//ゴールの座標をすり抜けた際の移動を止める為の予備距離
        halfDistance = Mathf.Abs(hitPosDistance / 2);//当たった距離の半分を取得し保持

    }

    public override TaskStatus OnUpdate()
    {
        if (MoveBackUpdate() == true)
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

    public bool MoveBackUpdate()
    {
        currentDistance = Mathf.Abs(Vector3.Distance(this.transform.position, hitPos));

        if (currentDistance <= stopPos)
        {
            //Debug.Log("<color=green>目的座標によって止まる</color>");
            velocity = Vector3.zero;
            flyingEnemyController.Rigidbody.velocity = velocity;
            flyingEnemyController.IsMoveBack = false;
            return true;
        }

        if (spareGoalDistance <= currentDistance)
        {
            //Debug.Log("<color=green>目的座標を通りこしてしまった場合のスペアの座標距離で止まる</color>");
            velocity = Vector3.zero;
            flyingEnemyController.Rigidbody.velocity = velocity;
            flyingEnemyController.IsMoveBack = false;
            return true;
        }

        if (halfDistance < currentDistance)
        {
            Debug.Log("<color=red>後ろに加速を追加</color>");
            velocity = velocity + (flyingEnemyController.Rigidbody.transform.forward * -acceleration);
            flyingEnemyController.Rigidbody.velocity = velocity;
        }
        else if (currentDistance <= halfDistance)
        {
            Debug.Log("<color=blue>減速</color>");
            velocity = velocity + (flyingEnemyController.Rigidbody.transform.forward * acceleration);
            //減速する際に速度がプラスになって後進してほしくない為
            if (velocity.z < 0)
            {
                flyingEnemyController.Rigidbody.velocity = velocity;
            }
            else
            {
                //Debug.Log("<color=green>velocity.yで止まる</color>");
                velocity = Vector3.zero;
                flyingEnemyController.Rigidbody.velocity = velocity;
                flyingEnemyController.IsMoveBack = false;
                return true;
            }
        }

        return false;
    }
}

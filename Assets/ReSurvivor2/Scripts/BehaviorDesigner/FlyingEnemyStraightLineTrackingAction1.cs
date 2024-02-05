using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがプレイヤーを直線追尾するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyStraightLineTrackingAction1 : Action
{
    FlyingEnemyController flyingEnemyController;

    [UnityEngine.Tooltip("ターゲットの座標位置")]
    Vector3 targetPos;

#if UNITY_EDITOR
    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //回転処理系の変数
    float t = 0.0f;
    Quaternion lookRotation;
    bool isRotEnd = false;

    //移動処理系の変数
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.1f;
    bool isMoveEnd = false;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;

        TargetPos();
        InitRotateToDirectionTarget();
        InitMove();
    }

    void TargetPos()
    {
        float xPos = flyingEnemyController.Target.transform.position.x;
        float yPos = flyingEnemyController.Target.transform.position.y;
        float zPos = flyingEnemyController.Target.transform.position.z;
        targetPos = new Vector3(xPos, yPos, zPos);

#if UNITY_EDITOR
        GameObject debugGameObject = UnityEngine.Object.Instantiate(obj, targetPos, Quaternion.identity);
        UnityEngine.GameObject.Destroy(debugGameObject, 5.0f);
#endif
    }

    void InitRotateToDirectionTarget()
    {
        isRotEnd = false;
        t = 0.0f;
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = targetPos - this.transform.position;

        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //2点間の角度を求める
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        lookRotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    void InitMove()
    {
        isMoveEnd = false;
        flyingEnemyController.Rigidbody.velocity = Vector3.zero;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isMoveEnd == true)
        {
            //目的地にたどりついた
            return TaskStatus.Success;
        }
        else
        {
            //移動実行中
            return TaskStatus.Running;
        }
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (isRotEnd == false)
        {
            //Debug.Log("<color=orange>回転中</color>");
            RotateToDirectionTarget();
        }
        else if (isRotEnd == true)
        {
            //回転し終えた
            Move();
        }
    }

    void RotateToDirectionTarget()
    {
        //Time.deltaTimeは0.1f
        t = t + Time.deltaTime;
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる(Lerpのtは0～1)
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, t);

        const float endRot = 1.0f;
        if (endRot <= t)
        {
            isRotEnd = true;
        }
    }

    void Move()
    {
        float currentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);
        if (currentDistance <= stopPos)
        {
            flyingEnemyController.Rigidbody.velocity = Vector3.zero;
            isMoveEnd = true;
        }

        flyingEnemyController.Rigidbody.velocity = targetPos - this.transform.position;
    }
}
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
    FlyingEnemy flyingEnemy;

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
    [SerializeField] float endPos = 0.1f;
    bool isEnd = false;

    [UnityEngine.Tooltip("エネミーの移動スピード")]
    float moveSpeed = 8.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemy = this.GetComponent<FlyingEnemy>();

        TargetPos();
        InitRotateToDirectionTarget();
        InitMove();
    }

    void TargetPos()
    {
        float xPos = flyingEnemy.Target.transform.position.x;
        float yPos = flyingEnemy.Target.transform.position.y;
        float zPos = flyingEnemy.Target.transform.position.z;
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
        flyingEnemy.Rigidbody.velocity = Vector3.zero;
        isEnd = false;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isEnd == true)
        {
            isEnd = false;
            flyingEnemy.Rigidbody.velocity = Vector3.zero;
            //目的地にたどりついた
            return TaskStatus.Success;
        }

        //移動実行中
        return TaskStatus.Running;
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
        float currentDistance = Vector3.SqrMagnitude(targetPos - flyingEnemy.transform.position);
        if (currentDistance <= endPos)
        {

            isEnd = true;
            return;
        }

        ConstantSpeed();
    }

    /// <summary>
    /// 一定の速さによる移動
    /// </summary>
    void ConstantSpeed()
    {
        //向きベクトル
        Vector3 moveDirection = targetPos - flyingEnemy.transform.position;
#if UNITY_EDITOR
        Ray ray1 = new Ray(flyingEnemy.transform.position, moveDirection);
        Debug.DrawRay(ray1.origin, ray1.direction * moveDirection.magnitude, Color.yellow);
#endif
        //Normalize()関数を使用すると2つのベクトルの長さを掛け合わせた正しい位置に自動で修正してくれる関数
        moveDirection.Normalize();

#if UNITY_EDITOR
        Ray ray2 = new Ray(flyingEnemy.transform.position, moveDirection);
        Debug.DrawRay(ray2.origin, ray2.direction * moveDirection.magnitude, Color.green);
#endif

        //正規化したベクトルに加速度をかける
        flyingEnemy.Rigidbody.velocity = moveDirection * moveSpeed;
    }
}
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがランダムに移動するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyRandomMoveAction1 : Action
{
    FlyingEnemyController flyingEnemyController;

    //ターゲット座標位置の変数
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
    [UnityEngine.Tooltip("エネミーがx軸で移動するランダム座標位置の範囲")]
    [SerializeField] int xMoveRange = 2;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    [SerializeField] int yMoveRange = 2;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    [SerializeField] int zMoveRange = 2;
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
        int xPos = UnityEngine.Random.Range(-xMoveRange, xMoveRange);
        //int yPos = UnityEngine.Random.Range(-yMoveRange, yMoveRange);
        int yPos = 0;
        int zPos = UnityEngine.Random.Range(-zMoveRange, zMoveRange); ;
        targetPos = new Vector3(this.transform.position.x + xPos, this.transform.position.y + yPos, this.transform.position.z + zPos);

#if UNITY_EDITOR
        GameObject debugGameObject = UnityEngine.Object.Instantiate(obj, targetPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
        UnityEngine.Object.Destroy(debugGameObject, 5.0f);// 5秒後にゲームオブジェクトを削除
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
        flyingEnemyController.Rigidbody.velocity = Vector3.zero;
        isMoveEnd = false;
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
        if (isRotEnd == false)
        {
            //Debug.Log("<color=orange>回転中</color>");
            RotateToDirectionTarget();
        }
        else if (isRotEnd == true)
        {
            //回転し終えた
            RandomMove();
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

    void RandomMove()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);

        if (sqrCurrentDistance <= stopPos)
        {
            //Debug.Log("<color=red>移動の終了</color>");
            flyingEnemyController.Rigidbody.velocity = Vector3.zero;
            isMoveEnd = true;
            return;
        }

        flyingEnemyController.Rigidbody.velocity = targetPos - this.transform.position;
    }
}
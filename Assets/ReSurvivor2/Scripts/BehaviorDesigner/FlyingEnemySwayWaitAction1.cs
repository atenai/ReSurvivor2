using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// フライングエネミーが揺らぎ待機をするタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemySwayWaitAction1 : Action
{
    FlyingEnemyController flyingEnemyController;

    //ターゲット座標位置の変数系
    Vector3 targetPos;
    Vector3 originPos;//原点の座標位置
    bool isOriginMove = false;
    bool isEnd = false;

#if UNITY_EDITOR
    [SerializeField] GameObject originPosObj;//プレハブをGameObject型で取得（デバッグ用）
    [SerializeField] GameObject targetPosObj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //移動処理系の変数
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.001f;
    [UnityEngine.Tooltip("エネミーがx軸で移動するランダム座標位置の範囲")]
    [SerializeField] float xMoveRange = 0.5f;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    [SerializeField] float yMoveRange = 0.5f;
    [UnityEngine.Tooltip("エネミーがz軸で移動するランダム座標位置の範囲")]
    [SerializeField] float zMoveRange = 0.5f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;

        TargetPos();
        InitMove();
    }

    public void TargetPos()
    {
        originPos = this.transform.position;
        //Debug.Log("<color=red>originPos : " + originPos + "</color>");

#if UNITY_EDITOR
        GameObject debugGameObject1 = UnityEngine.Object.Instantiate(originPosObj, originPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
        //UnityEngine.Object.Destroy(debugGameObject1, 1.0f);// 5秒後にゲームオブジェクトを削除
#endif

        float xPos = UnityEngine.Random.Range(-xMoveRange, xMoveRange);
        float yPos = UnityEngine.Random.Range(-yMoveRange, yMoveRange);
        float zPos = UnityEngine.Random.Range(-zMoveRange, zMoveRange);
        targetPos = new Vector3(this.transform.position.x + xPos, this.transform.position.y + yPos, this.transform.position.z + zPos);
        //Debug.Log("<color=orange>endPos : " + endPos + "</color>");

#if UNITY_EDITOR
        GameObject debugGameObject2 = UnityEngine.Object.Instantiate(targetPosObj, targetPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
        UnityEngine.Object.Destroy(debugGameObject2, 1.0f);// 5秒後にゲームオブジェクトを削除
#endif
    }

    public void InitMove()
    {
        flyingEnemyController.Rigidbody.velocity = Vector3.zero;
        isOriginMove = false;
        isEnd = false;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isEnd == true)
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
        if (isOriginMove == false)
        {
            MoveSway(targetPos, NextInitializingProcess);
        }
        else if (isOriginMove == true)
        {
            MoveSway(originPos, EndProcess);
        }
    }

    void MoveSway(Vector3 pos, UnityAction unityAction)
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(pos - this.transform.position);

        if (sqrCurrentDistance <= stopPos)
        {
            //Debug.Log("<color=green>目的座標によって止まる</color>");
            flyingEnemyController.Rigidbody.velocity = Vector3.zero;
            unityAction.Invoke();
        }

        flyingEnemyController.Rigidbody.velocity = pos - this.transform.position;
    }

    void NextInitializingProcess()
    {
        isOriginMove = true;
        return;
    }

    void EndProcess()
    {
        isEnd = true;
        return;
    }
}
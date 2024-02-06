using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがターゲットに近づくタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyMoveForwardAction1 : Action
{
    FlyingEnemyController flyingEnemyController;

    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.1f;
    Vector3 targetPos;
    bool isMoveEnd = false;

#if UNITY_EDITOR
    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemyController = this.GetComponent<FlyingEnemy>().FlyingEnemyController;

        TargetPos();
        InitMove();
    }

    void TargetPos()
    {
        targetPos = flyingEnemyController.Target.transform.position;//ターゲットの当たった座標位置を取得し保持
#if UNITY_EDITOR
        GameObject debugGameObject = UnityEngine.Object.Instantiate(obj, targetPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
        UnityEngine.Object.Destroy(debugGameObject, 5.0f);// 5秒後にゲームオブジェクトを削除
#endif
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
            // 実行中
            return TaskStatus.Running;
        }
    }

    public override void OnFixedUpdate()
    {
        Move();
    }

    public void Move()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);

        if (sqrCurrentDistance <= stopPos)
        {
            //Debug.Log("<color=red>移動の終了</color>");
            flyingEnemyController.Rigidbody.velocity = Vector3.zero;
            isMoveEnd = true;
            flyingEnemyController.IsMoveForward = false;
            return;
        }

        flyingEnemyController.Rigidbody.velocity = targetPos - this.transform.position;
    }
}
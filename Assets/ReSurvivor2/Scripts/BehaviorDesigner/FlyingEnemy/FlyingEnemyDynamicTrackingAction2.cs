using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// フライングエネミーがプレイヤーを動的追尾するタスク
/// </summary>
[TaskCategory("FlyingEnemy")]
public class FlyingEnemyDynamicTrackingAction2 : Action
{
    FlyingEnemy flyingEnemy;

    //ターゲット座標位置の変数
    Vector3 targetPos;

#if UNITY_EDITOR
    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //回転処理系の変数
    float t = 0.0f;
    Quaternion lookRotation;
    [UnityEngine.Tooltip("エネミーがターゲットを向くスピード")]
    [SerializeField][Range(1.0f, 4.0f)] float rotSpeed = 2.0f;

    //移動処理の変数
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float stopPos = 0.1f;
    bool isMoveEnd = false;

    [UnityEngine.Tooltip("エネミーがターゲット座標位置を再度取得するまでの時間")]
    [SerializeField][Range(0.1f, 0.5f)] float reTargetTime = 0.5f;
    float reTargetCount = 0.0f;

    [UnityEngine.Tooltip("エネミーの向きとターゲットの向きの内積")]
    float dot = 1.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        flyingEnemy = this.GetComponent<FlyingEnemy>();

        InitMove();
        TargetPos();
        InitRotateToDirectionTarget();
        isMoveEnd = false;

        reTargetCount = 0.0f;
    }

    void TargetPos()
    {
        float xPos = flyingEnemy.Target.transform.position.x;
        float yPos = flyingEnemy.Target.transform.position.y;
        float zPos = flyingEnemy.Target.transform.position.z;
        targetPos = new Vector3(xPos, yPos, zPos);

#if UNITY_EDITOR
        GameObject debugGameObject = UnityEngine.Object.Instantiate(obj, targetPos, Quaternion.identity);//プレハブを元に、インスタンスを生成（デバッグ用）
        UnityEngine.Object.Destroy(debugGameObject, 5.0f);// 5秒後にゲームオブジェクトを削除
#endif
    }

    void InitRotateToDirectionTarget()
    {
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
        PutDirectionInInnerProduct();
        ReTarget();
        RotateToDirectionTarget();
        Move();
    }

    /// <summary>
    /// 内積を使いエネミーの向きとプレイヤーの方向のコサイン数値を調べる
    /// </summary>
    void PutDirectionInInnerProduct()
    {
        Vector3 targetDir = targetPos - this.transform.position;
        dot = Vector3.Dot(this.transform.forward, targetDir);
#if UNITY_EDITOR
        Ray playerRay = new Ray(this.transform.position, this.transform.forward);
        Debug.DrawRay(playerRay.origin, playerRay.direction * 2.0f, Color.red);
        Ray targetRay = new Ray(this.transform.position, targetDir);
        Debug.DrawRay(targetRay.origin, targetRay.direction * 2.0f, Color.blue);
#endif
    }

    void ReTarget()
    {
        reTargetCount = reTargetCount + Time.deltaTime;

        if (reTargetTime <= reTargetCount)
        {
            InitMove();
            TargetPos();
            InitRotateToDirectionTarget();
            reTargetCount = 0.0f;
        }
    }

    void RotateToDirectionTarget()
    {
        //Time.deltaTimeは0.1f
        t = t + (Time.deltaTime * rotSpeed);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる(Lerpのtは0～1)
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, lookRotation, t);
    }

    void Move()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);

        if (sqrCurrentDistance <= stopPos)
        {
            Debug.Log("<color=red>移動の終了</color>");
            flyingEnemy.Rigidbody.velocity = Vector3.zero;
            isMoveEnd = true;
            return;
        }

        flyingEnemy.Rigidbody.velocity = targetPos - this.transform.position;
    }
}
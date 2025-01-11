using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// グラウンドエネミーがランダムに移動するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyRandomMoveAction : Action
{
    GroundEnemy groundEnemy;
    bool isEnd = false;

    //ターゲット座標位置の変数
    Vector3 targetPos;

#if UNITY_EDITOR
    [SerializeField] GameObject obj;//プレハブをGameObject型で取得（デバッグ用）
#endif

    //移動処理系の変数
    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float endPos = 0.1f;
    [UnityEngine.Tooltip("エネミーがx軸で移動するランダム座標位置の範囲")]
    int xMoveRange = 5;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    int yMoveRange = 0;
    [UnityEngine.Tooltip("エネミーがy軸で移動するランダム座標位置の範囲")]
    int zMoveRange = 5;
    [UnityEngine.Tooltip("エネミーの移動スピード")]
    float moveSpeed = 3.0f;

    //前進できない際の処理
    Vector3 startPos;
    float oldDistance = 0.0f;
    float endCount = 0.0f;
    [UnityEngine.Tooltip("前進できない際の強制終了時間")]
    float endTime = 1.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();

        TargetPos();
        InitAnimation();
        InitMove();
        InitEnemyCanNotMove();
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

    /// <summary>
    /// アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        groundEnemy.Animator.SetFloat("f_moveSpeed", 1.0f);
        groundEnemy.Animator.SetBool("b_isReload", false);
        groundEnemy.Animator.SetBool("b_isRifleAim", false);
        groundEnemy.Animator.SetBool("b_isRifleFire", false);
        groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
        groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
    }

    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;
        isEnd = false;
    }

    /// <summary>
    /// エネミーの移動距離が全く変わらない場合の強制終了カウントの初期化処理
    /// </summary>
    void InitEnemyCanNotMove()
    {
        startPos = groundEnemy.transform.position;
        endCount = 0.0f;
        oldDistance = 0.0f;
    }

    // 更新時に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (isEnd == true)
        {
            isEnd = false;
            //目的地にたどりついた
            return TaskStatus.Success;
        }

        if (groundEnemy.IsChase == false)
        {
            //追跡終了
            return TaskStatus.Success;
        }

        //実行中
        return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        float currentDistance = RounndFloat(Vector3.Distance(startPos, groundEnemy.transform.position));
        //Debug.Log("<color=red>currentDistance " + currentDistance + "</color>");
        //Debug.Log("<color=blue>oldDistance " + oldDistance + "</color>");
        //エネミーの移動距離が全く変わらない場合
        if (oldDistance == currentDistance)
        {
            //強制終了用のカウントを足す
            endCount = endCount + Time.deltaTime;
            //Debug.Log("<color=green>endCount " + endCount + "</color>");
        }

        //エネミーがその場から前に進めず距離計算ができない場合に、秒数で移動アクションを終了させる処理
        if (endTime <= endCount)
        {
            isEnd = true;
        }

        RotateToDirectionTarget();
        Move();

        //エネミーの移動距離が全く変わらない場合の強制終了カウント用の距離計算処理
        oldDistance = currentDistance;
    }

    /// <summary>
    /// 小数点第2以下を切り捨てる処理
    /// 12.345 を 12.3 にしたい場合
    /// </summary>
    float RounndFloat(float number)
    {
        //-------------------------------
        //0.31 を 0.3 にしたい場合（小数点第一以下切り捨ての例）
        //float calculation1 = 0.31f;
        //float calculation2 = calculation1 * 10;           //←0.31 を 一時的に10倍にし 3.1 にする
        //float calculation3 = Mathf.Floor(calculation2);   //←3.1 の .1 部分をFloorで切り捨て 3 にし
        //float result = calculation3 / 10;                 //その後10で割る事で 0.3 になり目的達成
        //-------------------------------

        float calculation1 = number;
        float calculation2 = calculation1 * 10;
        float calculation3 = Mathf.Floor(calculation2);
        float result = calculation3 / 10;

        return result;
    }

    void RotateToDirectionTarget()
    {
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = targetPos - groundEnemy.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    void Move()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(targetPos - this.transform.position);

        if (sqrCurrentDistance <= endPos)
        {
            //Debug.Log("<color=red>移動の終了</color>");
            groundEnemy.Rigidbody.velocity = Vector3.zero;
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
        Vector3 localTargetPos = targetPos;
        localTargetPos.y = groundEnemy.transform.position.y;
        //向きベクトル
        Vector3 moveDirection = localTargetPos - groundEnemy.transform.position;
#if UNITY_EDITOR
        Ray ray = new Ray(groundEnemy.transform.position, moveDirection);
        Debug.DrawRay(ray.origin, ray.direction * moveDirection.magnitude, Color.magenta);
#endif
        //Normalize()関数を使用すると2つのベクトルの長さを掛け合わせた正しい位置に自動で修正してくれる関数
        moveDirection.Normalize();

        //正規化したベクトルに加速度をかける
        groundEnemy.Rigidbody.velocity = moveDirection * moveSpeed;
    }
}
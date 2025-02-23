using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 指定座標をパトロールするタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyMovePatrolPointAction : Action
{
    GroundEnemy groundEnemy;
    bool isEnd = false;

    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float endPos = 0.5f;
    Vector3 patrolPointPos;
    [UnityEngine.Tooltip("エネミーの移動スピード")]
    float moveSpeed = 2.0f;

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

        InitAnimation();
        InitMove();
        SetPatrolPoint();
        InitEnemyCanNotMove();
    }

    /// <summary>
    /// アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        groundEnemy.Animator.SetFloat("f_moveSpeed", 0.5f);
        groundEnemy.Animator.SetBool("b_isReload", false);
        groundEnemy.Animator.SetBool("b_isRifleAim", false);
        groundEnemy.Animator.SetBool("b_isRifleFire", false);
        groundEnemy.Animator.SetBool("b_isGrenadeEquip", false);
        groundEnemy.Animator.SetBool("b_isGrenadeThrow", false);
    }

    /// <summary>
    /// 移動の初期化
    /// </summary>
    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;
        isEnd = false;
    }

    /// <summary>
    /// パトロールポイントをセットする
    /// </summary>
    void SetPatrolPoint()
    {
        //巡回地点が設定されていなければ
        if (groundEnemy.PatrolPoints.Count == 0)
        {
            //Debug.Log("<color=red>パトロールポイントが無い</color>");
            return;
        }

        //現在選択されている配列の座標を巡回地点の座標に代入
        patrolPointPos = groundEnemy.PatrolPoints[groundEnemy.PatrolPointNumber].transform.position;
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

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.IsChase == true)
        {
            //プレイヤーを発見した
            return TaskStatus.Success;
        }

        if (isEnd == true)
        {
            isEnd = false;
            //Debug.Log("<color=green>パトロール移動の終了</color>");
            groundEnemy.Rigidbody.velocity = Vector3.zero;
            NextPatrolPoint();
            //目的地にたどりついた
            return TaskStatus.Success;
        }

        //移動実行中
        return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        float currentDistance = Vector3.Distance(startPos, groundEnemy.transform.position);
        //エネミーの移動距離が全く変わらない場合
        if (oldDistance == currentDistance)
        {
            //強制終了用のカウントを足す
            endCount = endCount + Time.deltaTime;
            //Debug.Log("<color=blue>endCount " + endCount + "</color>");
        }

        //エネミーがその場から前に進めず距離計算ができない場合に、秒数で移動アクションを終了させる処理
        if (endTime <= endCount)
        {
            isEnd = true;
        }

        RotateToDirectionTarget();
        MovePatrolPoint();

        //エネミーの移動距離が全く変わらない場合の強制終了カウント用の距離計算処理
        oldDistance = currentDistance;
    }

    /// <summary>
    /// ターゲットの方を向く
    /// </summary> 
    void RotateToDirectionTarget()
    {
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = patrolPointPos - groundEnemy.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10.0f);
    }

    /// <summary>
    /// パトロールポイントに移動する
    /// </summary>
    void MovePatrolPoint()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(patrolPointPos - groundEnemy.transform.position);
        //Debug.Log("<color=red>sqrCurrentDistance: " + sqrCurrentDistance + "</color>");
        if (sqrCurrentDistance < endPos)
        {
            isEnd = true;
            return;
        }

        ConstantSpeed();
    }

    /// <summary>
    /// 次のパトロールポイントを出す
    /// </summary>
    void NextPatrolPoint()
    {
        //Debug.Log("<color=orange>今のパトロールポイント " + groundEnemy.PatrolPointNumber + "</color>");
        //配列の中から次の巡回地点を選択（必要に応じて繰り返し）
        groundEnemy.PatrolPointNumber = (groundEnemy.PatrolPointNumber + 1) % groundEnemy.PatrolPoints.Count;
        //Debug.Log("<color=yellow>次のパトロールポイント " + groundEnemy.PatrolPointNumber + "</color>");
    }

    /// <summary>
    /// 一定の速さによる移動
    /// </summary>
    void ConstantSpeed()
    {
        //向きベクトル
        Vector3 moveDirection = patrolPointPos - groundEnemy.transform.position;
#if UNITY_EDITOR
        Ray ray1 = new Ray(groundEnemy.transform.position, moveDirection);
        Debug.DrawRay(ray1.origin, ray1.direction * moveDirection.magnitude, Color.yellow);
#endif
        //Normalize()関数を使用すると2つのベクトルの長さを掛け合わせた正しい位置に自動で修正してくれる関数
        moveDirection.Normalize();

        moveDirection.y = 0.0f;
#if UNITY_EDITOR
        Ray ray2 = new Ray(groundEnemy.transform.position, moveDirection);
        Debug.DrawRay(ray2.origin, ray2.direction * moveDirection.magnitude, Color.green);
#endif

        //正規化したベクトルに加速度をかける
        groundEnemy.Rigidbody.velocity = moveDirection * moveSpeed;
    }
}
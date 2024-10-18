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

    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float endPos = 0.5f;
    Vector3 patrolPointPos;
    bool isMoveEnd = false;
    [UnityEngine.Tooltip("エネミーの移動スピード")]
    [SerializeField] float moveSpeed = 2.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();
        groundEnemy.Animator.SetBool("b_rifleAim", false);
        groundEnemy.Animator.SetFloat("f_moveSpeed", 0.5f);

        InitMove();
        SetPatrolPoint();
    }

    /// <summary>
    /// 移動の初期化
    /// </summary>
    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;

        isMoveEnd = false;
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

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.IsChase == true)
        {
            //プレイヤーを発見した
            return TaskStatus.Success;
        }

        if (isMoveEnd == true)
        {
            //目的地にたどりついた
            return TaskStatus.Success;
        }

        //移動実行中
        return TaskStatus.Running;
    }

    public override void OnFixedUpdate()
    {
        RotateToDirectionTarget();
        MovePatrolPoint();
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
            //Debug.Log("<color=red>移動の終了</color>");
            groundEnemy.Rigidbody.velocity = Vector3.zero;
            isMoveEnd = true;
            NextPatrolPoint();
            return;
        }

        ConstantSpeed();
    }

    /// <summary>
    /// 次のパトロールポイントを出す
    /// </summary>
    void NextPatrolPoint()
    {
        //配列の中から次の巡回地点を選択（必要に応じて繰り返し）
        groundEnemy.PatrolPointNumber = (groundEnemy.PatrolPointNumber + 1) % groundEnemy.PatrolPoints.Count;
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
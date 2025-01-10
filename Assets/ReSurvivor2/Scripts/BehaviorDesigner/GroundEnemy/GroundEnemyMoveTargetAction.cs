using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// プレイヤーを追跡するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyMoveTargetAction : Action
{
    GroundEnemy groundEnemy;
    bool isEnd = false;

    [UnityEngine.Tooltip("エネミーが止まってほしい座標位置の範囲")]
    [SerializeField] float endPos = 2.5f;
    [UnityEngine.Tooltip("エネミーの移動スピード")]
    float moveSpeed = 3.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();

        InitAnimation();
        InitMove();
    }

    /// <summary>
    /// アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        groundEnemy.Animator.SetBool("b_isRifleFire", false);
        groundEnemy.Animator.SetBool("b_isRifleAim", false);
        groundEnemy.Animator.SetFloat("f_moveSpeed", 1.0f);
    }

    /// <summary>
    /// 移動の初期化
    /// </summary> 
    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;
        isEnd = false;
    }

    // Tick毎に呼ばれる
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
        RotateToDirectionTarget();
        Move();
    }

    /// <summary>
    /// ターゲットの方を向く
    /// </summary> 
    void RotateToDirectionTarget()
    {
        //対象オブジェクトの位置 – 自分のオブジェクトの位置 = 対象オブジェクトの向きベクトルが求められる
        Vector3 direction = groundEnemy.Target.transform.position - groundEnemy.transform.position;
        //単純に左右だけを見るようにしたいので、y軸の数値を0にする
        direction.y = 0;
        //第一引数に向きたい方向の向きベクトルを入れてあげる、それによってどのくらい回転させれば良いのか？の数値を求めることができる
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        //↑で求めたどのくらい回転させれば良いのか？の数値を元に回転させる
        groundEnemy.transform.rotation = Quaternion.Lerp(groundEnemy.transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    /// <summary>
    /// 移動
    /// </summary>
    void Move()
    {
        float sqrCurrentDistance = Vector3.SqrMagnitude(groundEnemy.Target.transform.position - groundEnemy.transform.position);
        if (sqrCurrentDistance < endPos)
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
        Vector3 localTargetPos = groundEnemy.Target.transform.position;
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
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// リロードするタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyReloadAction : Action
{
    GroundEnemy groundEnemy;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();

        InitAnimation();
        InitMove();
        InitReload();
    }

    /// <summary>
    /// アニメーションの初期化処理
    /// </summary>
    void InitAnimation()
    {
        groundEnemy.Animator.SetBool("b_rifleFire", false);
        groundEnemy.Animator.SetBool("b_rifleAim", false);
        groundEnemy.Animator.SetFloat("f_moveSpeed", 0.0f);
        //ハンドガンのリロードアニメーションをオン
        groundEnemy.Animator.SetBool("b_isHandGunReload", true);
    }

    /// <summary>
    /// 移動の初期化
    /// </summary> 
    void InitMove()
    {
        groundEnemy.Rigidbody.velocity = Vector3.zero;
    }

    /// <summary>
    /// リロードの初期化
    /// </summary>
    void InitReload()
    {
        groundEnemy.IsHandGunReloadTimeActive = true;
    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.IsHandGunReloadTimeActive == true)
        {
            //リロード実行中
            return TaskStatus.Running;
        }

        //リロード終了
        return TaskStatus.Success;
    }

    public override void OnFixedUpdate()
    {
        Reload();
    }

    /// <summary>
    /// リロード
    /// </summary>
    void Reload()
    {
        if (groundEnemy.IsHandGunReloadTimeActive == true)//リロードがオンになったら
        {
            groundEnemy.HandGunReloadTime += Time.deltaTime;//リロードタイムをプラス

            if (groundEnemy.HandGunReloadTimeDefine <= groundEnemy.HandGunReloadTime)//リロードタイムが10以上になったら
            {
                //弾リセット
                groundEnemy.HandGunCurrentMagazine = groundEnemy.HandGunMagazineCapacity;

                groundEnemy.HandGunReloadTime = 0.0f;//リロードタイムをリセット
                groundEnemy.IsHandGunReloadTimeActive = false;//リロードのオフ

                //ハンドガンのリロードアニメーションをオフ
                groundEnemy.Animator.SetBool("b_isHandGunReload", false);
            }
        }
    }
}
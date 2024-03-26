using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーの方向を向くタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyUntraceableAction : Action
{
    GroundEnemy groundEnemy;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {
        groundEnemy = this.GetComponent<GroundEnemy>();
    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        if (groundEnemy.IsChase == true)
        {
            ChasePlayer();

            //移動実行中
            return TaskStatus.Running;
        }

        //目的地にたどりついた
        return TaskStatus.Success;
    }

    /// <summary>
    /// プレイヤーを追跡中の処理
    /// </summary>
    void ChasePlayer()
    {
        groundEnemy.ChaseCountTime -= Time.deltaTime;
        const float minTimer = 0.0f;
        if (groundEnemy.ChaseCountTime <= minTimer)
        {
            groundEnemy.IsChase = false;
            groundEnemy.Alert.gameObject.SetActive(false);
        }
    }

    public override void OnFixedUpdate()
    {
        RotateToDirectionTarget();
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
}
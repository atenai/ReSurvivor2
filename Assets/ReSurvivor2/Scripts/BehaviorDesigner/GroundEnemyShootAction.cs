using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// プレイヤーを射撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyShootAction : Action
{
    GroundEnemy groundEnemy;
    [SerializeField] GameObject shootGameObjectPrefab;
    [SerializeField] float spawnDistance = 1.0f;//キャラクターからの距離
    [SerializeField] float shootTime = 2.0f;
    [SerializeField] float lifeTime = 3.0f;
    float count = 0.0f;

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
            //移動実行中
            return TaskStatus.Running;
        }

        //目的地にたどりついた
        return TaskStatus.Success;
    }

    public override void OnFixedUpdate()
    {
        RotateToDirectionTarget();
        Shot();
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

    void Shot()
    {
        count = count + Time.deltaTime;
        if (shootTime < count)
        {
            count = 0.0f;

            // キャラクターの前方にオブジェクトを生成
            Vector3 spawnPosition = transform.position + transform.forward * spawnDistance;
            GameObject localGameObject = UnityEngine.Object.Instantiate(shootGameObjectPrefab, spawnPosition, this.transform.rotation);
            localGameObject.GetComponent<Rigidbody>().AddForce(this.transform.forward * 500.0f);
            UnityEngine.Object.Destroy(localGameObject, lifeTime);
        }

    }
}
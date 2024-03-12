using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// プレイヤーを攻撃するタスク
/// </summary>
[TaskCategory("GroundEnemy")]
public class GroundEnemyAttackAction : Action
{
    [SerializeField] GameObject attackGameObjectPrefab;
    [SerializeField] float spawnDistance = 2.0f; // キャラクターからの距離
    [SerializeField] float attackTimer = 2.0f;
    float count = 0.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {

    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        count = count + Time.deltaTime;
        if (attackTimer < count)
        {
            count = 0.0f;

            // キャラクターの前方にオブジェクトを生成
            Vector3 spawnPosition = transform.position + transform.forward * spawnDistance;
            GameObject localGameObject = UnityEngine.Object.Instantiate(attackGameObjectPrefab, spawnPosition, this.transform.rotation);
            UnityEngine.Object.Destroy(localGameObject, 0.5f);
        }

        //目的地にたどりついた
        return TaskStatus.Success;
    }

    public override void OnFixedUpdate()
    {

    }
}
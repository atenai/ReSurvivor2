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
    [SerializeField] GameObject shootGameObjectPrefab;
    [SerializeField] float spawnDistance = 1.0f; // キャラクターからの距離
    [SerializeField] float shootTime = 2.0f;
    [SerializeField] float lifeTime = 3.0f;
    float count = 0.0f;

    // Taskが処理される直前に呼ばれる
    public override void OnStart()
    {

    }

    // Tick毎に呼ばれる
    public override TaskStatus OnUpdate()
    {
        count += Time.deltaTime;
        if (shootTime < count)
        {
            count = 0.0f;

            // キャラクターの前方にオブジェクトを生成
            Vector3 spawnPosition = transform.position + transform.forward * spawnDistance;
            GameObject localGameObject = UnityEngine.Object.Instantiate(shootGameObjectPrefab, spawnPosition, this.transform.rotation);
            localGameObject.GetComponent<Rigidbody>().AddForce(this.transform.forward * 500.0f);
            UnityEngine.Object.Destroy(localGameObject, lifeTime);
        }

        //目的地にたどりついた
        return TaskStatus.Success;
    }

    public override void OnFixedUpdate()
    {

    }
}
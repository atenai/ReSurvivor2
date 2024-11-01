using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [Tooltip("爆発エフェクトのプレファブ")]
    [SerializeField] GameObject mineExplosionEffectPrefab = null;
    [Tooltip("爆発エフェクトの終了時間")]
    [SerializeField] float mineExplosionEffectDestroyTime = 1.0f;

    [Tooltip("煙エフェクトのプレファブ")]
    [SerializeField] GameObject mineSmokeEffectPrefab = null;
    [Tooltip("煙エフェクトの終了時間")]
    [SerializeField] float mineSmokeEffectDestroyTime = 1.5f;

    [Tooltip("地雷のSEプレファブ")]
    [SerializeField] GameObject mineSePrefab = null;
    [Tooltip("地雷のSE終了時間")]
    [SerializeField] float mineSeEndtime = 1.0f;

    [Tooltip("爆発のあたり判定オブジェクトを生成")]
    [SerializeField] GameObject mineExplosionColliderPrefab = null;
    float mineExplosionColliderDestroyTime = 1.0f;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player") || collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("GroundEnemy"))
        {
            Explosion();
        }
    }

    /// <summary>
    /// 爆発
    /// </summary>
    public void Explosion()
    {
        if (mineSePrefab != null)
        {
            //SEオブジェクトを生成する
            var se = Instantiate(mineSePrefab, this.gameObject.transform.position, Quaternion.identity);
            Destroy(se, mineSeEndtime);
        }

        if (mineExplosionEffectPrefab != null)
        {
            //爆発エフェクトオブジェクトを生成する	
            var effect = Instantiate(mineExplosionEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
            Destroy(effect, mineExplosionEffectDestroyTime);
        }

        if (mineSmokeEffectPrefab != null)
        {
            //煙エフェクトオブジェクトを生成する	
            var smokeEffect = Instantiate(mineSmokeEffectPrefab, this.gameObject.transform.position, Quaternion.identity);
            Destroy(smokeEffect, mineSmokeEffectDestroyTime);
        }

        if (mineExplosionColliderPrefab != null)
        {
            //爆発のあたり判定オブジェクトを生成する	
            var mineExplosionCollider = Instantiate(mineExplosionColliderPrefab, this.gameObject.transform.position, Quaternion.identity);
            Destroy(mineExplosionCollider, mineExplosionColliderDestroyTime);
        }

        Destroy(this.gameObject);
    }
}

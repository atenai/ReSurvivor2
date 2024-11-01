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

    [Tooltip("ダメージ")]
    [SerializeField] int damage = 50;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Explosion();
            Player.SingletonInstance.TakeDamage(damage);
        }
    }

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

        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾道エフェクトのクラス
/// </summary>
public class BulletEffect : MonoBehaviour
{
    float destroyTime = 10.0f;

    void Start()
    {
        Destroy(this.gameObject, destroyTime);
    }

    void OnTriggerEnter(Collider hit)
    {
        Destroy(this.gameObject);
    }
}

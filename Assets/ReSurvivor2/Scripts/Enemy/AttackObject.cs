using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 仮の攻撃にキューブのオブジェクトを出す際の当たり判定の処理
/// </summary>
public class AttackObject : MonoBehaviour
{
    [SerializeField] public float damage = 30.0f;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("プレイヤーにダメージ！");
            collider.gameObject.GetComponent<Player>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}
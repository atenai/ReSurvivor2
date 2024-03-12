using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 仮の攻撃にキューブのオブジェクトを出す際の当たり判定の処理
/// </summary>
public class AttackObject : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            Debug.Log("プレイヤーに攻撃！");
        }
    }
}
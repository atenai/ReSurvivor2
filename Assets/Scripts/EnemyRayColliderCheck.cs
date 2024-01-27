using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ビヘイビアデザイナーに当たり判定の情報を送る為のクラス
/// </summary>
public class EnemyRayColliderCheck : MonoBehaviour
{
    public UnityEvent<Collider> OnTriggerEnterHit;
    public UnityEvent<Collider> OnTriggerStayHit;
    public UnityEvent<Collider> OnTriggerExitHit;

    void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("<color=red>OnTriggerEnter</color>");
        OnTriggerEnterHit.Invoke(collider);
    }

    void OnTriggerStay(Collider collider)
    {
        //Debug.Log("<color=blue>OnTriggerStay</color>");
        OnTriggerStayHit.Invoke(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        //Debug.Log("<color=green>OnTriggerExit</color>");
        OnTriggerExitHit.Invoke(collider);
    }
}

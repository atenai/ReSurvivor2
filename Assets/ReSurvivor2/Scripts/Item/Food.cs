using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (Player.SingletonInstance.MaxFood <= Player.SingletonInstance.CurrentFood)
            {
                return;
            }

            Player.SingletonInstance.AcquireFood();

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}
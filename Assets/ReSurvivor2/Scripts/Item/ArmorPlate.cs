using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPlate : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (Player.SingletonInstance.MaxArmorPlate <= Player.SingletonInstance.CurrentArmorPlate)
            {
                return;
            }

            Player.SingletonInstance.AcquireArmorPlate();

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}

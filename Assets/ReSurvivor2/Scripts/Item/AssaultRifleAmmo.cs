using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleAmmo : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (PlayerCamera.SingletonInstance.MaxAssaultRifleAmmo <= PlayerCamera.SingletonInstance.CurrentAssaultRifleAmmo)
            {
                return;
            }

            PlayerCamera.SingletonInstance.AcquireAssaultRifleAmmo();

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}

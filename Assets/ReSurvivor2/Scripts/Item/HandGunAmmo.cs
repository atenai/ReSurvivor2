using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGunAmmo : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (PlayerCamera.SingletonInstance.MaxHandGunAmmo <= PlayerCamera.SingletonInstance.CurrentHandGunAmmo)
            {
                return;
            }

            PlayerCamera.SingletonInstance.AcquireHandGunAmmo();

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}

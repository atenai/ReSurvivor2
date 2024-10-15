using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGunAmmo : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (PlayerCamera.SingletonInstance.MaxShotGunAmmo <= PlayerCamera.SingletonInstance.CurrentShotGunAmmo)
            {
                return;
            }

            PlayerCamera.SingletonInstance.AcquireShotGunAmmo();

            Destroy(this.gameObject);//このオブジェクトを削除            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガンの弾薬
/// </summary>
public class HandGunAmmo : MonoBehaviour
{
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			if (PlayerCamera.SingletonInstance.HandGun.MaxHandGunAmmo <= PlayerCamera.SingletonInstance.HandGun.CurrentHandGunAmmo)
			{
				return;
			}

			PlayerCamera.SingletonInstance.HandGun.AcquireHandGunAmmo();

			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

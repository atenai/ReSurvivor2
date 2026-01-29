using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフルの弾薬
/// </summary>
public class AssaultRifleAmmo : MonoBehaviour
{
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			if (PlayerCamera.SingletonInstance.AssaultRifle.MaxAssaultRifleAmmo <= PlayerCamera.SingletonInstance.AssaultRifle.CurrentAmmo)
			{
				return;
			}

			PlayerCamera.SingletonInstance.AssaultRifle.AcquireAmmo();

			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

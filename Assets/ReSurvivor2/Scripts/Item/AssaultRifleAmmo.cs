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
			//弾薬がフルの状態でこのオブジェクトにあたってもデストロイさせない為
			if (PlayerCamera.SingletonInstance.GetGunFacade.AssaultRifle.MaxAssaultRifleAmmo <= PlayerCamera.SingletonInstance.GetGunFacade.AssaultRifle.CurrentAmmo)
			{
				return;
			}
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireAssaultRifleAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

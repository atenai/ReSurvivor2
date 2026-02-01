using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ショットガンの弾薬
/// </summary>
public class ShotGunAmmo : MonoBehaviour
{
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			//弾薬がフルの状態でこのオブジェクトにあたってもデストロイさせない為
			if (PlayerCamera.SingletonInstance.GetGunFacade.ShotGun.MaxShotGunAmmo <= PlayerCamera.SingletonInstance.GetGunFacade.ShotGun.CurrentAmmo)
			{
				return;
			}
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireShotGunAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

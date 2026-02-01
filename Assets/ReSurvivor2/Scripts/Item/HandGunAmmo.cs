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
			//弾薬がフルの状態でこのオブジェクトにあたってもデストロイさせない為
			if (PlayerCamera.SingletonInstance.GetGunFacade.HandGun.MaxHandGunAmmo <= PlayerCamera.SingletonInstance.GetGunFacade.HandGun.CurrentAmmo)
			{
				return;
			}
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireHandGunAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

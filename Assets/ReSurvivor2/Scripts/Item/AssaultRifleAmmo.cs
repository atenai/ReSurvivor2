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
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireAssaultRifleAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

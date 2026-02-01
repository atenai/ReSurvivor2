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
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireHandGunAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

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
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireShotGunAmmo();
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

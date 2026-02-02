using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アサルトライフルの弾薬
/// </summary>
public class AssaultRifleAmmo : MonoBehaviour
{
	[Tooltip("弾薬の追加数")]
	[SerializeField] int amount = 10;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.AssaultRifle, amount, DestroySelf);
		}
	}

	/// <summary>
	/// このゲームオブジェクトを削除する
	/// </summary>
	void DestroySelf()
	{
		Destroy(this.gameObject);
	}
}

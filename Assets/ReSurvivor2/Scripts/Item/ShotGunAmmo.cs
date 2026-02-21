using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ショットガンの弾薬
/// </summary>
public class ShotGunAmmo : MonoBehaviour
{
	[Tooltip("弾薬の追加数")]
	[SerializeField] int amount = 10;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			PlayerCamera.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.ShotGun, amount, DestroySelf);
		}
	}

	/// <summary>
	/// このゲームオブジェクトを削除する
	/// </summary>
	void DestroySelf()
	{
		ScreenUI.SingletonInstance.ItemOutPutLog.OutputLog("+ShotGunAmmo");
		Destroy(this.gameObject);
	}
}

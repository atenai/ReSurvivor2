using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハンドガンの弾薬
/// </summary>
public class HandGunAmmo : MonoBehaviour
{
	[Tooltip("弾薬の追加数")]
	[SerializeField] int amount = 10;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			PlayerCameraManager.SingletonInstance.GetGunFacade.AcquireAmmo(EnumManager.GunTYPE.HandGun, amount, DestroySelf);
		}
	}

	/// <summary>
	/// このゲームオブジェクトを削除する
	/// </summary>
	void DestroySelf()
	{
		ScreenUIManager.SingletonInstance.ScreenUIPresenter.ScreenUIView.ItemOutPutLog.OutputLog("+HandGunAmmo");
		Destroy(this.gameObject);
	}
}

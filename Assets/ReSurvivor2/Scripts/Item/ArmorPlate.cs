using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アーマープレート
/// </summary>
public class ArmorPlate : MonoBehaviour
{
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			if (PlayerManager.SingletonInstance.PlayerModel.MaxArmorPlate <= PlayerManager.SingletonInstance.PlayerModel.CurrentArmorPlate)
			{
				return;
			}

			PlayerManager.SingletonInstance.PlayerModel.AcquireArmorPlate((armorPlate) => PlayerManager.SingletonInstance.PlayerUIView.StartTextArmorPlate(armorPlate));

			ScreenUIManager.SingletonInstance.ScreenUIPresenter.ScreenUIView.ItemOutPutLog.OutputLog("+ArmorPlate");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

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
			if (PlayerManagerPresenter.SingletonInstance.PlayerModel.MaxArmorPlate <= PlayerManagerPresenter.SingletonInstance.PlayerModel.CurrentArmorPlate)
			{
				return;
			}

			PlayerManagerPresenter.SingletonInstance.PlayerModel.AcquireArmorPlate((armorPlate) => PlayerManagerPresenter.SingletonInstance.PlayerUIView.StartTextArmorPlate(armorPlate));

			ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.ItemOutPutLog.OutputLog("+ArmorPlate");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

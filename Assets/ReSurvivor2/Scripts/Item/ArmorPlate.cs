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
			if (PlayerManager.SingletonInstance.MaxArmorPlate <= PlayerManager.SingletonInstance.CurrentArmorPlate)
			{
				return;
			}

			PlayerManager.SingletonInstance.AcquireArmorPlate();

			ScreenUI.SingletonInstance.ItemOutPutLog.OutputLog("+ArmorPlate");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

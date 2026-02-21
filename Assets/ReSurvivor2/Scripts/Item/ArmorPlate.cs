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
			if (Player.SingletonInstance.MaxArmorPlate <= Player.SingletonInstance.CurrentArmorPlate)
			{
				return;
			}

			Player.SingletonInstance.AcquireArmorPlate();

			ScreenUI.SingletonInstance.ItemOutPutLog.OutputLog("+ArmorPlate");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

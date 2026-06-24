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
			if (InGameManager.SingletonInstance.PlayerManager.MaxArmorPlate <= InGameManager.SingletonInstance.PlayerManager.CurrentArmorPlate)
			{
				return;
			}

			InGameManager.SingletonInstance.PlayerManager.AcquireArmorPlate();

			InGameManager.SingletonInstance.ScreenUIManager.ItemOutPutLog.OutputLog("+ArmorPlate");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}

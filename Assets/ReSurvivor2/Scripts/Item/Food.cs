using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 食料
/// </summary>
public class Food : MonoBehaviour
{
	void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("Player"))
		{
			if (PlayerManager.SingletonInstance.MaxFood <= PlayerManager.SingletonInstance.CurrentFood)
			{
				return;
			}

			PlayerManager.SingletonInstance.AcquireFood();

			ScreenUIManager.SingletonInstance.ItemOutPutLog.OutputLog("+Food");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
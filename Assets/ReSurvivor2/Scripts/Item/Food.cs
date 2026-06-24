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
			if (InGameManager.SingletonInstance.PlayerManager.MaxFood <= InGameManager.SingletonInstance.PlayerManager.CurrentFood)
			{
				return;
			}

			InGameManager.SingletonInstance.PlayerManager.AcquireFood();

			InGameManager.SingletonInstance.ScreenUIManager.ItemOutPutLog.OutputLog("+Food");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
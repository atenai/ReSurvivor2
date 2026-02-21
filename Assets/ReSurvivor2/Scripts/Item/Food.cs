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
			if (Player.SingletonInstance.MaxFood <= Player.SingletonInstance.CurrentFood)
			{
				return;
			}

			Player.SingletonInstance.AcquireFood();

			ScreenUI.SingletonInstance.ItemOutPutLog.OutputLog("+Food");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
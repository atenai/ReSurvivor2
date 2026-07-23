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
			if (PlayerManager.SingletonInstance.PlayerModel.MaxFood <= PlayerManager.SingletonInstance.PlayerModel.CurrentFood)
			{
				return;
			}

			PlayerManager.SingletonInstance.PlayerModel.AcquireFood();

			ScreenUIManager.SingletonInstance.ScreenUIPresenter.ScreenUIView.ItemOutPutLog.OutputLog("+Food");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
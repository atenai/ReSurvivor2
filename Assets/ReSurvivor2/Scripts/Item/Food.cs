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
			if (PlayerManagerPresenter.SingletonInstance.PlayerModel.MaxFood <= PlayerManagerPresenter.SingletonInstance.PlayerModel.CurrentFood)
			{
				return;
			}

			PlayerManagerPresenter.SingletonInstance.PlayerModel.AcquireFood((food) => PlayerManagerPresenter.SingletonInstance.PlayerUIView.SetTextFood(food));

			ScreenUIManager.SingletonInstance.ScreenUIPresenter.ScreenUIView.ItemOutPutLog.OutputLog("+Food");
			Destroy(this.gameObject);//このオブジェクトを削除            
		}
	}
}
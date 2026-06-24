using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガンモデル
/// </summary>
public abstract class GunModelBase : MonoBehaviour
{
	void Start()
	{

	}

	void Update()
	{
		if (InGameManager.SingletonInstance.ScreenUIManager.IsPause == true)
		{
			return;
		}

		if (InGameManager.SingletonInstance.ScreenUIManager.IsComputerMenuActive == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理
	}
}

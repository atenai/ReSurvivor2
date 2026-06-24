using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンピューター
/// </summary>
/// <remarks>
/// Computer.cs ⇒ InGameManager.cs ⇒ UI.cs
/// </remarks>
public class Computer : MonoBehaviour
{
	readonly string TAG_PLAYER = "Player";

	[Tooltip("このコンピューターの名前")]
	[SerializeField] EnumManager.StageTYPE thisComputerStageNumber;

	/// <summary>
	/// プレイヤーがコンピューターにヒットしているかどうか
	/// </summary>
	bool isPlayerHit = false;

	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.CompareTag(TAG_PLAYER))
		{
			//Debug.Log("<color=yellow>プレイヤーがコンピューターに近づいた</color>");
			isPlayerHit = true;
		}
		else
		{
			Debug.Log("<color=red>プレイヤーじゃない</color>");
		}
	}

	void OnTriggerExit(Collider collider)
	{
		if (collider.gameObject.CompareTag(TAG_PLAYER))
		{
			//Debug.Log("<color=yellow>プレイヤーがコンピューターから離れた</color>");
			isPlayerHit = false;
		}
	}

	void Update()
	{
		//ポーズ中は切り上げる
		if (ScreenUIManager.SingletonInstance.IsPause == true)
		{
			return;
		}

		//コンピュータを使用中は切り上げる
		if (ScreenUIManager.SingletonInstance.IsComputerMenuActive == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		if (isPlayerHit == true)
		{
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("XInput Y"))
			{
				//Debug.Log("<color=green>Fキー</color>");
				MissionManager.SingletonInstance.Mission(thisComputerStageNumber);
			}
		}
	}
}

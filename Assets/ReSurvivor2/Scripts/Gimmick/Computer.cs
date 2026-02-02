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
	[Tooltip("このコンピューターの名前")]
	[SerializeField] EnumManager.ComputerTYPE thisComputerName;

	/// <summary>
	/// 当たり判定
	/// </summary>
	/// <param name="collider"></param>
	void OnTriggerStay(Collider collider)
	{
		if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("XInput Y"))
		{
			Debug.Log("<color=green>Fキー</color>");

			if (collider.gameObject.CompareTag("Player"))
			{
				Debug.Log("<color=yellow>プレイヤー</color>");
				InGameManager.SingletonInstance.Mission(thisComputerName);
			}
			else
			{
				Debug.Log("<color=red>プレイヤーじゃない</color>");
			}
		}
	}
}

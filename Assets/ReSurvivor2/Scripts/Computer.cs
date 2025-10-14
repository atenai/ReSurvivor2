using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// コンピューター
/// </summary>
public class Computer : MonoBehaviour
{
	[Tooltip("このコンピューターの名前")]
	[SerializeField] InGameManager.ComputerTYPE thisComputerName;

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
				Save();
				Mission();
			}
			else
			{
				Debug.Log("<color=red>プレイヤーじゃない</color>");
			}
		}
	}

	/// <summary>
	/// ミッション
	/// </summary>
	void Mission()
	{
		if (InGameManager.SingletonInstance.IsMissionActive == false)
		{
			UI.SingletonInstance.ShowComputerMenu(thisComputerName);
		}
		else if (InGameManager.SingletonInstance.IsMissionActive == true)
		{
			if (thisComputerName == InGameManager.SingletonInstance.TargetComputerName)
			{
				Debug.Log("<color=blue>ミッション終了</color>");
				InGameManager.SingletonInstance.IsMissionActive = false;

				//ゲームクリアー処理
				SceneManager.LoadScene("GameClear");
			}
		}
	}

	/// <summary>
	/// セーブ
	/// </summary>
	private void Save()
	{
		Debug.Log("<color=cyan>セーブ</color>");
		PlayerPrefs.SetInt("KeyItem1", InGameManager.SingletonInstance.KeyItem1);
		PlayerPrefs.Save();
	}
}

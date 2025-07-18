using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Computer : MonoBehaviour
{
	[Tooltip("このコンピューターの名前")]
	[SerializeField] InGameManager.ComputerTYPE thisComputerName;

	void OnTriggerStay(Collider collider)
	{
		if (collider.gameObject.CompareTag("Player"))
		{
			Debug.Log("<color=yellow>プレイヤー</color>");
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("XInput Y"))
			{
				Debug.Log("<color=green>Fキー</color>");
				Save();

				if (InGameManager.SingletonInstance.IsMissionActive == false)
				{
					//↓ここをミッションごとに変えるようにする必要がある
					//各ComputerTYPEに紐づいたミッションリストを取得して、そこから選択されたミッション内容を↓に反映すればいい
					var result = InGameManager.SingletonInstance.MissionSerch(thisComputerName);
					if (result != null)
					{
						Debug.Log("<color=red>ミッション開始</color>");
						InGameManager.SingletonInstance.IsMissionActive = true;
						InGameManager.SingletonInstance.TargetComputerName = result.TargetComputerName;
						Player.SingletonInstance.Minute = result.Minute;
						Player.SingletonInstance.Seconds = result.Seconds;
					}
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

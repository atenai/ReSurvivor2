using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
	[SerializeField] EnumManager.ComputerTYPE thisComputerName;

	bool isHit = false;

	/// <summary>
	/// コンピューターメニューのミッションリスト
	/// </summary>


	void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.CompareTag(TAG_PLAYER))
		{
			Debug.Log("<color=yellow>プレイヤーがコンピューターに近づいた</color>");
			isHit = true;
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
			Debug.Log("<color=yellow>プレイヤーがコンピューターから離れた</color>");
			isHit = false;
		}
	}

	void Start()
	{
		ScreenUI.SingletonInstance.InitComputerMenuMissionList(MissionSerchList());
		ScreenUI.SingletonInstance.DestroyAllMailListContent();
		ScreenUI.SingletonInstance.AddMailListContent();
	}

	/// <summary>
	/// ミッション検索リスト
	/// </summary>
	/// <param name="computerTYPE">コンピュータータイプ</param>
	/// <returns>ミッションリスト</returns>
	List<MasterMissionEntity> MissionSerchList()
	{
		//引数のコンピュータータイプからマスターデータのミッション情報を照らし合わせて、StartComputerNameと一致したコンピュータータイプの情報を全て取得する
		List<MasterMissionEntity> result = InGameManager.SingletonInstance.MasterMission.Sheet1.Where((MasterMissionEntity excelLine) => excelLine.StartComputerName == thisComputerName).ToList();

		if (result == null)
		{
			Debug.Log("<color=red>マスターデータのミッション情報がnull</color>");
		}

		return result;
	}


	void Update()
	{
		if (isHit == true)
		{
			if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("XInput Y"))
			{
				Debug.Log("<color=green>Fキー</color>");
				InGameManager.SingletonInstance.Mission(thisComputerName);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 各ステージ用のマネージャー
/// </summary>
public class StageManager : MonoBehaviour
{
	[Tooltip("ステージ名")]
	[SerializeField] EnumManager.StageTYPE currentStage;
	public EnumManager.StageTYPE CurrentStage
	{
		get { return currentStage; }
		set { currentStage = value; }
	}

	[Tooltip("プレイヤーリスポーンポイント")]
	[SerializeField] Transform playerRespawnPoint = null;//必ず設定するようにする、設定されていなければエラーログを出すようにする

	void Awake()
	{
		if (playerRespawnPoint != null)
		{
			PlayerManager.SingletonInstance.SetPlayerRespawnPoint(playerRespawnPoint.position, playerRespawnPoint.rotation);
		}
		else
		{
			UnityEngine.Debug.LogError("リスポーンポイントが設定されていません。");
		}
	}

	void Start()
	{
		CreateMission();
		ScreenUI.SingletonInstance.MapUI.SetCurrentPlayerStageNumber((int)currentStage);
		StartCoroutine(ScreenUI.SingletonInstance.FadeIn());
		StartCoroutine(InGameManager.SingletonInstance.PreloadScenesCoroutine());
	}

	void CreateMission()
	{
		//ミッションの作成
		MissionManager.SingletonInstance.CachedMissionList = MissionManager.SingletonInstance.MissionSerchList(currentStage);
		foreach (MasterMissionEntity mission in MissionManager.SingletonInstance.CachedMissionList)
		{
			UnityEngine.Debug.Log("<color=cyan>ミッション名：" + mission.MissionName + "</color>");
		}
		ScreenUI.SingletonInstance.InitComputerMenuMissionList(MissionManager.SingletonInstance.CachedMissionList);
		ScreenUI.SingletonInstance.DestroyAllMailListContent();
		ScreenUI.SingletonInstance.AddMailListContent();
	}

	void Update()
	{

	}
}

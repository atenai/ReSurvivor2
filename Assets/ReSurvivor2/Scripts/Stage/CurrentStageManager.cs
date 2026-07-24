using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 各現在のステージを管理するマネージャークラス
/// </summary>
public class CurrentStageManager : MonoBehaviour
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
			PlayerManager.SingletonInstance.PlayerModel.SetPlayerRespawnPoint(playerRespawnPoint.position, playerRespawnPoint.rotation);
		}
		else
		{
			UnityEngine.Debug.LogError("リスポーンポイントが設定されていません。");
		}
	}

	void Start()
	{
		CreateMission();
		ScreenUIManager.SingletonInstance.ScreenUIPresenter.ScreenUIView.MapUI.SetCurrentPlayerStageNumber((int)currentStage);
		StartCoroutine(ScreenUIManager.SingletonInstance.ScreenUIPresenter.FadeIn());
		StartCoroutine(ChangeSceneManager.SingletonInstance.PreloadScenesCoroutine());
	}

	void CreateMission()
	{
		//ミッションの作成
		MissionManager.SingletonInstance.CachedMissionList = MissionManager.SingletonInstance.MissionSerchList(currentStage);
		foreach (MasterMissionEntity mission in MissionManager.SingletonInstance.CachedMissionList)
		{
			UnityEngine.Debug.Log("<color=cyan>ミッション名：" + mission.MissionName + "</color>");
		}

		ScreenUIManager.SingletonInstance.ScreenUIPresenter.InitComputerMenuMissionList(MissionManager.SingletonInstance.CachedMissionList);
		ScreenUIManager.SingletonInstance.ScreenUIPresenter.DestroyAllMailListContent();
		ScreenUIManager.SingletonInstance.ScreenUIPresenter.AddMailListContent();
	}

	void Update()
	{

	}
}

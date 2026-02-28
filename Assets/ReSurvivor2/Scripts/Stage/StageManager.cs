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
	[SerializeField] EnumManager.StageTYPE stage;
	public EnumManager.StageTYPE Stage
	{
		get { return stage; }
		set { stage = value; }
	}

	[Tooltip("プレイヤーリスポーンポイント")]
	[SerializeField] Transform playerRespawnPoint = null;//必ず設定するようにする、設定されていなければエラーログを出すようにする

	void Awake()
	{
		if (playerRespawnPoint != null)
		{
			Player.SingletonInstance.SetPlayerRespawnPoint(playerRespawnPoint.position, playerRespawnPoint.rotation);
		}
		else
		{
			UnityEngine.Debug.LogError("リスポーンポイントが設定されていません。");
		}
	}

	void Start()
	{
		ScreenUI.SingletonInstance.MapUI.SetCurrentPlayerStageNumber((int)stage);
		StartCoroutine(ScreenUI.SingletonInstance.FadeIn());
	}

	void Update()
	{

	}
}

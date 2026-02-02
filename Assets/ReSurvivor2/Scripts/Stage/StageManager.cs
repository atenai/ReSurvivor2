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
	[SerializeField] Transform playerRespawnPoint;

	void Start()
	{
		StartCoroutine(ScreenUI.SingletonInstance.FadeIn());
		Player.SingletonInstance.SetPlayerRespawnPoint(playerRespawnPoint.position, playerRespawnPoint.rotation);
		ScreenUI.SingletonInstance.MapUI.SetCurrentPlayerStageNumber((int)stage);
	}

	void Update()
	{

	}
}

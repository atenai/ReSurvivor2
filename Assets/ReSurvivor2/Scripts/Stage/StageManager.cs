using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各ステージ用のマネージャー
/// </summary>
public class StageManager : MonoBehaviour
{
	public enum StageTYPE
	{
		Stage0 = 0,
		Stage1 = 1,
		Stage2 = 2,
		Stage3 = 3,
		Stage4 = 4,
		Stage5 = 5,
	}

	[Tooltip("ステージ名")]
	[SerializeField] StageTYPE stage;
	public StageTYPE Stage
	{
		get { return stage; }
		set { stage = value; }
	}

	void Start()
	{
		StartCoroutine(UI.SingletonInstance.FadeIn());

		UI.SingletonInstance.MapUI.SetCurrentPlayerStageNumber((int)stage);
	}

	void Update()
	{

	}
}

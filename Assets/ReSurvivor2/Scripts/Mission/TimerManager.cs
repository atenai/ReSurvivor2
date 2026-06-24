using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミッションの制限時間を管理するマネージャークラス
/// </summary>
public class TimerManager : MonoBehaviour
{
	[Tooltip("totalTimeは秒で集計されている")]
	float totalTime = 0.0f;

	[Tooltip("分")]
	int minute = 10;
	public int Minute
	{
		get { return minute; }
		set { minute = value; }
	}
	[Tooltip("秒")]
	float seconds = 0.0f;
	public float Seconds
	{
		get { return seconds; }
		set { seconds = value; }
	}

	void Awake()
	{

	}

	public void AfterUpdate()
	{
		UpdateTimerSystem();
	}

	/// <summary>
	/// 時間経過の処理
	/// </summary>
	void UpdateTimerSystem()
	{
		if (MissionManager.SingletonInstance.IsMissionActive == false)
		{
			InGameManager.SingletonInstance.PlayerManager.PlayerUI.TextTimer.text = "--" + ":" + "--";
			return;
		}

		totalTime = (minute * 60) + seconds;
		totalTime = totalTime - Time.deltaTime;

		minute = (int)totalTime / 60;
		seconds = totalTime - (minute * 60);

		if (minute <= 0 && seconds <= 0.0f)
		{
			//ゲームオーバー処理
			InGameManager.SingletonInstance.PlayerManager.PlayerUI.TextTimer.text = "00" + ":" + "00";
			ChangeSceneManager.SingletonInstance.GameOver();
		}
		else
		{
			InGameManager.SingletonInstance.PlayerManager.PlayerUI.TextTimer.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
		}
	}
}

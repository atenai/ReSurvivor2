using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミッションの制限時間を管理するマネージャークラス
/// </summary>
public class TimerManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static TimerManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static TimerManager SingletonInstance => singletonInstance;

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
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、Playerのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
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
			PlayerManager.SingletonInstance.PlayerUIView.TextTimer.text = "--" + ":" + "--";
			return;
		}

		totalTime = (minute * 60) + seconds;
		totalTime = totalTime - Time.deltaTime;

		minute = (int)totalTime / 60;
		seconds = totalTime - (minute * 60);

		if (minute <= 0 && seconds <= 0.0f)
		{
			//ゲームオーバー処理
			PlayerManager.SingletonInstance.PlayerUIView.TextTimer.text = "00" + ":" + "00";
			ChangeSceneManager.SingletonInstance.GameOver();
		}
		else
		{
			PlayerManager.SingletonInstance.PlayerUIView.TextTimer.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
		}
	}
}

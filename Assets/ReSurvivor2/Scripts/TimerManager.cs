using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ミッションの制限時間を管理するクラス
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

	void Start()
	{

	}

	void Update()
	{
		//ポーズ中は切り上げる
		if (ScreenUI.SingletonInstance.IsPause == true)
		{
			return;
		}

		//コンピュータを使用中は切り上げる
		if (ScreenUI.SingletonInstance.IsComputerMenuActive == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		UpdateTimerSystem();
	}

	/// <summary>
	/// 時間経過の処理
	/// </summary>
	void UpdateTimerSystem()
	{
		if (InGameManager.SingletonInstance.IsMissionActive == false)
		{
			Player.SingletonInstance.PlayerUI.TextTimer.text = "--" + ":" + "--";
			return;
		}

		totalTime = (minute * 60) + seconds;
		totalTime = totalTime - Time.deltaTime;

		minute = (int)totalTime / 60;
		seconds = totalTime - (minute * 60);

		if (minute <= 0 && seconds <= 0.0f)
		{
			//ゲームオーバー処理
			Player.SingletonInstance.PlayerUI.TextTimer.text = "00" + ":" + "00";
			InGameManager.SingletonInstance.GameOver();
		}
		else
		{
			Player.SingletonInstance.PlayerUI.TextTimer.text = minute.ToString("00") + ":" + ((int)seconds).ToString("00");
		}
	}
}

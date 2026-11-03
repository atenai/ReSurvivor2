using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

	[Tooltip("ミッションの残り時間が少ないとみなす秒数（この秒数以下になると、プレイヤーUIのタイマーの文字を赤くする）")]
	const float LowTimeSeconds = 60.0f;

	[Tooltip("ミッションの制限時間を数えているか？（ミッション中か？）")]
	bool isTimerActive = false;
	public bool IsTimerActive => isTimerActive;

	/// <summary>
	/// ミッションの残り時間が少ないか？
	/// </summary>
	public bool IsTimeLow => totalTime <= LowTimeSeconds;

	/// <summary>
	/// ミッションの制限時間が切れたか？
	/// </summary>
	public bool IsTimeUp => minute <= 0 && seconds <= 0.0f;

	//※制限時間を数えたことを知らせるだけで、タイマーの表示は、知らせを受け取った見た目の側が出す
	/// <summary>
	/// 制限時間を数えた時の知らせ（毎フレーム）
	/// </summary>
	public event UnityAction OnTimerUpdated;

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
			isTimerActive = false;
			OnTimerUpdated?.Invoke();
			return;
		}

		isTimerActive = true;
		totalTime = (minute * 60) + seconds;
		totalTime = totalTime - Time.deltaTime;

		minute = (int)totalTime / 60;
		seconds = totalTime - (minute * 60);

		OnTimerUpdated?.Invoke();

		if (IsTimeUp == true)
		{
			//ゲームオーバー処理
			ChangeSceneManager.SingletonInstance.GameOver();
		}
	}
}

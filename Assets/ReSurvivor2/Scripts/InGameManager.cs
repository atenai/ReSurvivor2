using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// インゲーム全体のマネージャー
/// </summary> 
public class InGameManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static InGameManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static InGameManager SingletonInstance => singletonInstance;

	[Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
	static bool isFirstLoad = true;
	public static bool IsFirstLoad
	{
		get { return isFirstLoad; }
		set { isFirstLoad = value; }
	}

	[Tooltip("エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題やキー入力でプレイヤーが移動してしまう問題の対処用")]
	bool isGamePlayReady = false;
	public bool IsGamePlayReady
	{
		get { return isGamePlayReady; }
		set { isGamePlayReady = value; }
	}

	[Header("ミッション")]

	[Tooltip("マスターデータのミッション情報")]
	[SerializeField] MasterMission masterMission;//２番目で作成したエクセルをスクリプト化したクラスの変数を作成する
	/// <summary>
	/// マスターデータのミッション情報のプロパティ
	/// </summary>
	public MasterMission MasterMission => masterMission;

	[Tooltip("ミッションがアクティブか？どうか")]
	bool isMissionActive = false;
	public bool IsMissionActive
	{
		get { return isMissionActive; }
		set { isMissionActive = value; }
	}

	/// <summary>ミッションエンドのコンピューター名</summary>
	EnumManager.ComputerTYPE endComputerName;
	/// <summary>ミッションエンドのコンピューター名のプロパティ </summary>
	public EnumManager.ComputerTYPE EndComputerName
	{
		get { return endComputerName; }
		set { endComputerName = value; }
	}

	[Tooltip("現在のミッションID")]
	[SerializeField] int currentMissionID = -1;
	public int CurrentMissionID
	{
		get { return currentMissionID; }
		set { currentMissionID = value; }
	}

	[Tooltip("ミッションID0のクリア状況")]
	bool missionID0 = false;
	[Tooltip("ミッションID1のクリア状況")]
	bool missionID1 = false;
	[Tooltip("ミッションID2のクリア状況")]
	bool missionID2 = false;

	[Header("キーアイテム")]

	[Tooltip("キーアイテム1がアクティブか？どうか")]
	[SerializeField] bool keyItem1 = false;
	public bool KeyItem1
	{
		get { return keyItem1; }
		set { keyItem1 = value; }
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

		Load();
	}

	/// <summary>
	/// セーブ
	/// </summary>
	void Save()
	{
		Debug.Log("<color=cyan>インゲームマネージャーセーブ</color>");
		ES3.Save<int>("Stage", SceneManager.GetActiveScene().name.Replace("Stage", "") != "" ? int.Parse(SceneManager.GetActiveScene().name.Replace("Stage", "")) : 0);
		ES3.Save<bool>("KeyItem1", keyItem1);
		ES3.Save<bool>("MissionID0", missionID0);
		ES3.Save<bool>("MissionID1", missionID1);
		ES3.Save<bool>("MissionID2", missionID2);
		Player.SingletonInstance.Save();
		PlayerCamera.SingletonInstance.Save();
	}

	/// <summary>
	/// ロード
	/// </summary>
	void Load()
	{
		if (isFirstLoad == false)
		{
			return;
		}
		isFirstLoad = false;

		Debug.Log("<color=purple>インゲームマネージャーロード</color>");
		keyItem1 = ES3.Load<bool>("KeyItem1", false);
		Debug.Log("<color=purple>キーアイテム1 : " + keyItem1 + "</color>");
		missionID0 = ES3.Load<bool>("MissionID0", false);
		Debug.Log("<color=purple>ミッションID0 : " + missionID0 + "</color>");
		missionID1 = ES3.Load<bool>("MissionID1", false);
		Debug.Log("<color=purple>ミッションID1 : " + missionID1 + "</color>");
		missionID2 = ES3.Load<bool>("MissionID2", false);
		Debug.Log("<color=purple>ミッションID2 : " + missionID2 + "</color>");
	}

	/// <summary>
	/// ミッション
	/// </summary>
	/// <param name="computerName"></param>
	public void Mission(EnumManager.ComputerTYPE computerName)
	{
		if (isMissionActive == true)//ミッション中の場合
		{
			if (computerName == EndComputerName)
			{
				Debug.Log("<color=blue>ミッション終了</color>");
				MissionIDUpdate();
				isMissionActive = false;
				currentMissionID = -1;
				ScreenUI.SingletonInstance.MapUI.SetEndComputerStageNumber(-1);
				Save();
				GameClear();
			}
			else
			{
				Debug.Log("<color=red>目的のコンピューターではない</color>");
			}
		}

		if (isMissionActive == false)//ミッション中でない場合
		{
			Save();
			ScreenUI.SingletonInstance.ShowComputerMenu();
		}
	}

	/// <summary>
	/// ミッションIDの状態を更新
	/// </summary>
	void MissionIDUpdate()
	{
		if (currentMissionID == 0)
		{
			missionID0 = true;
		}
		else if (currentMissionID == 1)
		{
			missionID1 = true;
		}
		else if (currentMissionID == 2)
		{
			missionID2 = true;
		}
	}

	/// <summary>
	/// ミッションがクリアされているかどうかの判定
	/// </summary>
	/// <param name="missionID"></param>
	/// <returns></returns>
	public bool IsMissionIDClearCheck(int missionID)
	{
		if (missionID == 0)
		{
			return missionID0;
		}
		else if (missionID == 1)
		{
			return missionID1;
		}
		else if (missionID == 2)
		{
			return missionID2;
		}
		else
		{
			Debug.LogError("ミッションIDの値が不正です。");
			return false;
		}
	}

	/// <summary>
	/// ゲームクリアー
	/// </summary>
	void GameClear()
	{
		if (missionID0 == true && missionID1 == true && missionID2 == true)
		{
			Debug.Log("<color=blue>ゲームクリアー</color>");
			InGameManager.IsFirstLoad = true;
			Player.IsFirstLoad = true;
			PlayerCamera.IsFirstLoad = true;
			//ゲームクリアー処理
			SceneManager.LoadScene("GameClear");
		}
	}

	/// <summary>
	/// ゲームオーバー
	/// </summary>
	public void GameOver()
	{
		Debug.Log("<color=red>ゲームオーバー</color>");
		InGameManager.IsFirstLoad = true;
		Player.IsFirstLoad = true;
		PlayerCamera.IsFirstLoad = true;
		//ゲームオーバー処理
		SceneManager.LoadScene("GameOver");
	}
}

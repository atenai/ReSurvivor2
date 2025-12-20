using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// インゲーム全体のマネージャー
/// </summary> 
public class InGameManager : MonoBehaviour
{
	/// <summary>
	/// シングルトンで作成（ゲーム中に１つのみにする）
	/// </summary>
	static InGameManager singletonInstance = null;
	/// <summary>
	/// シングルトンのプロパティ
	/// </summary>
	public static InGameManager SingletonInstance => singletonInstance;

	[Tooltip("エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題やキー入力でプレイヤーが移動してしまう問題の対処用")]
	bool isGamePlayReady = false;
	public bool IsGamePlayReady
	{
		get { return isGamePlayReady; }
		set { isGamePlayReady = value; }
	}

	[Tooltip("ミッションがアクティブか？どうか")]
	bool isMissionActive = false;
	public bool IsMissionActive
	{
		get { return isMissionActive; }
		set { isMissionActive = value; }
	}

	/// <summary>
	/// コンピュータータイプ
	/// </summary>
	public enum ComputerTYPE
	{
		Stage0Computer = 0,
		Stage1Computer = 1,
		Stage2Computer = 2,
		Stage3Computer = 3,
		Stage4Computer = 4,
	}

	/// <summary>
	/// ミッションエンドのコンピューター名
	/// </summary>
	ComputerTYPE endComputerName;
	/// <summary>
	/// ミッションエンドのコンピューター名のプロパティ
	/// </summary>
	public ComputerTYPE EndComputerName
	{
		get { return endComputerName; }
		set { endComputerName = value; }
	}

	[Tooltip("キーアイテム1がアクティブか？どうか")]
	[SerializeField] int keyItem1 = 0;//0 = false , 1 = true
	public int KeyItem1
	{
		get { return keyItem1; }
		set { keyItem1 = value; }
	}

	[Tooltip("マスターデータのミッション情報")]
	[SerializeField] MasterMission masterMission;//２番目で作成したエクセルをスクリプト化したクラスの変数を作成する
	/// <summary>
	/// マスターデータのミッション情報のプロパティ
	/// </summary>
	public MasterMission MasterMission => masterMission;

	[Tooltip("現在のミッションID")]
	[SerializeField] int currentMissionID = -1;
	public int CurrentMissionID
	{
		get { return currentMissionID; }
		set { currentMissionID = value; }
	}
	[Tooltip("ミッションID0のクリア状況")]
	int missionID0 = 0;//0 = false , 1 = true
	[Tooltip("ミッションID1のクリア状況")]
	int missionID1 = 0;//0 = false , 1 = true
	[Tooltip("ミッションID2のクリア状況")]
	int missionID2 = 0;//0 = false , 1 = true

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
	public void Save()
	{
		Debug.Log("<color=cyan>セーブ</color>");
		PlayerPrefs.SetInt("KeyItem1", keyItem1);
		PlayerPrefs.SetInt("MissionID0", missionID0);
		PlayerPrefs.SetInt("MissionID1", missionID1);
		PlayerPrefs.SetInt("MissionID2", missionID2);
		PlayerPrefs.Save();
	}

	/// <summary>
	/// ロード
	/// </summary>
	void Load()
	{
		Debug.Log("<color=purple>ロード</color>");

		if (PlayerPrefs.HasKey("KeyItem1") == true)
		{
			keyItem1 = PlayerPrefs.GetInt("KeyItem1", 0);
			Debug.Log("<color=purple>キーアイテム1 : " + keyItem1 + "</color>");
		}

		if (PlayerPrefs.HasKey("MissionID0") == true)
		{
			missionID0 = PlayerPrefs.GetInt("MissionID0", 0);
			Debug.Log("<color=purple>ミッションID0 : " + missionID0 + "</color>");
		}

		if (PlayerPrefs.HasKey("MissionID1") == true)
		{
			missionID1 = PlayerPrefs.GetInt("MissionID1", 0);
			Debug.Log("<color=purple>ミッションID1 : " + missionID1 + "</color>");
		}

		if (PlayerPrefs.HasKey("MissionID2") == true)
		{
			missionID2 = PlayerPrefs.GetInt("MissionID2", 0);
			Debug.Log("<color=purple>ミッションID2 : " + missionID2 + "</color>");
		}
	}

	/// <summary>
	/// ミッション検索リスト
	/// </summary>
	/// <param name="computerTYPE">コンピュータータイプ</param>
	/// <returns>ミッションリスト</returns>
	public List<MasterMissionEntity> MissionSerchList(ComputerTYPE computerTYPE)
	{
		//引数のコンピュータータイプからマスターデータのミッション情報を照らし合わせて、StartComputerNameと一致したコンピュータータイプの情報を全て取得する
		List<MasterMissionEntity> result = MasterMission.Sheet1.Where((MasterMissionEntity excelLine) => excelLine.StartComputerName == computerTYPE).ToList();

		if (result == null)
		{
			Debug.LogError("マスターデータのミッション情報がnull");
		}

		return result;
	}

	/// <summary>
	/// ミッション
	/// </summary>
	/// <param name="computerName"></param>
	public void Mission(ComputerTYPE computerName)
	{
		if (IsMissionActive == false)
		{
			Save();
			UI.SingletonInstance.ShowComputerMenu(computerName);
		}
		else if (IsMissionActive == true)
		{
			if (computerName == EndComputerName)
			{
				Debug.Log("<color=blue>ミッション終了</color>");
				if (currentMissionID == 0)
				{
					missionID0 = 1;
				}
				else if (currentMissionID == 1)
				{
					missionID1 = 1;
				}
				else if (currentMissionID == 2)
				{
					missionID2 = 1;
				}
				IsMissionActive = false;
				currentMissionID = -1;
				Save();
				GameClear();
			}
			else
			{
				Debug.Log("<color=red>目的のコンピューターではない</color>");
			}
		}
	}

	void GameClear()
	{
		if (missionID0 == 1 && missionID1 == 1 && missionID2 == 1)
		{
			Debug.Log("<color=yellow>ゲームクリアー</color>");
			//ゲームクリアー処理
			SceneManager.LoadScene("GameClear");
		}
	}

	void Start()
	{

	}

	void Update()
	{

	}
}

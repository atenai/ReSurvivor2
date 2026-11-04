using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ミッションを管理するマネージャークラス
/// </summary>
public class MissionManager : MonoBehaviour
{
    /// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static MissionManager singletonInstance = null;
    /// <summary>シングルトンのプロパティ</summary>
    public static MissionManager SingletonInstance => singletonInstance;

    [Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
    static bool isFirstLoad = true;
    public static bool IsFirstLoad
    {
        get { return isFirstLoad; }
        set { isFirstLoad = value; }
    }

    [Header("ミッション")]

    [Tooltip("マスターデータのミッション情報")]
    [SerializeField] MasterMission masterMission;//２番目で作成したエクセルをスクリプト化したクラスの変数を作成する
    /// <summary>
    /// マスターデータのミッション情報のプロパティ
    /// </summary>
    public MasterMission MasterMission => masterMission;

    //※ミッションの状態（↓の3つ）は、StartMission()とMissionReset()の中でだけ変える
    [Tooltip("ミッションがアクティブか？どうか")]
    bool isMissionActive = false;
    public bool IsMissionActive => isMissionActive;

    /// <summary>ミッションエンドのコンピューター名</summary>
    EnumManager.StageTYPE endComputerStageNumber;
    /// <summary>ミッションエンドのコンピューター名のプロパティ </summary>
    public EnumManager.StageTYPE EndComputerStageNumber => endComputerStageNumber;

    [Tooltip("現在のミッションID")]
    [SerializeField] int currentMissionID = -1;
    public int CurrentMissionID => currentMissionID;

    [Tooltip("ミッションID0のクリア状況")]
    [SerializeField] bool missionID0 = false;
    public bool MissionID0 => missionID0;
    [Tooltip("ミッションID1のクリア状況")]
    [SerializeField] bool missionID1 = false;
    public bool MissionID1 => missionID1;
    [Tooltip("ミッションID2のクリア状況")]
    [SerializeField] bool missionID2 = false;
    public bool MissionID2 => missionID2;

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
        Debug.Log("<color=cyan>ミッションマネージャーセーブ</color>");
        ES3.Save<bool>("MissionID0", missionID0);
        ES3.Save<bool>("MissionID1", missionID1);
        ES3.Save<bool>("MissionID2", missionID2);
    }

    /// <summary>
    /// ロード
    /// </summary>
    void Load()
    {
        Debug.Log("<color=red>isFirstLoad : " + isFirstLoad + "</color>");
        if (isFirstLoad == false)
        {
            return;
        }
        isFirstLoad = false;

        Debug.Log("<color=purple>ミッションマネージャーロード</color>");
        missionID0 = ES3.Load<bool>("MissionID0", false);
        Debug.Log("<color=purple>ミッションID0 : " + missionID0 + "</color>");
        missionID1 = ES3.Load<bool>("MissionID1", false);
        Debug.Log("<color=purple>ミッションID1 : " + missionID1 + "</color>");
        missionID2 = ES3.Load<bool>("MissionID2", false);
        Debug.Log("<color=purple>ミッションID2 : " + missionID2 + "</color>");
    }

    void Start()
    {
        Debug.Log("<color=green>ミッションID0 : " + missionID0 + "</color>");
        Debug.Log("<color=green>ミッションID1 : " + missionID1 + "</color>");
        Debug.Log("<color=green>ミッションID2 : " + missionID2 + "</color>");
    }

    /// <summary>
    /// ミッション検索リスト
    /// </summary>
    /// <param name="computerTYPE">コンピュータータイプ</param>
    /// <returns>ミッションリスト</returns>
    public List<MasterMissionEntity> MissionSerchList(EnumManager.StageTYPE currentComputerStageNumber)
    {
        //引数のコンピュータータイプからマスターデータのミッション情報を照らし合わせて、StartComputerStageNumberと一致したコンピュータータイプの情報を全て取得する
        List<MasterMissionEntity> result = MasterMission.Sheet1.Where((MasterMissionEntity excelLine) => excelLine.StartComputerStageNumber == currentComputerStageNumber).ToList();

        if (result == null)
        {
            Debug.Log("<color=red>マスターデータのミッション情報がnull</color>");
        }

        return result;
    }

    /// <summary>
    /// ミッション
    /// </summary>
    /// <param name="currentComputerStageNumber">現在のコンピューターステージ番号</param>
    public void Mission(EnumManager.StageTYPE currentComputerStageNumber)
    {
        Debug.Log("<color=red>ミッションID0 : " + missionID0 + "</color>");
        Debug.Log("<color=red>ミッションID1 : " + missionID1 + "</color>");
        Debug.Log("<color=red>ミッションID2 : " + missionID2 + "</color>");
        if (isMissionActive == true)//ミッション中の場合
        {
            if (currentComputerStageNumber == EndComputerStageNumber)
            {
                Debug.Log("<color=blue>ミッション終了</color>");
                MissionResult();
                MissionIDUpdate();
                MissionReset();
                SaveAndLoadDataManager.SingletonInstance.AllManagerSave();
                ChangeSceneManager.SingletonInstance.GameClear();
            }
            else
            {
                Debug.Log("<color=red>目的のコンピューターではない</color>");
            }
        }
        else if (isMissionActive == false)//ミッション中でない場合
        {
            SaveAndLoadDataManager.SingletonInstance.AllManagerSave();
            //コンピューターメニューのメール一覧を開いて、受けるミッションを選んでもらう（選んだらStartMission()が呼ばれる）
            ScreenUIManagerPresenter.SingletonInstance.ComputerMenuPresenter.Open();
        }
    }

    /// <summary>
    /// ミッション開始（コンピューターメニューで選んだミッションを始める）
    /// </summary>
    /// <param name="mission">始めるミッション</param>
    public void StartMission(MasterMissionEntity mission)
    {
        Debug.Log("<color=red>ミッション開始</color>");
        isMissionActive = true;
        currentMissionID = mission.MissionID;
        endComputerStageNumber = mission.EndComputerStageNumber;
        //制限時間を設定する
        TimerManager.SingletonInstance.Minute = mission.Minute;
        TimerManager.SingletonInstance.Seconds = mission.Seconds;
        //マップに、ゴールのコンピューターがあるステージを表示する
        ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.MapUI.SetEndComputerStageNumber((int)mission.EndComputerStageNumber);
    }

    /// <summary>
    /// ミッションのリザルト画面
    /// </summary>
    void MissionResult()
    {
        //リザルト画面を表示して、消えたらコンピューターメニューのメール一覧を開く
        ScreenUIManagerPresenter.SingletonInstance.ComputerMenuPresenter.ShowMissionResult();
    }

    /// <summary>
    /// ミッションIDの状態を更新
    /// </summary>
    void MissionIDUpdate()
    {
        Debug.Log("<color=blue>ミッションID0 : " + missionID0 + "</color>");
        Debug.Log("<color=blue>ミッションID1 : " + missionID1 + "</color>");
        Debug.Log("<color=blue>ミッションID2 : " + missionID2 + "</color>");
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
        Debug.Log("<color=yellow>ミッションID0 : " + missionID0 + "</color>");
        Debug.Log("<color=yellow>ミッションID1 : " + missionID1 + "</color>");
        Debug.Log("<color=yellow>ミッションID2 : " + missionID2 + "</color>");
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
    /// ミッションの状態をリセット
    /// </summary>
    void MissionReset()
    {
        isMissionActive = false;
        currentMissionID = -1;
        ScreenUIManagerPresenter.SingletonInstance.ScreenUIView.MapUI.SetEndComputerStageNumber(-1);
    }
}

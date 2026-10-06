using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン変更を管理するマネージャークラス
/// </summary>
public class ChangeSceneManager : MonoBehaviour
{
    /// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static ChangeSceneManager singletonInstance = null;
    /// <summary>シングルトンのプロパティ</summary>
    public static ChangeSceneManager SingletonInstance => singletonInstance;

    [Tooltip("初回ロードかどうか：なぜなら毎度ステージが切り替わる度にセーブデータをロードしてしまうと不具合が起きるため")]
    static bool isFirstLoad = true;
    public static bool IsFirstLoad
    {
        get { return isFirstLoad; }
        set { isFirstLoad = value; }
    }

    [Header("事前に読み込むシーン名")]
    [SerializeField] string gameClearSceneName = "GameClear";
    [SerializeField] string gameOverSceneName = "GameOver";

    bool isGameClearLoaded = false;
    bool isGameOverLoaded = false;
    bool isGameClearAndGameOverSceneSwitched = false;
    public bool IsGameClearAndGameOverSceneSwitched => isGameClearAndGameOverSceneSwitched;
    bool isGameClearTriggered = false;
    public bool IsGameClearTriggered => isGameClearTriggered;
    bool isGameOverTriggered = false;
    public bool IsGameOverTriggered => isGameOverTriggered;

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

        InitScene();
        Load();
    }

    /// <summary>
    /// セーブ
    /// </summary>
    public void Save()
    {
        Debug.Log("<color=cyan>チェンジシーンマネージャーセーブ</color>");
        ES3.Save<int>("Stage", SceneManager.GetActiveScene().name.Replace("Stage", "") != "" ? int.Parse(SceneManager.GetActiveScene().name.Replace("Stage", "")) : 0);
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

        //Debug.Log("<color=purple>チェンジシーンマネージャーロード</color>");
    }

    void InitScene()
    {
        isGameClearAndGameOverSceneSwitched = false;
        isGameClearLoaded = false;
        isGameOverLoaded = false;
        isGameClearTriggered = false;
        isGameOverTriggered = false;
    }

    /// <summary>
    /// ゲームクリアーシーンとゲームオーバーシーンを事前ロードする
    /// </summary>
    public void PreloadScenes()
    {
        //前のステージで読み込んだシーンはステージの切り替えで破棄されているので、読み込み済みフラグを戻す
        isGameClearLoaded = false;
        isGameOverLoaded = false;

        //読み込みはSceneLoadManagerが順番に行い、読み込んだ直後（Startが呼ばれる前）にルートオブジェクトを非表示にする
        SceneLoadManager.SingletonInstance.LoadSceneAdditive(gameClearSceneName, true, OnPreloadedScene);
        SceneLoadManager.SingletonInstance.LoadSceneAdditive(gameOverSceneName, true, OnPreloadedScene);
    }

    /// <summary>
    /// 事前ロードしたシーンの読み込みが終わった時の処理
    /// </summary>
    void OnPreloadedScene(Scene loadedScene)
    {
        if (loadedScene.name == gameClearSceneName)
        {
            isGameClearLoaded = true;
        }
        else if (loadedScene.name == gameOverSceneName)
        {
            isGameOverLoaded = true;
        }
    }

    void Start()
    {

    }

    void Update()
    {
        //ゲームクリアーシーンとゲームオーバーシーンに切り替えたら切り上げる
        if (isGameClearAndGameOverSceneSwitched == true)
        {
            return;
        }

        ShowGameClearScene();
        ShowGameOverScene();
    }

    /// <summary>
    /// ゲームクリアー画面へ切り替える
    /// </summary>
    void ShowGameClearScene()
    {
        if (isGameClearTriggered == false)
        {
            return;
        }

        if (isGameClearAndGameOverSceneSwitched == true)
        {
            return;
        }

        if (isGameClearLoaded == false)
        {
            Debug.Log("GameClearシーンがまだ読み込まれていません。");
            return;
        }

        isGameClearAndGameOverSceneSwitched = true;
        ShowScene(gameClearSceneName);
    }

    /// <summary>
    /// ゲームオーバー画面へ切り替える
    /// </summary>
    void ShowGameOverScene()
    {
        if (isGameOverTriggered == false)
        {
            return;
        }

        if (isGameClearAndGameOverSceneSwitched == true)
        {
            return;
        }

        if (isGameOverLoaded == false)
        {
            Debug.Log("GameOverシーンがまだ読み込まれていません。");
            return;
        }

        isGameClearAndGameOverSceneSwitched = true;
        ShowScene(gameOverSceneName);
    }

    /// <summary>
    /// 指定シーンを表示し、そのシーンをアクティブにする
    /// </summary>
    void ShowScene(string sceneName)
    {
        Scene targetScene = SceneManager.GetSceneByName(sceneName);

        if (targetScene.IsValid() == false || targetScene.isLoaded == false)
        {
            Debug.LogWarning(sceneName + " シーンが見つかりません。");
            return;
        }

        GameObject[] rootObjects = targetScene.GetRootGameObjects();

        for (int i = 0; i < rootObjects.Length; i++)
        {
            rootObjects[i].SetActive(true);
        }

        SceneManager.SetActiveScene(targetScene);
    }

    /// <summary>
    /// ゲームクリアー
    /// </summary>
    public void GameClear()
    {
        if (MissionManager.SingletonInstance.MissionID0 == true && MissionManager.SingletonInstance.MissionID1 == true && MissionManager.SingletonInstance.MissionID2 == true)
        {
            Debug.Log("<color=blue>ゲームクリアー</color>");
            ScreenUIManagerPresenter.SingletonInstance.HideComputerMenu();
            InGameManager.IsFirstLoad = true;
            ChangeSceneManager.IsFirstLoad = true;
            MissionManager.IsFirstLoad = true;
            PlayerManagerPresenter.IsFirstLoad = true;
            PlayerCameraManager.IsFirstLoad = true;
            //シーンを切り替える
            isGameClearTriggered = true;
            ScreenUIManagerPresenter.SingletonInstance.FadeOut();
        }
    }

    /// <summary>
    /// ゲームオーバー
    /// </summary>
    public void GameOver()
    {
        //シーン切り替え中はゲームオーバーを受け付けない（切り替え中にゲームオーバー画面が割り込むと、シーンの読み込みが止まってしまうため）
        if (SceneLoadManager.IsChangingScene == true)
        {
            Debug.Log("<color=red>シーン切り替え中のためゲームオーバーを受け付けません</color>");
            return;
        }

        Debug.Log("<color=red>ゲームオーバー</color>");
        InGameManager.IsFirstLoad = true;
        ChangeSceneManager.IsFirstLoad = true;
        MissionManager.IsFirstLoad = true;
        PlayerManagerPresenter.IsFirstLoad = true;
        PlayerCameraManager.IsFirstLoad = true;
        //シーンを切り替える
        isGameOverTriggered = true;
        ScreenUIManagerPresenter.SingletonInstance.FadeOut();
    }
}

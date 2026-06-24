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

		InitScene();
		Load();
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
	/// セーブ
	/// </summary>
	public void Save()
	{
		Debug.Log("<color=cyan>インゲームマネージャーセーブ</color>");
		ES3.Save<int>("Stage", SceneManager.GetActiveScene().name.Replace("Stage", "") != "" ? int.Parse(SceneManager.GetActiveScene().name.Replace("Stage", "")) : 0);
		ES3.Save<bool>("KeyItem1", keyItem1);
		MissionManager.SingletonInstance.Save();
		PlayerManager.SingletonInstance.Save();
		PlayerCameraManager.SingletonInstance.Save();
		ScreenUIManager.SingletonInstance.ShowSaveNowText();
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

		//Debug.Log("<color=purple>インゲームマネージャーロード</color>");
		keyItem1 = ES3.Load<bool>("KeyItem1", false);
		//Debug.Log("<color=purple>キーアイテム1 : " + keyItem1 + "</color>");
	}

	/// <summary>
	/// ゲームクリアーシーンとゲームオーバーシーンを事前ロードする
	/// </summary>
	public IEnumerator PreloadScenesCoroutine()
	{
		yield return StartCoroutine(LoadSceneAdditiveAndHide(gameClearSceneName));
		yield return StartCoroutine(LoadSceneAdditiveAndHide(gameOverSceneName));
	}

	/// <summary>
	/// シーンを Additive で非同期ロードし、読み込み後にルートオブジェクトを非表示にする
	/// </summary>
	IEnumerator LoadSceneAdditiveAndHide(string sceneName)
	{
		if (Application.CanStreamedLevelBeLoaded(sceneName) == false)
		{
			Debug.LogError(sceneName + " が Build Settings に含まれていないか、名前が一致しません。");
			yield break;
		}

		Debug.Log("Start loading scene: " + sceneName);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

		if (asyncOperation == null)
		{
			Debug.LogError("LoadSceneAsync failed for: " + sceneName);
			yield break;
		}

		// シンプルに AsyncOperation を待機し、その後シーンが実際にロードされたか確認する
		yield return asyncOperation;

		Scene loadedScene = SceneManager.GetSceneByName(sceneName);

		// AsyncOperation が完了してもシーン取得に時間が掛かる場合があるため、短いタイムアウト付きで確認する
		int checks = 0;
		while ((loadedScene.IsValid() == false || loadedScene.isLoaded == false) && checks < 60)
		{
			Debug.Log("Waiting for scene to become available: " + sceneName + " (check=" + checks + ")");
			checks++;
			yield return null;
			loadedScene = SceneManager.GetSceneByName(sceneName);
		}

		if (loadedScene.IsValid() && loadedScene.isLoaded)
		{
			GameObject[] rootObjects = loadedScene.GetRootGameObjects();

			for (int i = 0; i < rootObjects.Length; i++)
			{
				rootObjects[i].SetActive(false);
			}
		}
		else
		{
			Debug.LogWarning(sceneName + " のロードが完了していません。");
		}

		if (sceneName == gameClearSceneName)
		{
			isGameClearLoaded = true;
		}
		else if (sceneName == gameOverSceneName)
		{
			isGameOverLoaded = true;
		}

		Debug.Log("Finished loading scene: " + sceneName);
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
			InGameManager.IsFirstLoad = true;
			MissionManager.IsFirstLoad = true;
			PlayerManager.IsFirstLoad = true;
			PlayerCameraManager.IsFirstLoad = true;
			//シーンを切り替える
			isGameClearTriggered = true;
			ScreenUIManager.SingletonInstance.FadeOut();
		}
	}

	/// <summary>
	/// ゲームオーバー
	/// </summary>
	public void GameOver()
	{
		Debug.Log("<color=red>ゲームオーバー</color>");
		InGameManager.IsFirstLoad = true;
		MissionManager.IsFirstLoad = true;
		PlayerManager.IsFirstLoad = true;
		PlayerCameraManager.IsFirstLoad = true;
		//シーンを切り替える
		isGameOverTriggered = true;
		ScreenUIManager.SingletonInstance.FadeOut();
	}
}

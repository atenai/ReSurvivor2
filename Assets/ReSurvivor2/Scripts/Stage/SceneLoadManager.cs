using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// シーンの読み込みを一か所で管理するマネージャークラス（常駐）
/// ・読み込みは要求された順番に1つずつ行う
/// ・シーンの切り替え（Single読み込み）は同時に1つしか受け付けない
/// ・読み込みを要求したオブジェクトが途中で破棄されても、シーンの切り替えは必ず許可する
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static SceneLoadManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ（まだ無ければ常駐オブジェクトを作成する）</summary>
	public static SceneLoadManager SingletonInstance
	{
		get
		{
			if (singletonInstance == null)
			{
				GameObject managerGameObject = new GameObject("SceneLoadManager");
				singletonInstance = managerGameObject.AddComponent<SceneLoadManager>();
			}
			return singletonInstance;
		}
	}

	/// <summary>
	/// シーンを切り替え中か？（切り替えを受け付けてから次のシーンが読み込まれるまで）
	/// </summary>
	public static bool IsChangingScene => singletonInstance != null && singletonInstance.isChangingScene;

	/// <summary>切り替えの条件が揃わない場合に、待たずに切り替えるまでの秒数</summary>
	const float CanActivateTimeoutSeconds = 10.0f;

	/// <summary>
	/// 読み込み要求
	/// </summary>
	class LoadRequest
	{
		public string sceneName;
		public LoadSceneMode mode;
		/// <summary>読み込み進捗（0～1）を受け取る処理</summary>
		public UnityAction<float> onProgress;
		/// <summary>シーンを切り替えてよいか？（nullなら条件なし）</summary>
		public Func<bool> canActivate;
		/// <summary>シーンを切り替える直前の処理</summary>
		public UnityAction onBeforeActivate;
		/// <summary>追加読み込みしたシーンのルートオブジェクトを非表示にするか？</summary>
		public bool isHideRootObjects;
		/// <summary>追加読み込みが終わった時の処理</summary>
		public UnityAction<Scene> onLoaded;
	}

	readonly Queue<LoadRequest> requestQueue = new Queue<LoadRequest>();
	LoadRequest currentRequest = null;
	Coroutine processCoroutine = null;
	bool isChangingScene = false;

	void Awake()
	{
		if (singletonInstance == null || singletonInstance == this)
		{
			singletonInstance = this;
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnDestroy()
	{
		if (singletonInstance == this)
		{
			singletonInstance = null;
		}
	}

	/// <summary>
	/// シーンを切り替える（Single読み込み）
	/// </summary>
	/// <param name="sceneName">次のシーン名</param>
	/// <param name="onProgress">読み込み進捗（0～1）を受け取る処理</param>
	/// <param name="canActivate">シーンを切り替えてよいか？（nullなら条件なし）</param>
	/// <param name="onBeforeActivate">シーンを切り替える直前の処理</param>
	/// <returns>受け付けたらtrue、他のシーンに切り替え中などで受け付けなかったらfalse</returns>
	public bool LoadScene(string sceneName, UnityAction<float> onProgress = null, Func<bool> canActivate = null, UnityAction onBeforeActivate = null)
	{
		if (isChangingScene == true)
		{
			Debug.LogWarning("シーン切り替え中のため読み込みを受け付けません: " + sceneName);
			return false;
		}

		if (Application.CanStreamedLevelBeLoaded(sceneName) == false)
		{
			Debug.LogError(sceneName + " が Build Settings に含まれていないか、名前が一致しません。");
			return false;
		}

		//まだ始まっていない追加読み込みは今のシーン用なので取りやめる
		requestQueue.Clear();
		isChangingScene = true;
		requestQueue.Enqueue(new LoadRequest
		{
			sceneName = sceneName,
			mode = LoadSceneMode.Single,
			onProgress = onProgress,
			canActivate = canActivate,
			onBeforeActivate = onBeforeActivate,
		});
		StartProcess();
		return true;
	}

	/// <summary>
	/// シーンを追加で読み込む（Additive読み込み）
	/// 既に読み込まれているシーンは読み込み直さない
	/// </summary>
	/// <param name="sceneName">追加するシーン名</param>
	/// <param name="isHideRootObjects">読み込んだ直後（Startが呼ばれる前）にルートオブジェクトを非表示にするか？</param>
	/// <param name="onLoaded">読み込みが終わった時の処理</param>
	/// <returns>受け付けたらtrue、シーンを切り替え中などで受け付けなかったらfalse</returns>
	public bool LoadSceneAdditive(string sceneName, bool isHideRootObjects, UnityAction<Scene> onLoaded = null)
	{
		if (isChangingScene == true)
		{
			Debug.LogWarning("シーン切り替え中のため追加読み込みを受け付けません: " + sceneName);
			return false;
		}

		if (Application.CanStreamedLevelBeLoaded(sceneName) == false)
		{
			Debug.LogError(sceneName + " が Build Settings に含まれていないか、名前が一致しません。");
			return false;
		}

		requestQueue.Enqueue(new LoadRequest
		{
			sceneName = sceneName,
			mode = LoadSceneMode.Additive,
			isHideRootObjects = isHideRootObjects,
			onLoaded = onLoaded,
		});
		StartProcess();
		return true;
	}

	void StartProcess()
	{
		if (processCoroutine == null)
		{
			processCoroutine = StartCoroutine(ProcessRequestQueue());
		}
	}

	/// <summary>
	/// 読み込み要求を順番に1つずつ処理する
	/// </summary>
	IEnumerator ProcessRequestQueue()
	{
		while (0 < requestQueue.Count)
		{
			currentRequest = requestQueue.Dequeue();

			if (currentRequest.mode == LoadSceneMode.Single)
			{
				yield return StartCoroutine(ChangeScene(currentRequest));
			}
			else
			{
				yield return StartCoroutine(AddScene(currentRequest));
			}

			currentRequest = null;
		}

		processCoroutine = null;
	}

	/// <summary>
	/// シーンを切り替える
	/// </summary>
	IEnumerator ChangeScene(LoadRequest request)
	{
		Debug.Log("<color=red>シーン読み込み開始: " + request.sceneName + "</color>");
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Single);
		if (asyncOperation == null)
		{
			Debug.LogError("LoadSceneAsync failed for: " + request.sceneName);
			isChangingScene = false;
			yield break;
		}

		//読み込みが終わっても勝手にシーンが切り替わらないようにする
		asyncOperation.allowSceneActivation = false;

		//読み込み数値が0.9になるまで待つ（切り替えを許可するまでは0.9で止まる）
		float lastProgress = -1.0f;
		while (asyncOperation.progress < 0.9f)
		{
			if (asyncOperation.progress != lastProgress)
			{
				lastProgress = asyncOperation.progress;
				Debug.Log("<color=red>読み込み進捗: " + lastProgress * 100 + "%</color>");
			}
			InvokeProgress(request, asyncOperation.progress);
			yield return null;
		}

		//スライダーを満タンにして1フレーム表示する
		//※WaitForEndOfFrameはエディターでGameビューが描画されていないと再開しないため使わない
		Debug.Log("<color=red>読み込み進捗: 90%</color>");
		InvokeProgress(request, 1.0f);
		yield return null;

		//切り替えの条件が揃うまで待つ（揃わない場合も時間切れで切り替える）
		float waitStartTime = Time.realtimeSinceStartup;
		while (CanActivate(request) == false)
		{
			if (CanActivateTimeoutSeconds <= Time.realtimeSinceStartup - waitStartTime)
			{
				Debug.LogWarning("切り替えの条件が揃わないまま時間切れになったため切り替えます: " + request.sceneName);
				break;
			}
			yield return null;
		}

		InvokeBeforeActivate(request);

		//シーンを切り替える（要求したオブジェクトが破棄されていても必ず許可する）
		Debug.Log("<color=red>シーン切り替え: " + request.sceneName + "</color>");
		asyncOperation.allowSceneActivation = true;
		while (asyncOperation.isDone == false)
		{
			yield return null;
		}

		//通常は次のシーンが読み込まれた時点（OnSceneLoaded）で解除済み
		isChangingScene = false;
		Debug.Log("<color=red>シーン読み込み完了: " + request.sceneName + "</color>");
	}

	/// <summary>
	/// シーンを追加で読み込む
	/// </summary>
	IEnumerator AddScene(LoadRequest request)
	{
		//既に読み込まれている場合は読み込み直さない（二重読み込み防止）
		Scene loadedScene = FindLoadedScene(request.sceneName);
		if (loadedScene.IsValid() == true)
		{
			OnAdditiveSceneLoaded(request, loadedScene);
			yield break;
		}

		Debug.Log("Start loading scene: " + request.sceneName);
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(request.sceneName, LoadSceneMode.Additive);
		if (asyncOperation == null)
		{
			Debug.LogError("LoadSceneAsync failed for: " + request.sceneName);
			yield break;
		}

		while (asyncOperation.isDone == false)
		{
			yield return null;
		}

		Debug.Log("Finished loading scene: " + request.sceneName);
	}

	/// <summary>
	/// シーンが読み込まれた時の処理（読み込んだシーンのAwakeの後、Startの前に呼ばれる）
	/// </summary>
	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (currentRequest == null || currentRequest.sceneName != scene.name || currentRequest.mode != mode)
		{
			return;
		}

		if (mode == LoadSceneMode.Single)
		{
			//次のシーンが読み込まれたので切り替え中を解除する（ここから先は次のシーンの処理を受け付ける）
			isChangingScene = false;
		}
		else
		{
			OnAdditiveSceneLoaded(currentRequest, scene);
		}
	}

	/// <summary>
	/// 追加読み込みが終わった時の処理
	/// </summary>
	void OnAdditiveSceneLoaded(LoadRequest request, Scene scene)
	{
		if (request.isHideRootObjects == true)
		{
			//Startが呼ばれる前に非表示にして、表示するまでシーンの処理が動かないようにする
			GameObject[] rootObjects = scene.GetRootGameObjects();
			for (int i = 0; i < rootObjects.Length; i++)
			{
				rootObjects[i].SetActive(false);
			}
		}

		try
		{
			request.onLoaded?.Invoke(scene);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	/// <summary>
	/// 読み込み済みのシーンを名前で探す
	/// </summary>
	Scene FindLoadedScene(string sceneName)
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.name == sceneName && scene.isLoaded == true)
			{
				return scene;
			}
		}
		return new Scene();
	}

	//要求元の処理で例外が起きても読み込みが止まらないようにする

	void InvokeProgress(LoadRequest request, float progress)
	{
		try
		{
			request.onProgress?.Invoke(progress);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	bool CanActivate(LoadRequest request)
	{
		if (request.canActivate == null)
		{
			return true;
		}

		try
		{
			return request.canActivate();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return true;
		}
	}

	void InvokeBeforeActivate(LoadRequest request)
	{
		try
		{
			request.onBeforeActivate?.Invoke();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}
}

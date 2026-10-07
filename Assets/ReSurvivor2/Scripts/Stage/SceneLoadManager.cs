using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// インゲームのシーンの読み込みを一か所で管理するマネージャークラス
/// ・読み込みは要求された順番に1つずつ行う
/// ・シーンの切り替え（Single読み込み）は同時に1つしか受け付けない
/// ・読み込みを要求したオブジェクトが途中で破棄されても、シーンの切り替えは必ず許可する
/// ・他のインゲームのマネージャーと同じく各ステージのヒエラルキーに置き、アウトゲームに移動した時に OutGameBase で破棄する
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static SceneLoadManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static SceneLoadManager SingletonInstance => singletonInstance;

	/// <summary>
	/// シーンを切り替え中か？（切り替えを受け付けてから次のシーンが読み込まれるまで）
	/// </summary>
	public static bool IsChangingScene => singletonInstance.isChangingScene;

	[Tooltip("allowSceneActivation が false の時、読み込みが終わると progress はこの値で止まる（ここから先はシーンを切り替えると進む）")]
	const float ActivationReadyProgress = 0.9f;
	[Tooltip("スライダーを満タンにする時の進捗")]
	const float FullProgress = 1.0f;
	[Tooltip("読み込み進捗をまだ一度もログに出していないことを表す値（進捗は 0〜0.9 なので、この値なら最初の 0% も必ずログに出る）")]
	const float NotLoggedProgress = -1.0f;
	[Tooltip("ログに出す時に進捗（0〜1）をパーセントにする倍率")]
	const float PercentRate = 100.0f;

	/// <summary>
	/// 読み込み要求
	/// </summary>
	class LoadRequest
	{
		public string sceneName;
		public LoadSceneMode mode;
		/// <summary>読み込み進捗（0～1）を受け取る処理</summary>
		public UnityAction<float> onProgress;
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
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
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

	/// <summary>
	/// シーンを切り替える（Single読み込み）
	/// </summary>
	/// <param name="sceneName">次のシーン名</param>
	/// <param name="onProgress">読み込み進捗（0～1）を受け取る処理</param>
	/// <param name="onBeforeActivate">シーンを切り替える直前の処理</param>
	/// <returns>受け付けたらtrue、他のシーンに切り替え中で受け付けなかったらfalse</returns>
	public bool LoadScene(string sceneName, UnityAction<float> onProgress = null, UnityAction onBeforeActivate = null)
	{
		if (isChangingScene == true)
		{
			Debug.LogWarning("シーン切り替え中のため読み込みを受け付けません: " + sceneName);
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
		//進捗が変わった時だけログに出す（lastProgress は前回ログに出した進捗）
		float lastProgress = NotLoggedProgress;
		while (asyncOperation.progress < ActivationReadyProgress)
		{
			if (asyncOperation.progress != lastProgress)
			{
				lastProgress = asyncOperation.progress;
				Debug.Log("<color=red>読み込み進捗: " + lastProgress * PercentRate + "%</color>");
			}
			InvokeProgress(request, asyncOperation.progress);
			yield return null;
		}

		//スライダーを満タンにして1フレーム表示する
		//※WaitForEndOfFrameはエディターでGameビューが描画されていないと再開しないため使わない
		Debug.Log("<color=red>読み込み進捗: " + ActivationReadyProgress * PercentRate + "%</color>");
		InvokeProgress(request, FullProgress);
		yield return null;

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

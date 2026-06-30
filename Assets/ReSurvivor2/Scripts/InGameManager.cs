using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インゲーム全体のマネージャークラス
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

	/// <summary> ポーズ中か？</summary>
	bool isPause = false;
	public bool IsPause => isPause;

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
	public void Save()
	{
		Debug.Log("<color=cyan>インゲームマネージャーセーブ</color>");
		ES3.Save<bool>("KeyItem1", keyItem1);
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

	void Update()
	{
		//ゲームクリアーシーンとゲームオーバーシーンに切り替えたら切り上げる
		if (ChangeSceneManager.SingletonInstance.IsGameClearAndGameOverSceneSwitched == true)
		{
			return;
		}

		//ゲームクリアーとゲームオーバーをトリガーのどちらかが起動したら切り上げる
		if (ChangeSceneManager.SingletonInstance.IsGameClearTriggered == true || ChangeSceneManager.SingletonInstance.IsGameOverTriggered == true)
		{
			return;
		}

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			PlayerManager.SingletonInstance.ResetMove();
			PlayerManager.SingletonInstance.PlayerModel.ResetMoveAnimation();
			return;
		}
		//↓ロード中に動かせない処理

		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("XInput Start"))
		{
			isPause = isPause ? false : true;
			ScreenUIManager.SingletonInstance.InputUpdate(isPause);
		}

		ScreenUIManager.SingletonInstance.BeforeUpdate1();

		//ポーズ中は切り上げる
		if (isPause == true)
		{
			return;
		}

		ScreenUIManager.SingletonInstance.BeforeUpdate2();

		//コンピュータを使用中は切り上げる
		if (ScreenUIManager.SingletonInstance.ScreenUIPresenter.IsComputerMenuActive == true)
		{
			PlayerManager.SingletonInstance.ResetMove();
			PlayerManager.SingletonInstance.PlayerModel.ResetMoveAnimation();
			return;
		}

		PlayerManager.SingletonInstance.AfterUpdate();
		PlayerManager.SingletonInstance.PlayerModel.AfterUpdate();
		PlayerManager.SingletonInstance.PlayerUI.AfterUpdate();
		PlayerCameraManager.SingletonInstance.AfterUpdate();
		TimerManager.SingletonInstance.AfterUpdate();
		ScreenUIManager.SingletonInstance.AfterUpdate();
	}

	void FixedUpdate()
	{
		//ゲームクリアーシーンとゲームオーバーシーンに切り替えたら切り上げる
		if (ChangeSceneManager.SingletonInstance.IsGameClearAndGameOverSceneSwitched == true)
		{
			return;
		}

		//ゲームクリアーとゲームオーバーをトリガーのどちらかが起動したら切り上げる
		if (ChangeSceneManager.SingletonInstance.IsGameClearTriggered == true || ChangeSceneManager.SingletonInstance.IsGameOverTriggered == true)
		{
			return;
		}

		PlayerCameraManager.SingletonInstance.AlwaysFixedUpdate();

		//↑ロード中に動かせる処理
		if (InGameManager.SingletonInstance.IsGamePlayReady == false)
		{
			return;
		}
		//↓ロード中に動かせない処理

		PlayerManager.SingletonInstance.AfterFixedUpdate();
		PlayerCameraManager.SingletonInstance.AfterFixedUpdate();
	}
}

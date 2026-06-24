using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		ChangeSceneManager.SingletonInstance.Save();
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
}

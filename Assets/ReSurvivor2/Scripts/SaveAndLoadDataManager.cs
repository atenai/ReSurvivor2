using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// セーブとロードを管理するマネージャークラス
/// </summary>
public class SaveAndLoadDataManager : MonoBehaviour
{
    /// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
    static SaveAndLoadDataManager singletonInstance = null;
    /// <summary>シングルトンのプロパティ</summary>
    public static SaveAndLoadDataManager SingletonInstance => singletonInstance;

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

        // AllManagerLoad();
    }

    /// <summary>
    /// セーブ
    /// </summary>
    public void AllManagerSave()
    {
        Debug.Log("<color=cyan>セーブアンドロードデータマネージャーセーブ</color>");
        ChangeSceneManager.SingletonInstance.Save();
        InGameManager.SingletonInstance.Save();
        MissionManager.SingletonInstance.Save();
        PlayerManager.SingletonInstance.Save();
        PlayerCameraManager.SingletonInstance.Save();

        //SaveNowのテキストを表示する
        ScreenUIManager.SingletonInstance.ScreenUIPresenter.ShowSaveNowText();
    }

    /// <summary>
    /// ロード
    /// </summary>
    // public void AllManagerLoad()
    // {
    //     //Debug.Log("<color=purple>セーブアンドロードデータマネージャーロード</color>");
    // }

    void Start()
    {

    }

    void Update()
    {

    }
}

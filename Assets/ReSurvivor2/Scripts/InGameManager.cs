using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インゲーム全体のマネージャー
/// </summary> 
public class InGameManager : MonoBehaviour
{
    //シングルトンで作成（ゲーム中に１つのみにする）
    static InGameManager singletonInstance = null;
    public static InGameManager SingletonInstance => singletonInstance;

    [Tooltip("エディターで実行ロード時にマウスの座標が入力されてカメラが動いてしまう問題やキー入力でプレイヤーが移動してしまう問題の対処用")]
    bool isGamePlayReady = false;
    public bool IsGamePlayReady
    {
        get { return isGamePlayReady; }
        set { isGamePlayReady = value; }
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
    }

    void Start()
    {

    }

    void Update()
    {

    }
}

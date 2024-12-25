using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Computer : MonoBehaviour
{
    [Tooltip("このコンピューターの名前")]
    [SerializeField] InGameManager.ComputerTYPE thisComputerName;

    void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("<color=yellow>プレイヤー</color>");
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("<color=green>Fキー</color>");
                Save();

                if (InGameManager.SingletonInstance.IsMissionActive == false)
                {
                    Debug.Log("<color=red>ミッション開始</color>");
                    InGameManager.SingletonInstance.IsMissionActive = true;

                    //↓ここをミッションごとに変えるようにする必要がある
                    //各ComputerTYPEに紐づいたミッションリストを取得して、そこから選択されたミッション内容を↓に反映すればいい
                    InGameManager.SingletonInstance.TargetComputerName = InGameManager.ComputerTYPE.Stage4Computer;
                    Player.SingletonInstance.Minute = 10;
                    Player.SingletonInstance.Seconds = 30.0f;
                }
                else if (InGameManager.SingletonInstance.IsMissionActive == true && thisComputerName == InGameManager.SingletonInstance.TargetComputerName)
                {
                    Debug.Log("<color=blue>ミッション終了</color>");
                    InGameManager.SingletonInstance.IsMissionActive = false;

                    //ゲームクリアー処理
                    SceneManager.LoadScene("GameClear");
                }
            }
        }
    }

    /// <summary>
    /// セーブ
    /// </summary>
    private void Save()
    {
        Debug.Log("<color=cyan>セーブ</color>");
        PlayerPrefs.SetInt("KeyItem1", InGameManager.SingletonInstance.KeyItem1);
        PlayerPrefs.Save();
    }
}

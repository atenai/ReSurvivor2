using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Computer : MonoBehaviour
{
    [Tooltip("このコンピューターで既にミッション関連の処理を行ったか？")]
    [SerializeField] bool isAlreadyReceivedMissionThisComputer = false;

    void OnTriggerStay(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("<color=yellow>プレイヤー</color>");
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("<color=green>Fキー</color>");
                if (InGameManager.SingletonInstance.IsMissionActive == false && isAlreadyReceivedMissionThisComputer == false)
                {
                    //ミッション開始
                    Debug.Log("<color=red>ミッション開始</color>");
                    InGameManager.SingletonInstance.IsMissionActive = true;
                    isAlreadyReceivedMissionThisComputer = true;

                    Player.SingletonInstance.Minute = 10;
                    Player.SingletonInstance.Seconds = 3.0f;
                }
                else if (InGameManager.SingletonInstance.IsMissionActive == true && isAlreadyReceivedMissionThisComputer == false)
                {
                    //ミッション終了
                    Debug.Log("<color=blue>ミッション終了</color>");
                    InGameManager.SingletonInstance.IsMissionActive = false;
                    isAlreadyReceivedMissionThisComputer = true;

                    //ゲームクリアー処理
                    SceneManager.LoadScene("GameClear");
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// アウトゲームのマネージャー
/// </summary>
public class OutGameManagerBase : MonoBehaviour
{
    [SerializeField] protected string sceneName;
    [SerializeField] protected Slider sliderLoading;
    bool isLoadOnce = false;

    protected void Start()
    {
        if (InGameManager.SingletonInstance != null)
        {
            Destroy(InGameManager.SingletonInstance.gameObject);
        }

        if (Player.SingletonInstance != null)
        {
            Destroy(Player.SingletonInstance.gameObject);
        }

        if (PlayerCamera.SingletonInstance != null)
        {
            Destroy(PlayerCamera.SingletonInstance.gameObject);
        }

        if (UI.SingletonInstance != null)
        {
            Destroy(UI.SingletonInstance.gameObject);
        }
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Load(sceneName);
        }
    }

    /// <summary>
    /// ロード
    /// </summary> 
    protected void Load(string nextSceneName)
    {
        if (isLoadOnce == false)
        {
            isLoadOnce = true;
            StartCoroutine(LoadScene(nextSceneName));
        }
    }

    /// <summary>
    /// シーンをロードする
    /// </summary>
    IEnumerator LoadScene(string nextSceneName)
    {
        //スライダーの値を最低にする
        sliderLoading.value = float.MinValue;

        //シーンをロード
        AsyncOperation async = SceneManager.LoadSceneAsync(nextSceneName);
        //シーンが勝手に切り替わらないようにする
        async.allowSceneActivation = false;
        //シーンをロードするまでのループ処理
        while (async.isDone == false)
        {
            //ロード数値をスライダーに反映
            sliderLoading.value = async.progress;

            //ロード数値が0.9より同じかそれ以上大きくなったら中身を実行する
            if (0.9f <= async.progress)
            {
                //スライダーの値を最大にする
                sliderLoading.value = float.MaxValue;

                //フレームのラストまで待つ
                yield return new WaitForEndOfFrame();

                //シーンを切り替える
                async.allowSceneActivation = true;
            }

            //1フレーム待つ
            yield return null;
        }
    }
}

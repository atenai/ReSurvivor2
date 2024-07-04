using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] string sceneName;
    [SerializeField] public Slider sliderLoading;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(LoadScene());
        }
    }

    /// <summary>
    /// シーンをロードする
    /// </summary>
    IEnumerator LoadScene()
    {
        //スライダーの値を最低にする
        sliderLoading.value = float.MinValue;

        //シーンをロード
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
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

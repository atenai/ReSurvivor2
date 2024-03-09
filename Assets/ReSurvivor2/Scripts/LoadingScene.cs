using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] Slider slider_Loading;
    [SerializeField] string sceneName;
    [SerializeField] GameObject spawnPos;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            SetPlayerSpawnPos(collider);
            StartCoroutine(LoadScene());
        }
    }

    /// <summary>
    /// シーン遷移した際にプレイヤーのスポーン位置を設定
    /// </summary>
    void SetPlayerSpawnPos(Collider collider)
    {
        if (spawnPos != null)
        {
            collider.gameObject.transform.position = spawnPos.transform.position;
        }
        else
        {
            collider.gameObject.transform.position = new Vector3(0, 1, 0);
        }
    }

    /// <summary>
    /// シーンをロードする
    /// </summary>
    IEnumerator LoadScene()
    {
        //スライダーの値を最低にする
        slider_Loading.value = float.MinValue;
        //ロードUIをOnにする
        UI.singletonInstance.panelLoading.SetActive(true);

        //シーンをロード
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        //シーンが勝手に切り替わらないようにする
        async.allowSceneActivation = false;
        //シーンをロードするまでのループ処理
        while (async.isDone == false)
        {
            //ロード数値をスライダーに反映
            slider_Loading.value = async.progress;

            //ロード数値が0.9より同じかそれ以上大きくなったら中身を実行する
            if (0.9f <= async.progress)
            {
                //スライダーの値を最大にする
                slider_Loading.value = float.MaxValue;

                //フレームのラストまで待つ
                yield return new WaitForEndOfFrame();

                //ロードUIをOffにする
                UI.singletonInstance.panelLoading.SetActive(false);
                //シーンを切り替える
                async.allowSceneActivation = true;
            }

            //1フレーム待つ
            yield return null;
        }
    }
}
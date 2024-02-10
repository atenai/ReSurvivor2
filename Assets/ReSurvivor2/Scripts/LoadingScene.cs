using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] GameObject panel_Loading;
    [SerializeField] Slider slider_Loading;
    [SerializeField] string sceneName;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        panel_Loading.SetActive(true);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        while (!async.isDone)
        {
            slider_Loading.value = async.progress;
            yield return null;
        }
    }
}
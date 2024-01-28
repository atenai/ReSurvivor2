using UnityEngine;
using UnityEditor;

public class ScreenSetting : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_ANDROID//端末がAndroidだった場合の処理

#endif //終了

#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理

        Screen.SetResolution(1920, 1080, true, 60);

#endif //終了

#if UNITY_STANDALONE_WIN//端末がPCだった場合の処理
        //マウスカーソルを消す
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif //終了

    }

    void Update()
    {
#if UNITY_EDITOR//Unityエディター上での処理
        //Cキーでマウスカーソルを出す
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
#endif //終了   

#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理
        //Escapeキーでゲーム終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();//ゲーム終了
        }
#endif //終了   
    }

    void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }
}

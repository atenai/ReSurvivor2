using UnityEngine;
using UnityEditor;
using UnityEngine.ResourceManagement.ResourceProviders;

public class ScreenSetting : MonoBehaviour
{
    [SerializeField] bool isCursor = false;

    void Awake()
    {
#if UNITY_ANDROID//端末がAndroidだった場合の処理

#endif //終了

#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理

        Screen.SetResolution(1920, 1080, true, 60);

#endif //終了

#if UNITY_STANDALONE_WIN//端末がPCだった場合の処理
        CursorActive();
#endif //終了

    }

    void Update()
    {
#if UNITY_EDITOR//Unityエディター上での処理
        //Tキーでマウスカーソルを出すorマウスカーソルを消す
        if (Input.GetKeyDown(KeyCode.T))
        {
            isCursor = isCursor ? false : true;
            CursorActive();
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

    void CursorActive()
    {
        if (isCursor == false)
        {
            //マウスカーソルを消す
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (isCursor == true)
        {
            //マウスカーソルを出す
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
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

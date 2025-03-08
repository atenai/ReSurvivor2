using UnityEngine;
using UnityEditor;

public class ScreenSetting : MonoBehaviour
{
	[Tooltip("マウスカーソルのオン/オフ")]
	[SerializeField] bool isCursor = false;

	void Awake()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN//Unityエディター上または端末がPCだった場合の処理

		Screen.SetResolution(1920, 1080, true, 60);
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
		}
		CursorActive();

#endif //終了    
	}

	/// <summary>
	/// マウスカーソルのオン/オフ処理 
	/// </summary>
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
}

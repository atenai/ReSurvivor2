using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XInputの状態を管理するクラス
/// </summary>
public class XInputManager : MonoBehaviour
{
	/// <summary> シングルトンで作成（ゲーム中に１つのみにする）</summary>
	static XInputManager singletonInstance = null;
	/// <summary>シングルトンのプロパティ</summary>
	public static XInputManager SingletonInstance => singletonInstance;

	[Tooltip("XInputのDPadハンドラー")]
	XInputDPadHandler xInputDPadHandler = new XInputDPadHandler();
	/// <summary> XInputのDPadハンドラー </summary>
	public XInputDPadHandler XInputDPadHandler => xInputDPadHandler;

	[Tooltip("XInputのトリガーハンドラー")]
	XInputTriggerHandler xInputTriggerHandler = new XInputTriggerHandler();
	/// <summary> XInputのトリガーハンドラー</summary>
	public XInputTriggerHandler XInputTriggerHandler => xInputTriggerHandler;

	void Awake()
	{
		//staticな変数instanceはメモリ領域は確保されていますが、初回では中身が入っていないので、中身を入れます。
		if (singletonInstance == null)
		{
			singletonInstance = this;//thisというのは自分自身のインスタンスという意味になります。この場合、PlayerCameraのインスタンスという意味になります。
			DontDestroyOnLoad(this.gameObject);//シーンを切り替えた時に破棄しない
		}
		else
		{
			Destroy(this.gameObject);//中身がすでに入っていた場合、自身のインスタンスがくっついているゲームオブジェクトを破棄します。
		}
	}

	void Update()
	{
		UpdateXInputDPad();
		UpdateXInputTrigger();
	}

	/// <summary>
	/// XInputのDPad状態を更新
	/// </summary>
	void UpdateXInputDPad()
	{
		// 状態更新
		xInputDPadHandler.UpdateDPadDown();
		xInputDPadHandler.UpdateDPadHold();
		xInputDPadHandler.UpdateDPadUp();
	}

	/// <summary>
	/// XInputのトリガー状態を更新
	/// </summary>
	void UpdateXInputTrigger()
	{
		xInputTriggerHandler.Update(Input.GetAxis("XInput RT"));
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XInputManager : MonoBehaviour
{
	//シングルトンで作成（ゲーム中に１つのみにする）
	static XInputManager singletonInstance = null;
	public static XInputManager SingletonInstance => singletonInstance;

	[Tooltip("XInputのDPadハンドラー")]
	XInputDPadHandler xInputDPadHandler = new XInputDPadHandler();
	/// <summary>
	/// XInputのDPadハンドラー
	/// </summary>
	public XInputDPadHandler XInputDPadHandler => xInputDPadHandler;

	[Tooltip("XInputのトリガーハンドラー")]
	XInputTriggerHandler xInputTriggerHandler = new XInputTriggerHandler();
	/// <summary>
	/// XInputのトリガーハンドラー
	/// </summary>
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

	void Start()
	{

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
		// DPad軸を取得（InputManagerで設定済み or 軸番号で直接）
		float dpadX = Input.GetAxis("XInput DPad Left&Right");
		float dpadY = Input.GetAxis("XInput DPad Up&Down");

		// 状態更新
		xInputDPadHandler.Update(dpadX, dpadY);
	}

	/// <summary>
	/// XInputのトリガー状態を更新
	/// </summary>
	void UpdateXInputTrigger()
	{
		xInputTriggerHandler.Update(Input.GetAxis("XInput RT"));
	}
}

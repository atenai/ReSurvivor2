using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// クレジット
/// </summary> 
public class Credits : OutGameBase
{
	/// <summary>
	/// スクロールビュー
	/// </summary>
	[SerializeField] ScrollRect scrollRect;

	/// <summary>
	/// XInputのDPad管理クラス
	/// </summary>
	XInputDPadHandler xInputDPadHandler = new XInputDPadHandler();

	new void Start()
	{
		base.Start();

		scrollRect.verticalNormalizedPosition = 1.0f;
	}


	new void Update()
	{
		base.Update();

		float scroll = scrollRect.verticalNormalizedPosition;

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || xInputDPadHandler.UpDown)
		{
			scroll = 2;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || xInputDPadHandler.DownDown)
		{
			scroll = -1;
		}
		else
		{
			scroll = scrollRect.verticalNormalizedPosition;
		}

		scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, scroll, 0.01f);

		UpdateXInputDPad();
	}

	/// <summary>
	/// XInputのDPadの状態更新
	/// </summary>
	void UpdateXInputDPad()
	{
		// DPad軸を取得（InputManagerで設定済み or 軸番号で直接）
		float dpadX = Input.GetAxis("XInput DPad Left&Right");
		float dpadY = Input.GetAxis("XInput DPad Up&Down");

		// 状態更新
		xInputDPadHandler.Update(dpadX, dpadY);
	}
}

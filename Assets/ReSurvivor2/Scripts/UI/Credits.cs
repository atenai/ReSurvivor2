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

	new void Start()
	{
		base.Start();

		scrollRect.verticalNormalizedPosition = 1.0f;
	}


	new void Update()
	{
		base.Update();

		float scroll = scrollRect.verticalNormalizedPosition;

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || XInputManager.SingletonInstance.XInputDPadHandler.UpDown)
		{
			scroll = 2;
		}
		else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || XInputManager.SingletonInstance.XInputDPadHandler.DownDown)
		{
			scroll = -1;
		}
		else
		{
			scroll = scrollRect.verticalNormalizedPosition;
		}

		scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, scroll, 0.01f);
	}
}
